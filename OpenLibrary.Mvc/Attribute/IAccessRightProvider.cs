
namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Provider for <see cref="AccessRightAttribute"/>
	/// </summary>
	public interface IAccessRightProvider
	{
		/// <summary>
		/// Determine wether authenticated user has right to access object or not
		/// </summary>
		/// <param name="username">logged on username</param>
		/// <param name="accessObject">object being accessed</param>
		/// <param name="accessRight">access right requested to object</param>
		/// <returns></returns>
		bool IsAuthorized(string username, string accessObject, int accessRight);
	}
}
