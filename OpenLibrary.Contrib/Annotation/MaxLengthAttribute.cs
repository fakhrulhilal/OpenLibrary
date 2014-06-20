#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
	//TODO: implement MaxLengthAttribute
	[AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class MaxLengthAttribute : ValidationAttribute
	{
		public int Length { get; set; }

		public MaxLengthAttribute(int length)
		{
			Length = length;
		}

		public MaxLengthAttribute()
			: this(int.MaxValue)
		{ }
	}
}
#endif