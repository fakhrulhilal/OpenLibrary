
namespace OpenLibrary.Extension
{
	/// <summary>
	/// Extension for <see cref="System.String"/>
	/// </summary>
	public static class StringExtension
	{
		/// <summary>
		/// Get substring from right
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length">length of character from right</param>
		/// <returns></returns>
		public static string Right(this string data, int length)
		{
			if (string.IsNullOrEmpty(data))
				return string.Empty;
			return data.Length <= length ? data : data.Substring(data.Length - length);			
		}
	}
}
