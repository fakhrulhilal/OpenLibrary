#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
	/// <summary>
	/// Specifies the database table that a class is mapped to.
	/// </summary>
	[AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = false)]
	public class TableAttribute : Attribute
	{
		/// <summary>
		/// Gets the name of the table the class is mapped to.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the schema of the table the class is mapped to.
		/// </summary>
		public string Schema { get; set; }

		/// <summary>
		/// Initializes a new instance of the TableAttribute class using the specified name of the table.
		/// </summary>
		/// <param name="name">The name of the table the class is mapped to.</param>
		public TableAttribute(string name)
		{
			Name = name;
		}
	}
}
#endif