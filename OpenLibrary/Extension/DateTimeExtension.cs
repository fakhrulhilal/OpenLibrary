using System.Linq;

namespace OpenLibrary.Extension
{
	/// <summary>
	/// Extension for <see cref="System.DateTime"/>
	/// </summary>
	public static class DateTimeExtension
	{
		/// <summary>
		/// Convert date time to SQL DateTime
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static string ToSqlDateTime(this System.DateTime dateTime)
		{
			return string.Format("cast('{0}' as datetime)", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
		}

		/// <summary>
		/// SQL Server minimum date time
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static System.DateTime SqlMinDateTime(this System.DateTime dateTime)
		{
			return System.DateTime.ParseExact("1753-01-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Check wether certain date is the end of the month
		/// </summary>
		/// <param name="date">tanggal yang akan dicek</param>
		/// <returns></returns>
		public static bool IsEndOfMonth(this System.DateTime date)
		{
			//untuk februari kabisat -> berakhir tanggal 29
			if (date.Year % 4 == 0 && date.Month == 2 && date.Day == 29)
				return true;
			//untuk februari non kabisat -> berakhir tanggal 28
			if (date.Year % 4 != 0 && date.Month == 2 && date.Day == 28)
				return true;
			//dari januari - juli, bulan ganjil berakhir tgl 31
			if (date.Month % 2 == 1 && date.Month <= 7 && date.Day == 31)
				return true;
			//dari januari - juli, bulan genap berakhir tgl 30
			if (date.Month % 2 == 0 && date.Month <= 7 && date.Day == 30)
				return true;
			//dari agustus - desember, bulan ganjil berakhir tgl 30
			if (date.Month % 2 != 0 && date.Month >= 8 && date.Day == 30)
				return true;
			//dari agustus - desember, bulan genap berakhir tgl 31
			if (date.Month % 2 == 0 && date.Month >= 8 && date.Day == 31)
				return true;
			return false;
		}

		/// <summary>
		/// Digunakan untuk mencari tanggal pertama kali untuk hari tertentu pada bulan yg ditentukan.
		/// </summary>
		/// <param name="month">bulang pencarian tanggal pertama</param>
		/// <param name="day">hari (senin - minggu)</param>
		/// <returns></returns>
		public static System.DateTime FirstDayWeekOfMonth(this System.DateTime month, System.DayOfWeek day)
		{
			//cari untuk pekan pertama saja
			for (int i = 1; i <= 7; i++)
			{
				var date = new System.DateTime(month.Year, month.Month, i);
				if (date.DayOfWeek == day)
					return date.Date;
			}
			return month.Date;
		}

		/// <summary>
		/// Get next week for certain day of week
		/// </summary>
		/// <param name="date"></param>
		/// <param name="weeks">how many week</param>
		/// <param name="dayTarget">day of week</param>
		/// <returns></returns>
		public static System.DateTime NextWeek(this System.DateTime date, int weeks, System.DayOfWeek? dayTarget = null)
		{
			var day = dayTarget.HasValue ? dayTarget.Value : date.DayOfWeek;
			var dayToBeSearch = date;
			//cari tanggal untuk hari
			for (int i = 0; i <= 7; i++)
			{
				var tempDay = date.AddDays(i);
				if (tempDay.DayOfWeek == day)
				{
					dayToBeSearch = tempDay;
					break;
				}
			}
			return dayToBeSearch.AddDays(7 * weeks);
		}

		/// <summary>
		/// Get previous week for certain day of week
		/// </summary>
		/// <param name="date"></param>
		/// <param name="weeks">how many week</param>
		/// <param name="dayTarget">day of week</param>
		/// <returns></returns>
		public static System.DateTime BeforeWeek(this System.DateTime date, int weeks, System.DayOfWeek? dayTarget = null)
		{
			var day = dayTarget.HasValue ? dayTarget.Value : date.DayOfWeek;
			var dayToBeSearch = date;
			//cari tanggal untuk hari
			for (int i = 0; i <= 7; i++)
			{
				var tempDay = date.AddDays(-i);
				if (tempDay.DayOfWeek == day)
				{
					dayToBeSearch = tempDay;
					break;
				}
			}
			return dayToBeSearch.AddDays(-(7 * weeks));
		}

		/// <summary>
		/// Get total day for day of week for a certain range date
		/// </summary>
		/// <param name="startDate">start range</param>
		/// <param name="endDate">end range</param>
		/// <param name="days">look for these day of week</param>
		/// <returns></returns>
		public static int TotalDays(System.DateTime startDate, System.DateTime endDate, System.DayOfWeek[] days)
		{
			int total = 0;
			var day = startDate;
			while (day <= endDate)
			{
				if (days.Contains(day.DayOfWeek))
					total++;
				day = day.AddDays(1);
			}
			return total;
		}
	}
}
