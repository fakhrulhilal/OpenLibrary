
namespace OpenLibrary
{
	/// <summary>
	/// OpenLibrary error type
	/// </summary>
	public enum OpenLibraryErrorType
	{
		/// <summary>
		/// General error
		/// </summary>
		GlobalError,
		
		/// <summary>
		/// Error not triggered by OpenLibrary
		/// </summary>
		ExternalError,

		/// <summary>
		/// Argument not specified
		/// </summary>
		ArgumentNullError,

		/// <summary>
		/// Invalid argument
		/// </summary>
		ArgumentNotValidError,

		/// <summary>
		/// Failed while processing command
		/// </summary>
		OperationFailedError
	}

	/// <summary>
	/// General OpenLibraryException
	/// </summary>
	public class OpenLibraryException : System.Exception
	{
		/// <summary>
		/// Error type <see cref="OpenLibraryErrorType"/>
		/// </summary>
		public OpenLibraryErrorType ErrorType { get; private set; }

		/// <summary>
		/// Generate exception with error type only
		/// </summary>
		/// <param name="errorType">error type</param>
		public OpenLibraryException(OpenLibraryErrorType errorType = OpenLibraryErrorType.GlobalError)
		{
			ErrorType = errorType;
		}

		/// <summary>
		/// Generate exception with message &amp; error type
		/// </summary>
		/// <param name="errorMessage"></param>
		/// <param name="errorType"></param>
		public OpenLibraryException(string errorMessage, OpenLibraryErrorType errorType = OpenLibraryErrorType.GlobalError)
			: base(errorMessage)
		{
			ErrorType = errorType;
		}

		/// <summary>
		/// Generate exception with message, error type, and inner exception
		/// </summary>
		/// <param name="errorMessage">error message</param>
		/// <param name="exceptionObject">inner exception</param>
		/// <param name="errorType">error type</param>
		public OpenLibraryException(string errorMessage, System.Exception exceptionObject, OpenLibraryErrorType errorType = OpenLibraryErrorType.GlobalError)
			: base(errorMessage, exceptionObject)
		{
			ErrorType = errorType;
		}
	}
}
