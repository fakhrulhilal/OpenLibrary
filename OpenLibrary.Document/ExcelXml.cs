using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenLibrary.Extension;
using OpenLibrary.Annotation;

namespace OpenLibrary.Document
{
	/// <summary>
	/// Processing with excel xml (&gt;= 2007)
	/// </summary>
	public static class ExcelXml
	{
		// ReSharper disable EmptyGeneralCatchClause
		private static Dictionary<Type, string> styleOptions;

		private static Dictionary<Type, string> StyleOptions
		{
			get
			{
				return styleOptions ??
					   new Dictionary<Type, string>
					   {
						   { typeof(DateTime), "yyyy-MM-dd" },
						   { typeof(decimal), "#,#.#####;-#,#.#####;\"-\"" },
						   { typeof(double), "#,#.#####;-#,#.#####;\"-\"" },
						   { typeof(int), "#,#;-#,#;\"-\"" },
						   { typeof(long), "#,#;-#,#;\"-\"" },
						   { typeof(string), "@" }
					   };
			}
			// ReSharper disable UnusedMember.Local
			set { styleOptions = value; }
			// ReSharper restore UnusedMember.Local
		}

		/// <summary>
		/// Buat file excel dari data
		/// </summary>
		/// <param name="data">data yg akan diexport ke excel</param>
		/// <param name="worksheetName">nama worksheet</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="styleOption">format cetak</param>
		/// <returns>Stream</returns>
		public static Stream ToExcel<T>(IEnumerable<T> data, string worksheetName = "", List<MappingOption> exportOption = null, Dictionary<Type, string> styleOption = null)
			where T : class
		{
			//List<object> objects = data.Cast<object>().ToList();
			List<T> objects = data.ToList();
			var workbook = new ExcelPackage();
			var worksheet = workbook.Workbook.Worksheets.Add(string.IsNullOrEmpty(worksheetName) ? "data" : worksheetName);

			if (objects.Count > 0)
			{
				//buat opsi export jika tidak ada
				if (exportOption == null)
				{
					//exportOption = objects[0].ExtractField();
					exportOption = typeof(T).ExtractField();
					exportOption.ForEach(option => option.Width = 30);
				}
				//header row
				int columnNumber = 1;
				worksheet.Row(1).Style.Font.Bold = true;
				worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				exportOption = (from option in exportOption
								orderby option.Sequence.HasValue descending, option.Sequence ascending
								select option).ToList();
				//default format
				if (styleOption == null)
					styleOption = StyleOptions;
				foreach (var excelColumn in exportOption)
				{
					worksheet.Cells[1, columnNumber].Value = excelColumn.Caption;
					worksheet.Column(columnNumber).AutoFit(excelColumn.Width);
					columnNumber++;
				}
				//freeze baris pertama
				worksheet.View.FreezePanes(2, 1);

				//isi data
				int rowNumber = 2;
				foreach (var rowData in objects)
				{
					columnNumber = 1;
					exportOption.ForEach(excelColumn =>
						worksheet.Cells[rowNumber, columnNumber++].Value = rowData.GetFieldValue(excelColumn.Field));
					rowNumber++;
				}
				//format style
				int totalRow = worksheet.Dimension.End.Row;
				//ambil sample data
				var sampleData = objects[1];
				//format data per kolom
				exportOption.ForEach(option =>
				{
					columnNumber = option.Sequence.HasValue ? option.Sequence.Value : 1;
					using (ExcelRange range = worksheet.Cells[2, columnNumber, totalRow, columnNumber])
					{
						//cari tipe data untuk kolom tersebut
						var nilai = sampleData.GetFieldValue(option.Field);
						try
						{
							//set format jika ada
							if (styleOption[nilai.GetType()] != null)
								range.Style.Numberformat.Format = styleOption[nilai.GetType()];
						}
						catch (KeyNotFoundException) { }
						catch (NullReferenceException) { }
						catch (System.Reflection.TargetInvocationException) { }
						catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
					}
				});
			}

			var output = new MemoryStream();
			workbook.SaveAs(output);
			workbook.Dispose();
			return output;
		}

		/// <summary>
		/// Buat file excel dari data
		/// </summary>
		/// <param name="data">data yg akan diexport ke excel</param>
		/// <param name="filename">nama file output</param>
		/// <param name="worksheetName">nama worksheet</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="styleOption">format cetak</param>
		/// <returns>Stream</returns>
		public static void ToExcel<T>(IEnumerable<T> data, string filename, string worksheetName = "", List<MappingOption> exportOption = null, Dictionary<Type, string> styleOption = null)
			where T : class
		{
			using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
			using (var output = ToExcel(data, worksheetName, exportOption, styleOption))
			{
				output.Seek(0, SeekOrigin.Begin);
				output.CopyTo(fileStream);
				fileStream.Flush();
			}
		}

		private static void PrepareFromExcel<T>(ExcelPackage workbook, out ExcelWorksheet worksheet, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isCaseSensitive = false)
		{
			//generate format field secara langsung jika tidak ditentukan
			if (importOption == null)
				importOption = typeof(T).ExtractField();
			//hapus priority untuk sementara
			importOption.ForEach(column => column.Sequence = null);
			worksheet = string.IsNullOrEmpty(worksheetName) ? workbook.Workbook.Worksheets[1] : workbook.Workbook.Worksheets[worksheetName];

			#region Process format column import
			//parsing baris header row untuk mencari posisi field dan nilainya
			var firstRow = worksheet.Row(headerRow);
			if (firstRow == null || worksheet.Dimension == null)
				throw new ArgumentNullException("headerRow", "Header row not found");
			int kolom = 1;
			//int lastKolom = -1;
			while (true)
			{
				//ambil nama caption yg muncul
				var cell = worksheet.Cells[headerRow, kolom];
				if (cell == null || cell.Value == null)
					break;
				string caption = cell.Value.To<string>();
				if (!string.IsNullOrEmpty(caption))
					caption = caption.Trim();
				//jika sudah ditemukan kolom yang isinya kosong, langsung hentikan pencarian
				if (string.IsNullOrEmpty(caption))
					break;
				//cara nama caption dari daftar opsi import
				var columnOption = isCaseSensitive
					? importOption.FirstOrDefault(model => model.Caption.Trim() == caption)
					: importOption.FirstOrDefault(model => model.Caption.Trim().ToLower() == caption.ToLower());
				//masukkan posisi kolom ke priority
				if (columnOption != null)
					columnOption.Sequence = kolom;
				//lastKolom = kolom;
				kolom++;
			}
			//sort opsi import untuk mempermudah akses
			importOption = (from option in importOption
							orderby option.Sequence.HasValue descending, option.Sequence ascending
							select option).ToList();
			#endregion
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="file">pointer ke file</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		public static void FromExcel<T>(Stream file, Action<T> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
			where T : class, new()
		{
			var workbook = new ExcelPackage(file);
			try
			{
				ExcelWorksheet worksheet;
				PrepareFromExcel<T>(workbook, out worksheet, worksheetName, importOption, headerRow, isCaseSensitive);
				if (importOption == null)
					importOption = new List<MappingOption>();
				var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence ?? 0, m => m);
				int lastKolom = importOption.Max(model => model.Sequence ?? 0);
				//tidak perlu proses jika dalam caption tidak disebutkan dalam baris pertama
				if (mapping.Count < 1)
					return;

				#region Process import data
				int totalRow = worksheet.Dimension.End.Row;
				//tidak perlu proses jika memang tidak ada data
				if (totalRow <= headerRow)
					return;
				for (int baris = headerRow + 1; baris <= totalRow; baris++)
				{
					//penampung satu baris output
					var rowOutput = new T();
					bool isAllEmpty = true;
					for (int kolom = 1; kolom <= lastKolom; kolom++)
					{
						//skip baca untuk yg tidak terdaftar di kolom yang akan diparsing
						if (!mapping.ContainsKey(kolom))
							continue;
						//ambil nilai dari cell
						object nilai;
						try
						{
							nilai = worksheet.Cells[baris, kolom].Value;
						}
						catch (NullReferenceException) { nilai = null; }
						if (nilai != null)
							isAllEmpty = false;
						//konversi tipe data dari excel sesuai yang didefinisikan object output
						//langsung set nilainya
						try
						{
							rowOutput.SetFieldValue(mapping[kolom].Field, nilai, dateFormat: dateFormat);
						}
						catch { }
					}
					//hanya execute fungsi jika satu baris ada isinya semua
					if (!isAllEmpty)
						try
						{
							action(rowOutput);
						}
						catch { }
					if (isAllEmpty && isBreakOnEmptyRow)
						break;
				}
				#endregion
			}
			catch (KeyNotFoundException) { }
			catch (ArgumentNullException) { }
			workbook.Dispose();
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="file">pointer ke file</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <returns>collection</returns>
		public static List<T> FromExcel<T>(Stream file, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
			where T : class, new()
		{
			var output = new List<T>();
			FromExcel<T>(file, output.Add, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="filename">nama file (fullpath)</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <returns>collection</returns>
		public static List<T> FromExcel<T>(string filename, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
			where T : class, new()
		{
			List<T> output;
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				output = FromExcel<T>(fileStream, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <typeparam name="T">tipe class output</typeparam>
		/// <param name="filename">nama file</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		public static void FromExcel<T>(string filename, Action<T> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
			where T : class, new()
		{
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				FromExcel(fileStream, action, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
			}
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <param name="file">pointer ke file</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		public static void FromExcel(Stream file, Action<Dictionary<string, object>> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
		{
			var workbook = new ExcelPackage(file);
			if (importOption == null || importOption.Count < 1)
				throw new OpenLibraryException("Import option must be provided when using dictionary inspite off entity class.", OpenLibraryErrorType.ArgumentNotValidError);
			try
			{
				ExcelWorksheet worksheet;
				PrepareFromExcel<object>(workbook, out worksheet, worksheetName, importOption, headerRow, isCaseSensitive);
				var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence ?? 0, m => m);
				int lastKolom = importOption.Max(model => model.Sequence ?? 0);
				//tidak perlu proses jika dalam caption tidak disebutkan dalam baris pertama
				if (mapping.Count < 1)
					return;

				#region Process import data
				int totalRow = worksheet.Dimension.End.Row;
				//tidak perlu proses jika memang tidak ada data
				if (totalRow <= headerRow)
					return;
				for (int baris = headerRow + 1; baris <= totalRow; baris++)
				{
					//penampung satu baris output
					var rowOutput = new Dictionary<string, object>();
					bool isAllEmpty = true;
					for (int kolom = 1; kolom <= lastKolom; kolom++)
					{
						//skip baca untuk yg tidak terdaftar di kolom yang akan diparsing
						if (!mapping.ContainsKey(kolom))
							continue;
						//ambil nilai dari cell
						object nilai;
						try
						{
							nilai = worksheet.Cells[baris, kolom].Value;
						}
						catch (NullReferenceException) { nilai = null; }
						if (nilai != null)
							isAllEmpty = false;
						//konversi tipe data dari excel sesuai yang didefinisikan object output
						//langsung set nilainya
						try
						{
							rowOutput[mapping[kolom].Field] = nilai.To(mapping[kolom].Type, false, dateFormat);
						}
						catch { }
					}
					//hanya execute fungsi jika satu baris ada isinya semua
					if (!isAllEmpty)
						try
						{
							action(rowOutput);
						}
						catch { }
					if (isAllEmpty && isBreakOnEmptyRow)
						break;
				}
				#endregion
			}
			catch (KeyNotFoundException) { }
			catch (ArgumentNullException) { }
			workbook.Dispose();
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <param name="file">pointer ke file</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <returns>collection</returns>
		public static List<Dictionary<string, object>> FromExcel(Stream file, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
		{
			List<Dictionary<string, object>> output = new List<Dictionary<string, object>>();
			FromExcel(file, output.Add, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <param name="filename">nama file (fullpath)</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <returns>collection</returns>
		public static List<Dictionary<string, object>> FromExcel(string filename, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
		{
			List<Dictionary<string, object>> output;
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				output = FromExcel(fileStream, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <param name="filename">nama file</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		public static void FromExcel(string filename, Action<Dictionary<string, object>> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
		{
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				FromExcel(fileStream, action, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
		}
	}
}
