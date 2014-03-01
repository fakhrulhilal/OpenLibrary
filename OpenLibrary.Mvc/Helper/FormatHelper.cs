using System.Web.Mvc;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Jenis format
	/// </summary>
	public enum FormatType
	{
		/// <summary>
		/// Format angka tanpa digit desimal
		/// </summary>
		Numeric,

		/// <summary>
		/// Format angka dengan 2 digit desimal
		/// </summary>
		NumericWithDecimalPoint,

		/// <summary>
		/// Format prosentase (ada karakter '%') tanpa digit desimal
		/// </summary>
		Percentage,

		/// <summary>
		/// Format prosentase (ada karakter '%') dengan 2 digit desimal
		/// </summary>
		PercentageWithDecimalPoint,

		/// <summary>
		/// Format tanggal
		/// </summary>
		Date,

		/// <summary>
		/// Format jam
		/// </summary>
		Time,

		/// <summary>
		/// Format tanggal &amp; jam
		/// </summary>
		DateTime,

		/// <summary>
		/// Format boolean menjadi yes (jika true) atau no (jika false)
		/// </summary>
		YesNo
	}

	/// <summary>
	/// Helper for print format
	/// </summary>
	public static class FormatHelper
	{
		private const string THOUSAND_SEPARATOR = ",";
		private const string DECIMAL_SEPARATOR = ".";
		private const string DEFAULT_DATEFORMAT = "yyyy-MM-dd";
		private const string DEFAULT_TIMEFORMAT = "HH:mm";
		private const string DEFAULT_EMPTY = "-";

		/// <summary>
		/// Menggenerate format yg compatible dengan C# atau kendo
		/// </summary>
		/// <param name="formatType">jenis format</param>
		/// <param name="isCSharpFormat">jika ya, maka akan digunakan {0:format}</param>
		/// <param name="decimalPoint">jumlah digit desimal</param>
		/// <param name="suffix">tambahan kata di akhir</param>
		/// <param name="prefix">tambahan kata di awal</param>
		/// <param name="dateFormat">custom date format (default: yyyy-MM-dd)</param>
		/// <param name="forcePaddingZero">paksa isi dengan angka 0 jika nilai decimal tidak ada (hanya jika <paramref name="decimalPoint"/> &gt; 0</param>
		/// <returns>string</returns>
		private static string PrepareFormat(FormatType formatType, bool isCSharpFormat = true, int decimalPoint = 2, string suffix = "", string prefix = "", string dateFormat = "", bool forcePaddingZero = true)
		{
			string format = string.Empty;
			string defaultDateFormat = DEFAULT_DATEFORMAT;
			switch (formatType)
			{
				case FormatType.Numeric:
				case FormatType.Percentage:
					format = "#" + THOUSAND_SEPARATOR + (forcePaddingZero ? "0" : "#");
					break;
				case FormatType.NumericWithDecimalPoint:
				case FormatType.PercentageWithDecimalPoint:
					format = "#" + THOUSAND_SEPARATOR + (forcePaddingZero ? "0" : "#");
					if (decimalPoint > 0)
						format += DECIMAL_SEPARATOR + new System.String(forcePaddingZero ? '0' : '#', decimalPoint);
					break;
				case FormatType.Date:
					format = string.IsNullOrEmpty(dateFormat) ? defaultDateFormat : dateFormat;
					break;
				case FormatType.Time:
					format = DEFAULT_TIMEFORMAT;
					break;
				case FormatType.DateTime:
					format = (string.IsNullOrEmpty(dateFormat) ? defaultDateFormat : dateFormat) + " " + DEFAULT_TIMEFORMAT;
					break;
			}
			//untuk format C#, harus menggunakan {x:#} dst
			if (isCSharpFormat)
				format = "{0:" + format + "}";
			//karakter tambahan harus diluar {x}
			if (formatType.ToString().Contains("Percentage"))
				format += " %";
			if (!string.IsNullOrEmpty(prefix))
				format = prefix + " " + format;
			if (!string.IsNullOrEmpty(suffix))
				format += " " + suffix;
			return format;
		}

		/// <summary>
		/// Generate string format
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="formatType">jenis format</param>
		/// <param name="isCSharpFormat">menentukan apakah format yg diinginkan format untuk bahasa C# atau tidak (dalam kurung kurawal)</param>
		/// <param name="decimalPoint">number of decimal digit, hanya berlaku untuk jenis format angka (default: 0)</param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <param name="dateFormat">format tanggal</param>
		/// <param name="forcePaddingZero">paksa isi dengan angka 0 jika nilai decimal tidak ada (hanya jika <paramref name="decimalPoint"/> &gt; 0, hanya berlaku untuk jenis format angka</param>
		/// <returns>string format</returns>
		public static string Format(this HtmlHelper helper, FormatType formatType, bool isCSharpFormat = true, int decimalPoint = 2, string suffix = "", string prefix = "", string dateFormat = "", bool forcePaddingZero = true)
		{
			return PrepareFormat(formatType, isCSharpFormat, decimalPoint, suffix, prefix, dateFormat, forcePaddingZero);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="decimalPoint">number of decimal digit, hanya berlaku untuk jenis format angka (default: 0)</param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <param name="forcePaddingZero">paksa isi dengan angka 0 jika nilai decimal tidak ada (hanya jika <paramref name="decimalPoint"/> &gt; 0, hanya berlaku untuk jenis format angka</param>
		/// <returns></returns>
		public static string Format(this double number, int decimalPoint = 2, string suffix = "", string prefix = "", bool forcePaddingZero = true)
		{
			return number.ToString(PrepareFormat(FormatType.NumericWithDecimalPoint, isCSharpFormat: false, decimalPoint: decimalPoint, suffix: suffix, prefix: prefix, forcePaddingZero: forcePaddingZero), System.Threading.Thread.CurrentThread.CurrentCulture);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="decimalPoint">number of decimal digit, hanya berlaku untuk jenis format angka (default: 0)</param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <param name="forcePaddingZero">paksa isi dengan angka 0 jika nilai decimal tidak ada (hanya jika <paramref name="decimalPoint"/> &gt; 0, hanya berlaku untuk jenis format angka</param>
		/// <returns></returns>
		public static string Format(this double? number, int decimalPoint = 2, string suffix = "", string prefix = "", bool forcePaddingZero = true)
		{
			return number.HasValue
				? number.Value.Format(decimalPoint, suffix, prefix, forcePaddingZero)
				: DEFAULT_EMPTY;
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="decimalPoint">number of decimal digit, hanya berlaku untuk jenis format angka (default: 0)</param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <param name="forcePaddingZero">paksa isi dengan angka 0 jika nilai decimal tidak ada (hanya jika <paramref name="decimalPoint"/> &gt; 0, hanya berlaku untuk jenis format angka</param>
		/// <returns></returns>
		public static string Format(this decimal number, int decimalPoint = 2, string suffix = "", string prefix = "", bool forcePaddingZero = true)
		{
			return Format((double)number, decimalPoint, suffix, prefix, forcePaddingZero);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="decimalPoint">number of decimal digit, hanya berlaku untuk jenis format angka (default: 0)</param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <param name="forcePaddingZero">paksa isi dengan angka 0 jika nilai decimal tidak ada (hanya jika <paramref name="decimalPoint"/> &gt; 0, hanya berlaku untuk jenis format angka</param>
		/// <returns></returns>
		public static string Format(this decimal? number, int decimalPoint = 2, string suffix = "", string prefix = "", bool forcePaddingZero = true)
		{
			return Format((double?)number, decimalPoint, suffix, prefix, forcePaddingZero);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <returns></returns>
		public static string Format(this long number, string suffix = "", string prefix = "")
		{
			return number.ToString(PrepareFormat(FormatType.Numeric, isCSharpFormat: false, suffix: suffix, prefix: prefix), System.Threading.Thread.CurrentThread.CurrentCulture);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <returns></returns>
		public static string Format(this long? number, string suffix = "", string prefix = "")
		{
			return number.HasValue
				? number.Value.Format(suffix, prefix)
				: DEFAULT_EMPTY;
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <returns></returns>
		public static string Format(this int number, string suffix = "", string prefix = "")
		{
			return Format((long)number, suffix, prefix);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <returns></returns>
		public static string Format(this int? number, string suffix = "", string prefix = "")
		{
			return Format((long?)number, suffix, prefix);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <returns></returns>
		public static string Format(this short number, string suffix = "", string prefix = "")
		{
			return Format((long)number, suffix, prefix);
		}

		/// <summary>
		/// Format untuk angka
		/// </summary>
		/// <param name="number"></param>
		/// <param name="suffix">character after formatting</param>
		/// <param name="prefix">character before formatting</param>
		/// <returns></returns>
		public static string Format(this short? number, string suffix = "", string prefix = "")
		{
			return Format((long?)number, suffix, prefix);
		}

		/// <summary>
		/// Format date &amp; time
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="dateFormat">format spesifik waktu (default jika kosong: yyyy-MM-dd HH:mm)</param>
		/// <returns></returns>
		public static string Format(this System.DateTime dateTime, string dateFormat = "")
		{
			return dateTime.ToString(PrepareFormat(FormatType.DateTime, isCSharpFormat: false, dateFormat: dateFormat));
		}

		/// <summary>
		/// Format date &amp; time
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="dateFormat">format spesifik waktu (default jika kosong: yyyy-MM-dd HH:mm)</param>
		/// <returns></returns>
		public static string Format(this System.DateTime? dateTime, string dateFormat = "")
		{
			return dateTime.HasValue
				? dateTime.Value.Format(dateFormat)
				: DEFAULT_EMPTY;
		}

		/// <summary>
		/// Format for date
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="dateFormat">format spesifik waktu (default jika kosong: yyyy-MM-dd)</param>
		/// <returns></returns>
		public static string FormatDate(this System.DateTime dateTime, string dateFormat = "")
		{
			return dateTime.ToString(PrepareFormat(FormatType.Date, isCSharpFormat: false, dateFormat: dateFormat));
		}

		/// <summary>
		/// Format for date
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="dateFormat">format spesifik waktu (default jika kosong: yyyy-MM-dd)</param>
		/// <returns></returns>
		public static string FormatDate(this System.DateTime? dateTime, string dateFormat = "")
		{
			return dateTime.HasValue
				? dateTime.Value.FormatDate(dateFormat)
				: DEFAULT_EMPTY;
		}

		/// <summary>
		/// Format untuk time
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="dateFormat">format spesifik waktu (default jika kosong: HH:mm)</param>
		/// <returns></returns>
		public static string FormatTime(this System.DateTime dateTime, string dateFormat = "")
		{
			return dateTime.ToString(PrepareFormat(FormatType.Time, isCSharpFormat: false, dateFormat: dateFormat));
		}

		/// <summary>
		/// Format untuk time
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="dateFormat">format spesifik waktu (default jika kosong: HH:mm)</param>
		/// <returns></returns>
		public static string FormatTime(this System.DateTime? dateTime, string dateFormat = "")
		{
			return dateTime.HasValue
				? dateTime.Value.FormatTime(dateFormat)
				: DEFAULT_EMPTY;
		}

		/// <summary>
		/// Format yes/no
		/// </summary>
		/// <param name="boolean"></param>
		/// <returns></returns>
		public static string Format(this bool boolean)
		{
			return boolean ? "yes" : "no";
		}

		/// <summary>
		/// Format yes/no
		/// </summary>
		/// <param name="boolean"></param>
		/// <returns></returns>
		public static string Format(this bool? boolean)
		{
			return boolean.HasValue
				? boolean.Value ? "yes" : "no"
				: DEFAULT_EMPTY;
		}
	}
}
