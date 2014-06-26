#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
	/// <summary>
	/// Specifies how the database generates values for a property.
	/// </summary>
	[AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class DatabaseGeneratedAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the pattern used to generate values for the property in the database.
		/// </summary>
		public DatabaseGeneratedOption DatabaseGeneratedOption { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the DatabaseGeneratedAttribute class.
		/// </summary>
		/// <param name="option">Gets or sets the pattern used to generate values for the property in the database.</param>
		public DatabaseGeneratedAttribute(DatabaseGeneratedOption option)
		{
			DatabaseGeneratedOption = option;
		}
	}

	/// <summary>
	/// Represents the pattern used to generate values for a property in the database.
	/// </summary>
	public enum DatabaseGeneratedOption
	{
		/// <summary>
		/// The database generates a value when a row is inserted or updated.
		/// </summary>
		Computed,

		/// <summary>
		/// The database generates a value when a row is inserted.
		/// </summary>
		Identity,

		/// <summary>
		/// The database does not generate values.
		/// </summary>
		None
	}
}
#endif