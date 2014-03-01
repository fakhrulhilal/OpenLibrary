
namespace OpenLibrary.Extension
{
	/// <summary>
	/// Extension for <see cref="System.Exception"/>
	/// </summary>
	public static class ExceptionExtension
	{
		/// <summary>
		/// Get message of leaf inner exception
		/// </summary>
		/// <param name="exception"></param>
		/// <returns></returns>
		public static string GetLeafMessage(this System.Exception exception)
		{
			var leafException = GetLeafException(exception);
			return leafException == null ? string.Empty : leafException.Message;
		}

		/// <summary>
		/// Get leaf inner exception
		/// </summary>
		/// <param name="exception"></param>
		/// <returns></returns>
		public static System.Exception GetLeafException(this System.Exception exception)
		{
			return exception.InnerException == null ? exception : GetLeafException(exception.InnerException);
		}
	}
}
