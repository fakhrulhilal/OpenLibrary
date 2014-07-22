

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
	/// Helper for controller
	/// </summary>
	public static class ControllerHelper
	{
		// ReSharper disable EmptyGeneralCatchClause
		// ReSharper disable InconsistentNaming
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

		/// <summary>
		/// Absolute site URL
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		public static string GetApplicationUrl(this System.Web.Mvc.Controller controller)
		{
			return controller.GetHostUrl() + controller.Url.Content("~/");
		}

		/// <summary>
		/// Absolute site URL without base path
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		public static string GetHostUrl(this System.Web.Mvc.Controller controller)
		{
			return controller.Request.Url != null
				? controller.Request.Url.Scheme + "://" + controller.Request.ServerVariables["HTTP_HOST"]
				: string.Empty;
		}

		/// <summary>
		/// Get remote client IP
		/// </summary>
		/// <param name="controller"></param>
		/// <returns>Client IP</returns>
		[System.Obsolete("Use Controller.Request.IpAddress() instead")]
		public static string GetClientIpAddress(this System.Web.Mvc.Controller controller)
		{
			return controller.Request.IpAddress();
		}

		/// <summary>
		/// Get remote client IP
		/// </summary>
		/// <param name="request"></param>
		/// <returns>Client IP</returns>
		public static string IpAddress(this System.Web.HttpRequestBase request)
		{
			string ip = request.UserHostAddress ?? string.Empty;
			try
			{
				var temp = System.Net.Dns.GetHostAddresses(ip);
				foreach (var ipAddress in temp)
				{
					if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						return ipAddress.ToString().Trim();
				}
				foreach (var ipAddress in temp)
				{
					if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
						continue;
					var ipEntry = System.Net.Dns.GetHostEntry(ipAddress);
					foreach (var _ipEntry in ipEntry.AddressList)
					{
						if (_ipEntry.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
							return _ipEntry.ToString().Trim();
					}
				}
				if (temp.Length > 0)
					ip = temp[0].MapToIPv4().ToString();
			}
			catch (System.Exception) { }
			return ip;
		}

		/// <summary>
		/// Get remote hostname 
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="ip">IP address</param>
		/// <returns></returns>
		[System.Obsolete("Use Controller.Request.Hostname() instead")]
		public static string GetClientHostname(this System.Web.Mvc.Controller controller, string ip = "")
		{
			return controller.Request.Hostname(ip);
		}

		/// <summary>
		/// Get remote hostname 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="ip">IP address</param>
		/// <returns></returns>
		public static string Hostname(this System.Web.HttpRequestBase request, string ip = "")
		{
			try
			{
				if (string.IsNullOrEmpty(ip))
					ip = request.IpAddress();
				return System.Net.Dns.GetHostEntry(ip).HostName;
			}
			catch (System.Exception) { }
			return string.Empty;
		}

		/// <summary>
		/// Get raw request
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static string Raw(this System.Web.HttpRequestBase request)
		{
			return new System.IO.StreamReader(request.InputStream).ReadToEnd();
		}

#if NET40
		/// <summary>
		/// Implement missing extension of MapToIPv4
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <returns></returns>
		public static System.Net.IPAddress MapToIPv4(this System.Net.IPAddress ipAddress)
		{
			//TODO: implement MapToIPv4
			return new System.Net.IPAddress(0);
		}
#endif
	}
}
