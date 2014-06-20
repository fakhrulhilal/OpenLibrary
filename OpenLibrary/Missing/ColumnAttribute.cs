#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
	/// <summary>
	/// Represents the database column that a property is mapped to.
	/// </summary>
	[AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class ColumnAttribute : Attribute
	{
		/// <summary>
		/// Gets the name of the column the property is mapped to.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the zero-based order of the column the property is mapped to.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Gets or sets the database provider specific data type of the column the property is mapped to.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Initializes a new instance of the ColumnAttribute class.
		/// </summary>
		/// <param name="name">The name of the column the property is mapped to.</param>
		public ColumnAttribute(string name)
		{
			Name = name;
		}
	}
}
#endif