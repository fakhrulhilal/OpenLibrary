
namespace OpenLibrary.Annotation
{
	/// <summary>
	/// Define attribute that applied read only
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyAttribute : System.Attribute
	{ }
}
