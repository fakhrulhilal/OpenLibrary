
namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Extension for HTTP
	/// </summary>
	public static class HttpHelper
	{
		private const string HTTP_CONTEXT = "MS_HttpContext";
		private const string REMOTE_ENDPOINT_MESSAGE = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

		/// <summary>
		/// Get current client IP Address
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static string GetClientIpAddress(this System.Net.Http.HttpRequestMessage request)
		{
			if (request.Properties.ContainsKey(HTTP_CONTEXT))
			{
				dynamic ctx = request.Properties[HTTP_CONTEXT];
				if (ctx != null)
					return ctx.Request.UserHostAddress;
			}

			if (request.Properties.ContainsKey(REMOTE_ENDPOINT_MESSAGE))
			{
				dynamic remoteEndpoint = request.Properties[REMOTE_ENDPOINT_MESSAGE];
				if (remoteEndpoint != null)
					return remoteEndpoint.Address;
			}

			return null;
		}

		/// <summary>
		/// Get current client IP Address
		/// </summary>
		/// <param name="controller"></param>
		/// <returns></returns>
		public static string GetClientIpAddress(this System.Web.Http.ApiController controller)
		{
			return controller == null ? string.Empty : GetClientIpAddress(controller.Request);
		}
	}
}
