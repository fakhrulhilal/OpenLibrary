using System.ComponentModel;

namespace OpenLibrary.Document
{
	/// <summary>
	/// Cetak sesuai dengan tipe data
	/// </summary>
	public class StyleOption
	{
		/// <summary>
		/// Tipe data
		/// </summary>
		public System.Type Type { get; set; }

		/// <summary>
		/// Style
		/// </summary>
		public string Style { get; set; }
	}

	/// <summary>
	/// File document type
	/// </summary>
	public enum DocumentType
	{
		/// <summary>
		/// Old excel (&lt;= 2003)
		/// </summary>
		[Description("application/vnd.ms-excel")]
		Xls,

		/// <summary>
		/// Excel XML (&gt; 2007)
		/// </summary>
		[Description("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
		Xlsx,

		/// <summary>
		/// Comma Separated Version (CSV)
		/// </summary>
		[Description("text/csv")]
		Csv
	}
}
