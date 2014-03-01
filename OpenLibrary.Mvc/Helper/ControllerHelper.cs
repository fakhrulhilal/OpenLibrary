
namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Routing type
	/// </summary>
	public enum RoutingType
	{
		/// <summary>
		/// Area route name
		/// </summary>
		Area,

		/// <summary>
		/// Controller route name
		/// </summary>
		Controller,

		/// <summary>
		/// Action route name
		/// </summary>
		Action
	}

	/// <summary>
	/// Helper for routing
	/// </summary>
	public static class ControllerHelper
	{
		/// <summary>
		/// Get current routing value
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="type">Requested routing type</param>
		/// <returns>string</returns>
		public static string Routing(this System.Web.Mvc.HtmlHelper helper, RoutingType type)
		{
			return Routing(helper.ViewContext.RouteData, type);
		}

		/// <summary>
		/// Get current routing value
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="type">Requested routing type</param>
		/// <returns>string</returns>
		public static string Routing(this System.Web.Mvc.Controller controller, RoutingType type)
		{
			return Routing(controller.RouteData, type);
		}

		/// <summary>
		/// Get current routing value
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="type">Requested routing type</param>
		/// <returns>string</returns>
		public static string Routing(this System.Web.Mvc.ControllerBase controller, RoutingType type)
		{
			return Routing(controller.ControllerContext.RouteData, type);
		}

		/// <summary>
		/// Get current routing value
		/// </summary>
		/// <param name="routeData"></param>
		/// <param name="type">Requested routing type</param>
		/// <returns>string</returns>
		public static string Routing(this System.Web.Routing.RouteData routeData, RoutingType type)
		{
			try
			{
				return type == RoutingType.Area
					? routeData.DataTokens[type.ToString().ToLower()].ToString()
					: routeData.Values[type.ToString().ToLower()].ToString();
			}
			catch (System.NullReferenceException)
			{
				return string.Empty;
			}
		}
	}
}
