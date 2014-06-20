#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
	/// <summary>
	/// Denotes that a property or class should be excluded from database mapping.
	/// </summary>
	[AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class NotMappedAttribute : Attribute
	{ }
}
#endif