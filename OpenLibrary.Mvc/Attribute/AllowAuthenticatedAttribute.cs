
namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Represents the attribute that marks controllers and actions to skip <see cref="AccessRightAttribute"/> when authenticated.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple = false)]
	public class AllowAuthenticatedAttribute : System.Attribute
	{ }
}
