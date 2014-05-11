using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OpenLibrary.Extension;
using OpenLibrary.Annotation;

namespace OpenLibrary.Document
{
	/// <summary>
	/// Processing old excel (&lt;= 2003)
	/// </summary>
	public class Excel
	{
		// ReSharper disable EmptyGeneralCatchClause
		/// <summary>
		/// Buat file excel dari data
		/// </summary>
		/// <param name="data">data yg akan diexport ke excel</param>
		/// <param name="worksheetName">nama worksheet</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="formatType">jenis format excel</param>
		/// <returns>Stream</returns>
		public static Stream ToExcel<T>(IEnumerable<T> data, string worksheetName = "", List<MappingOption> exportOption = null, DocumentType formatType = DocumentType.Xls)
			where T : class
		{
			//List<object> objects = data.Cast<object>().ToList();
			List<T> objects = data.ToList();
			var workbook = new HSSFWorkbook();
			var worksheet = workbook.CreateSheet(string.IsNullOrEmpty(worksheetName) ? "data" : worksheetName);

			if (objects.Count > 0)
			{
				//buat opsi export jika tidak ada
				if (exportOption == null)
					exportOption = typeof(T).ExtractField();
				//header row
				int columnNumber = 0;
				var headerRow = worksheet.CreateRow(0);
				exportOption = exportOption.OrderBy(model => model.Sequence).ToList();
				foreach (var excelColumn in exportOption)
				{
					headerRow.CreateCell(columnNumber).SetCellValue(excelColumn.Caption);
					worksheet.SetColumnWidth(columnNumber, excelColumn.Width * 256);
					columnNumber++;
				}
				//freeze baris pertama
				worksheet.CreateFreezePane(0, 1, 0, 1);

				//isi data
				int rowNumber = 1;
				foreach (var rowData in objects)
				{
					var row = worksheet.CreateRow(rowNumber++);
					columnNumber = 0;
					foreach (var excelColumn in exportOption)
					{
						//var value = rowData.GetType().GetProperty(excelColumn.Field).GetValue(rowData, null);
						var value = rowData.GetFieldValue(excelColumn.Field);
						//skip jika tidak ada data
						if (value == null)
							columnNumber++;
						else
							row.CreateCell(columnNumber++).SetCellValue(value);
					}
				}
			}

			var output = new MemoryStream();
			workbook.Write(output);
			return output;
		}

		/// <summary>
		/// Buat file excel dari data
		/// </summary>
		/// <param name="data">data yg akan diexport ke excel</param>
		/// <param name="filename">nama file output</param>
		/// <param name="worksheetName">nama worksheet</param>
		/// <param name="exportOption">opsi mapping caption &amp; field untuk export</param>
		/// <param name="formatType">jenis format excel</param>
		/// <returns>Stream</returns>
		public static void ToExcel<T>(IEnumerable<T> data, string filename, string worksheetName = "",
									  List<MappingOption> exportOption = null, DocumentType formatType = DocumentType.Xls)
			where T : class
		{
			var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
			var output = ToExcel(data, worksheetName, exportOption, formatType);
			output.Seek(0, SeekOrigin.Begin);
			output.CopyTo(fileStream);
			fileStream.Close();
		}

		/// <summary>
		/// Digunakan untuk mengambil nilai dari cell di excel
		/// </summary>
		/// <param name="cell">cell</param>
		/// <param name="type">tipe data (jika tidak ditentukan, akan dicari otomatis)</param>
		/// <returns>string</returns>
		private static string GetCellValue(ICell cell, CellType? type = null)
		{
			string output;
			if (!type.HasValue)
				type = cell.CellType;
			switch (type.Value)
			{
				case CellType.NUMERIC:
					output = cell.NumericCellValue.To<string>();
					break;
				case CellType.FORMULA:
					output = GetCellValue(cell, cell.CachedFormulaResultType);
					break;
				case CellType.BOOLEAN:
					output = cell.BooleanCellValue.To<string>();
					break;
				case CellType.STRING:
					output = cell.StringCellValue;
					break;
				default:
					output = cell.StringCellValue;
					break;
			}
			return output;
		}

		private static void PrepareFromExcel<T>(HSSFWorkbook workbook, out ISheet worksheet, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isCaseSensitive = false)
		{
			//generate format field secara langsung jika tidak ditentukan
			if (importOption == null)
				importOption = typeof(T).ExtractField();
			//hapus priority untuk sementara
			importOption.ForEach(column => column.Sequence = null);
			worksheet = string.IsNullOrEmpty(worksheetName)
								? workbook.GetSheetAt(0)
								: workbook.GetSheet(worksheetName);
			#region Process format column import
			//parsing baris header row untuk mencari posisi field dan nilainya
			var firstRow = worksheet.GetRow(headerRow - 1);
			if (firstRow == null)
				return;
			int kolom = 0;
			//int lastKolom = -1;
			while (true)
			{
				//ambil nama caption yg muncul
				var cell = firstRow.GetCell(kolom);
				if (cell == null)
					break;
				string caption = cell.StringCellValue;
				//jika sudah ditemukan kolom yang isinya kosong, langsung hentikan pencarian
				if (string.IsNullOrEmpty(caption))
					break;
				//cara nama caption dari daftar opsi import
				var columnOption = isCaseSensitive
					? importOption.FirstOrDefault(model => model.Caption.Trim() == caption.Trim())
					: importOption.FirstOrDefault(model => model.Caption.Trim().ToLower() == caption.Trim().ToLower());
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
		/// <returns>collection</returns>
		public static void FromExcel<T>(Stream file, Action<T> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
			where T : class, new()
		{
			var workbook = new HSSFWorkbook(file);
			ISheet worksheet;
			try
			{
				PrepareFromExcel<T>(workbook, out worksheet, worksheetName, importOption, headerRow, isCaseSensitive);
				if (importOption == null)
					importOption = new List<MappingOption>();
				var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence ?? 0, m => m);
				int lastKolom = importOption.Max(model => model.Sequence ?? 0);
				//tidak perlu proses jika dalam caption tidak disebutkan dalam baris pertama
				if (lastKolom < 0 || mapping.Count < 1)
					return;
				#region Process import data
				for (int baris = headerRow; baris <= worksheet.LastRowNum; baris++)
				{
					var row = worksheet.GetRow(baris);
					//penampung satu baris output
					var rowOutput = new T();
					bool isAllEmpty = true;
					for (int kolom = 0; kolom <= lastKolom; kolom++)
					{
						//skip baca untuk yg tidak terdaftar di kolom yang akan diparsing
						if (!mapping.ContainsKey(kolom))
							continue;
						//ambil nilai dari cell
						string nilai;
						try
						{
							nilai = GetCellValue(row.GetCell(kolom));
						}
						catch (NullReferenceException) { nilai = ""; }
						if (!string.IsNullOrEmpty(nilai))
							isAllEmpty = false;
						//konversi tipe data dari excel sesuai yang didefinisikan object output
						//langsung set nilainya
						try
						{
							rowOutput.SetFieldValue(mapping[kolom].Field, nilai, true, dateFormat);
						}
						catch { }
					}
					//tambahkan ke hasil satu baris ke output
					//hanya tambahkan jika satu baris ada isinya semua
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
			worksheet = null;
			workbook = null;
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
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <returns>collection</returns>
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
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				return FromExcel<T>(fileStream, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
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
		/// <returns>collection</returns>
		public static void FromExcel(Stream file, Action<Dictionary<string, object>> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
		{

			var workbook = new HSSFWorkbook(file);
			ISheet worksheet;
			if (importOption == null || importOption.Count < 1)
				throw new OpenLibraryException("Import option must be provided when using dictionary inspite off entity class.", OpenLibraryErrorType.ArgumentNotValidError);
			try
			{
				PrepareFromExcel<object>(workbook, out worksheet, worksheetName, importOption, headerRow, isCaseSensitive);
				var mapping = importOption.Where(m => m.Sequence.HasValue).ToList().ToDictionary(m => m.Sequence ?? 0, m => m);
				int lastKolom = importOption.Max(model => model.Sequence ?? 0);
				//tidak perlu proses jika dalam caption tidak disebutkan dalam baris pertama
				if (lastKolom < 0 || mapping.Count < 1)
					return;
				#region Process import data
				for (int baris = headerRow; baris <= worksheet.LastRowNum; baris++)
				{
					var row = worksheet.GetRow(baris);
					//penampung satu baris output
					var rowOutput = new Dictionary<string, object>();
					bool isAllEmpty = true;
					for (int kolom = 0; kolom <= lastKolom; kolom++)
					{
						//skip baca untuk yg tidak terdaftar di kolom yang akan diparsing
						if (!mapping.ContainsKey(kolom))
							continue;
						//ambil nilai dari cell
						string nilai;
						try
						{
							nilai = GetCellValue(row.GetCell(kolom));
						}
						catch (NullReferenceException) { nilai = ""; }
						if (!string.IsNullOrEmpty(nilai))
							isAllEmpty = false;
						//konversi tipe data dari excel sesuai yang didefinisikan object output
						//langsung set nilainya
						try
						{
							rowOutput[mapping[kolom].Field] = nilai.To(mapping[kolom].Type, true, dateFormat);
						}
						catch { }
					}
					//tambahkan ke hasil satu baris ke output
					//hanya tambahkan jika satu baris ada isinya semua
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
			worksheet = null;
			workbook = null;
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
			var output = new List<Dictionary<string, object>>();
			FromExcel(file, output.Add, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
			return output;
		}

		/// <summary>
		/// Digunakan untuk mengimport data dari excel ke collection.
		/// Baris pertama sebagai penentu nama kolom. 
		/// Pencarian nama kolom berhenti jika pada baris pertama ditemukan nama kolom yang kosong.
		/// </summary>
		/// <param name="filename">nama file (fullpath)</param>
		/// <param name="action">fungsi yang dieksekusi setiap baris row</param>
		/// <param name="worksheetName">nama worksheet di excel, jika tidak ditentukan, maka diambil worksheet pertama</param>
		/// <param name="importOption">opsi mapping caption &amp; field untuk import</param>
		/// <param name="headerRow">lokasi row untuk scan field (default: baris pertama)</param>
		/// <param name="isBreakOnEmptyRow">jika ditemukan baris kosong untuk semua yg terscan, maka dianggap akhir baris (default: tidak)</param>
		/// <param name="isCaseSensitive">menentukan apakah pencocokan nama header case sensitive atau tidak (default: tidak)</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <returns>collection</returns>
		public static void FromExcel(string filename, Action<Dictionary<string, object>> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "")
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
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				return FromExcel(fileStream, worksheetName, importOption, headerRow, isBreakOnEmptyRow, isCaseSensitive, dateFormat);
		}
	}
}
