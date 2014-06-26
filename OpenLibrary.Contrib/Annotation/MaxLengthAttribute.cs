#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
	//TODO: implement MaxLengthAttribute
	/// <summary>
	/// Specifies the maximum length of array or string data allowed in a property.
	/// </summary>
	[AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class MaxLengthAttribute : ValidationAttribute
	{
		/// <summary>
		/// Gets the maximum allowable length of the array or string data.
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// Initializes a new instance of the MaxLengthAttribute class based on the length parameter.
		/// </summary>
		/// <param name="length">The maximum allowable length of array or string data.</param>
		public MaxLengthAttribute(int length)
		{
			Length = length;
		}

		/// <summary>
		/// The maximum allowable length supported by the database will be used.
		/// </summary>
		public MaxLengthAttribute()
			: this(int.MaxValue)
		{ }
	}
}
#endif