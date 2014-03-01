using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenLibrary.Extension;
using OpenLibrary.Annotation;

namespace OpenLibrary.Document
{
	/// <summary>
	/// Processing CSV document
	/// </summary>
	public static class Csv
	{
		/// <summary>
		/// Column delimiter
		/// </summary>
		public static string Delimiter { get; private set; }

		/// <summary>
		/// Regex parser CSV
		/// </summary>
		public static string RegexParser
		{
			get
			{
				return @"[" + Delimiter + "](?=(?:[^\"]|\"[^\"]*\")*$)";
			}
		}

		/// <summary>
		/// Digunakan untuk menggenerate string CSV
		/// </summary>
		/// <param name="data">data yang akan diexport</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="dateFormat">format tanggal (jika ada)</param>
		/// <returns>string</returns>
		public static string SerializeToCsv<T>(this T data, List<MappingOption> exportOption = null, string dateFormat = "")
			where T : class
		{
			//buat opsi export jika tidak ada
			if (exportOption == null)
				exportOption = typeof(T).ExtractField();
			exportOption = (from option in exportOption
							orderby option.Sequence.HasValue descending, option.Sequence ascending
							select option).ToList();
			var line = new StringBuilder();
			bool firstColumn = true;
			foreach (var column in exportOption)
			{
				//selain kolom pertama, harus ditambahkan pemisah kolom
				if (!firstColumn)
					line.Append(Delimiter);
				var value = data.GetType().GetProperty(column.Field).GetValue(data, null);
				//convert semuanya ke string
				string csv = value.To<string>(dateFormat: dateFormat).Trim();
				//karakter yg wajib diescape
				if (csv.IndexOfAny(new[] { '"', Delimiter[0] }) != -1)
					csv = string.Format("\"{0}\"", csv.Replace("\"", "\"\""));
				line.Append(csv);
				firstColumn = false;
			}
			return line.ToString();
		}

		/// <summary>
		/// Export data to CSV
		/// </summary>
		/// <param name="data">data yang akan diexport</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="dateFormat">format tanggal (jika ada)</param>
		/// <returns>stream</returns>
		public static Stream ToCsv<T>(IEnumerable<T> data, List<MappingOption> exportOption = null, string delimiter = ",", string dateFormat = "")
			where T : class
		{
			List<T> objects = data.ToList();
			var output = new StreamWriter(new MemoryStream());
			if (objects.Count > 0)
			{
				//buat opsi export jika tidak ada
				if (exportOption == null)
					exportOption = (from option in typeof(T).ExtractField()
									orderby option.Sequence.HasValue descending, option.Sequence ascending
									select option).ToList();					
				//header row
				bool firstColumn = true;
				var line = new StringBuilder();
				Delimiter = delimiter;
				foreach (var option in exportOption)
				{
					//tambahkan pemisah kolom untuk selain awal kolom
					if (!firstColumn)
						line.Append(delimiter);
					//karakter yg wajib diescape
					if (option.Caption.IndexOfAny(new[] { '"', Delimiter[0] }) != -1)
						line.AppendFormat("\"{0}\"", option.Caption.Replace("\"", "\"\""));
					else
						line.Append(option.Caption);
					firstColumn = false;
				}
				output.WriteLine(line.ToString());

				//isi data
				foreach (var rowData in objects)
					output.WriteLine(rowData.SerializeToCsv(exportOption, dateFormat));
			}
			output.Flush();
			return output.BaseStream;
		}

		/// <summary>
		/// Buat export data ke csv
		/// </summary>
		/// <param name="data">data yang akan diexport</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="filename">nama file output</param>
		/// <param name="dateFormat">format tanggal (jika ada)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		public static void ToCsv<T>(IEnumerable<T> data, string filename, List<MappingOption> exportOption = null, string delimiter = ",", string dateFormat = "")
			where T : class
		{
			var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
			var output = ToCsv<T>(data, exportOption, delimiter, dateFormat);
			output.Seek(0, SeekOrigin.Begin);
			output.CopyTo(fileStream);
			fileStream.Flush();
			fileStream.Close();
		}

		/// <summary>
		/// Digunakan untuk memparsing string CSV ke object.
		/// </summary>
		/// <typeparam name="T">tipe data output</typeparam>
		/// <param name="data">string CSV</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <returns>T</returns>
		public static T DeserializeCsv<T>(this string data, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",")
			where T : class, new()
		{
			Delimiter = delimiter;
			//generate format field secara langsung jika tidak ditentukan
			if (importOption == null)
			{
				importOption = typeof(T).ExtractField();
				//sort opsi import untuk mempermudah akses
				importOption = (from option in importOption
								orderby option.Sequence.HasValue descending, option.Sequence ascending
								select option).ToList();
			}
			//ambil daftar kolom yang diimport saja
			List<int> filterKolom = importOption.Where(model => model.Sequence.HasValue).Select(model => model.Sequence != null ? model.Sequence.Value : 0).ToList();
			int lastKolom = filterKolom.Max();
			var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence.Value, m => m);
			//penampung satu baris output
			var rowOutput = new T();
			//var rowInfo = rowOutput.GetType();
			//pecah berdasarkan format CSV
			string[] csvRow = Regex.Split(data.To<string>(), RegexParser);
			bool isAllEmpty = true;
			for (int kolom = 0; kolom <= lastKolom; kolom++)
			{
				//skip baca untuk yg tidak terdaftar di kolom yang akan diparsing
				if (!mapping.ContainsKey(kolom))
					continue;
				//jika hasil parsing sudah habis, hentikan pencarian
				if (kolom > (csvRow.Length - 1))
					break;
				//ambil nilai dari cell
				string nilai = csvRow[kolom];
				//hapus petik ganda di awal dan diakhir
				//gantikan petik ganda yang double dengan satu petik ganda
				nilai = Regex.Replace(nilai, "(^\"|\"$)", "").Replace("\"\"", "\"");
				//jika salah satu saja ada yg bernilai
				if (!string.IsNullOrEmpty(nilai))
					isAllEmpty = false;
				//konversi tipe data dari excel sesuai yang didefinisikan object output
				//langsung set nilainya
				rowOutput.SetFieldValue(mapping[kolom].Field, nilai);
			}
			//tambahkan ke hasil satu baris ke output
			//hanya kembalikan jika salah satu ada nilainya
			return !isAllEmpty ? rowOutput : null;
		}

		/// <summary>
		/// Digunakan untuk memparsing string CSV ke dictionary.
		/// </summary>
		/// <param name="data">string CSV</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <returns>T</returns>
		public static Dictionary<string, object> DeserializeCsv(this string data, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",")
		{
			Delimiter = delimiter;
			//import option must be provided
			if (importOption == null || importOption.Count < 1)
				throw new OpenLibrary.OpenLibraryException("Import option must be provided when using dictionary inspite off entity class.", OpenLibraryErrorType.ArgumentNotValidError);
			//ambil daftar kolom yang diimport saja
			List<int> filterKolom = importOption.Where(model => model.Sequence.HasValue).Select(model => model.Sequence != null ? model.Sequence.Value : 0).ToList();
			int lastKolom = filterKolom.Max();
			var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence.Value, m => m);
			//penampung satu baris output
			var rowOutput = new Dictionary<string, object>();
			//var rowInfo = rowOutput.GetType();
			//pecah berdasarkan format CSV
			string[] csvRow = Regex.Split(data.To<string>(), RegexParser);
			bool isAllEmpty = true;
			for (int kolom = 0; kolom <= lastKolom; kolom++)
			{
				//skip baca untuk yg tidak terdaftar di kolom yang akan diparsing
				if (!mapping.ContainsKey(kolom))
					continue;
				//jika hasil parsing sudah habis, hentikan pencarian
				if (kolom > (csvRow.Length - 1))
					break;
				//ambil nilai dari cell
				string nilai = csvRow[kolom];
				//hapus petik ganda di awal dan diakhir
				//gantikan petik ganda yang double dengan satu petik ganda
				nilai = Regex.Replace(nilai, "(^\"|\"$)", "").Replace("\"\"", "\"");
				//jika salah satu saja ada yg bernilai
				if (!string.IsNullOrEmpty(nilai))
					isAllEmpty = false;
				//konversi tipe data dari excel sesuai yang didefinisikan object output
				//langsung set nilainya
				rowOutput[mapping[kolom].Field] = nilai.To(mapping[kolom].Type, dateFormat: dateFormat);
			}
			//tambahkan ke hasil satu baris ke output
			//hanya kembalikan jika salah satu ada nilainya
			return !isAllEmpty ? rowOutput : null;
		}

		private static void PrepareFromCsv<T>(Stream file, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
			where T : class, new()
		{
			Delimiter = delimiter;
			//generate format field secara langsung jika tidak ditentukan
			if (importOption == null)
				importOption = typeof(T).ExtractField();
			//hapus priority untuk sementara
			importOption.ForEach(column => column.Sequence = null);
			var reader = new StreamReader(file);
			#region Process format column import
			//parsing baris pertama untuk mencari posisi field dan nilainya
			string[] firstRow = Regex.Split(reader.ReadLine().To<string>(), RegexParser);
			if (firstRow.Length < 1)
				return;
			for (int kolom = 0; kolom < firstRow.Length; kolom++)
			{
				//ambil nama caption yg muncul
				string caption = firstRow[kolom];
				//hapus petik ganda di awal dan diakhir
				//gantikan petik ganda yang double dengan satu petik ganda
				caption = Regex.Replace(caption, "(^\"|\"$)", "").Replace("\"\"", "\"");
				//skip jika nama caption kosong
				if (string.IsNullOrEmpty(caption))
					continue;
				//cara nama caption dari daftar opsi import
				var columnOption = isCaseSensitive
					? importOption.FirstOrDefault(model => model.Caption.Trim() == caption.Trim())
					: importOption.FirstOrDefault(model => model.Caption.Trim().ToLower() == caption.Trim().ToLower());
				//masukkan posisi kolom ke priority
				if (columnOption != null)
					columnOption.Sequence = kolom;
			}
			//sort opsi import untuk mempermudah akses
			importOption = (from option in importOption
							orderby option.Sequence.HasValue descending, option.Sequence ascending
							select option).ToList();
			#endregion
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari csv ke collection. Baris pertama sebagai penentu nama kolom. 
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="file">pointer ke file</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <returns>collection</returns>
		public static void FromCsv<T>(Stream file, System.Action<T> action, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
			where T : class, new()
		{
			PrepareFromCsv<T>(file, importOption, dateFormat, delimiter, isCaseSensitive);
			var reader = new StreamReader(file);
			var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence.Value, m => m);
			//tidak perlu proses jika dalam caption tidak disebutkan dalam baris pertama
			if (mapping.Count < 1)
				return;
			#region Process import data
			string row;
			try
			{
				while ((row = reader.ReadLine()) != null)
				{
					//penampung satu baris output
					var rowOutput = row.DeserializeCsv<T>(importOption, dateFormat, delimiter);
					//hanya tambahkan jika ada hasilnya
					if (rowOutput != null)
						action(rowOutput);
				}
			}
			catch (System.IndexOutOfRangeException) { }
			catch (KeyNotFoundException) { }
			catch (System.ArgumentNullException) { }
			#endregion
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari csv ke collection. Baris pertama sebagai penentu nama kolom. 
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="file">pointer ke file</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <returns>collection</returns>
		public static List<T> FromCsv<T>(Stream file, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
			where T : class, new()
		{
			var output = new List<T>();
			FromCsv<T>(file, row => output.Add(row), importOption, dateFormat, delimiter, isCaseSensitive);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari csv ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="filename">nama file (fullpath)</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <returns>collection</returns>
		public static List<T> FromCsv<T>(string filename, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
			where T : class, new()
		{
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				return FromCsv<T>(fileStream, importOption, dateFormat, delimiter, isCaseSensitive);
			}
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari csv ke collection. Baris pertama sebagai penentu nama kolom. 
		/// </summary>
		/// <param name="file">pointer ke file</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <returns>collection</returns>
		public static void FromCsv(Stream file, System.Action<Dictionary<string, object>> action, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
		{
			if (importOption == null || importOption.Count < 1)
				throw new OpenLibrary.OpenLibraryException("Import option must be provided when using dictionary inspite off entity class.", OpenLibraryErrorType.ArgumentNotValidError);
			PrepareFromCsv<object>(file, importOption, dateFormat, delimiter, isCaseSensitive);
			var reader = new StreamReader(file);
			var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence.Value, m => m);
			//tidak perlu proses jika dalam caption tidak disebutkan dalam baris pertama
			if (mapping.Count < 1)
				return;
			#region Process import data
			string row;
			try
			{
				while ((row = reader.ReadLine()) != null)
				{
					//penampung satu baris output
					var rowOutput = row.DeserializeCsv(importOption, dateFormat, delimiter);
					//hanya tambahkan jika ada hasilnya
					if (rowOutput != null)
						action(rowOutput);
				}
			}
			catch (System.IndexOutOfRangeException) { }
			catch (KeyNotFoundException) { }
			catch (System.ArgumentNullException) { }
			#endregion
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari csv ke collection. Baris pertama sebagai penentu nama kolom. 
		/// </summary>
		/// <param name="file">pointer ke file</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <returns>collection</returns>
		public static List<Dictionary<string, object>> FromCsv(Stream file, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
		{
			var output = new List<Dictionary<string, object>>();
			FromCsv(file, row => output.Add(row), importOption, dateFormat, delimiter, isCaseSensitive);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari csv ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// </summary>
		/// <param name="filename">nama file (fullpath)</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="dateFormat">format import DateTime jika ada (default: yyyy-MM-dd)</param>
		/// <param name="delimiter">karakter pemisah (default: koma (,) -> standar en-US)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <returns>collection</returns>
		public static List<Dictionary<string, object>> FromCsv(string filename, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false)
		{
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				return FromCsv(fileStream, importOption, dateFormat, delimiter, isCaseSensitive);
			}
		}
	}
}
