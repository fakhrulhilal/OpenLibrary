
namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Define allowed character if IllegalCharacterModelBinder is registered.
	/// See also <seealso cref="OpenLibrary.Mvc.ModelBinding.IllegalCharacterModelBinder"/>
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
	public class AllowCharacterAttribute : System.Attribute
	{
		private string errorMessage;

		/// <summary>
		/// List of allowed characters
		/// </summary>
		public string LegalCharacters { get; set; }

		/// <summary>
		/// Specify error message (index 0 is model name, 1 is illegal characters)
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return string.IsNullOrEmpty(errorMessage)
						   ? "{0} contains illegal characters, i.e. one of {1}."
						   : errorMessage;
			}
			set { errorMessage = value; }
		}

		/// <summary>
		/// Exclude character explicitly when using IllegalCharacterModelBinding
		/// </summary>
		/// <param name="legalCharacters">whitelist characters</param>
		public AllowCharacterAttribute(string legalCharacters)
		{
			LegalCharacters = legalCharacters;
		}

		/// <summary>
		/// Exclude all characters explicitly when using IllegalCharacterModelBinding
		/// </summary>
		public AllowCharacterAttribute()
			: this("")
		{ }
	}
}
