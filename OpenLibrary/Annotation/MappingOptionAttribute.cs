
namespace OpenLibrary.Annotation
{
	/// <summary>
	/// Set mapping option for export/import to/from file
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class MappingOptionAttribute : System.Attribute
	{
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		/// <summary>
		/// Caption in file.
		/// Using <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/> or property name when not specified.
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Property name in class.
		/// </summary>
		public string Field { get; private set; }

		/// <summary>
		/// Width of column in file (applied only for excel).
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Sequence column order in file from left (applied only when exporting).
		/// </summary>
		public int? Sequence { get; set; }

		/// <summary>
		/// Data type (applied only when using dictionary instead of entity)
		/// </summary>
		public System.Type Type { get; set; }

		/// <summary>
		/// Set mapping option
		/// </summary>
		/// <param name="caption">caption in file</param>
		/// <param name="width">width of column in file</param>
		/// <param name="sequence">sequence column order in file from left</param>
		public MappingOptionAttribute(string caption, int width, int sequence)
			: this(caption, sequence)
		{
			this.Width = width;
		}

		/// <summary>
		/// Set mapping option
		/// </summary>
		/// <param name="caption">caption in file</param>
		/// <param name="sequence">sequence column order in file from left</param>
		public MappingOptionAttribute(string caption, int sequence)
			: this(caption)
		{
			this.Sequence = sequence;
		}

		/// <summary>
		/// Set mapping option
		/// </summary>
		/// <param name="caption">caption in file</param>
		public MappingOptionAttribute(string caption)
		{
			this.Caption = caption;
		}
	}

	/// <summary>
	/// Column &lt;-&gt; Field export/import definition
	/// </summary>
	public class MappingOption
	{
		/// <summary>
		/// Caption name shown on first row in file
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Property name of object/class
		/// </summary>
		public string Field { get; set; }

		/// <summary>
		/// Width of column (applied to excel)
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Column priority, the smaller the initial display (from left to right)
		/// </summary>
		public int? Sequence { get; set; }

		/// <summary>
		/// Data type (applied only when using dictionary instead of entity class)
		/// </summary>
		public System.Type Type { get; set; }

		public override string ToString()
		{
			return string.Format("{0} -> {1}", Field, Caption);
		}
	}
}
