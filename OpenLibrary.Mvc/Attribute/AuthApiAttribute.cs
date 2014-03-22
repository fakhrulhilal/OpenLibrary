using System.Linq;
using System.Net;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Runtime.Caching;
using OpenLibrary.Mvc.Utility;

namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Use custom authorization based on public key and secret key for Web API.
	/// Unauthorized user will be brought to ViewResult with the name "UnauthorizedAccess".
	/// </summary>
	public class AuthApiAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		private string apiKeyHeader;
		private string authenticationScheme;
		private string unauthorizedMessage;

		/// <summary>
		/// API key provider for providing secret key based on public key
		/// </summary>
		public IApiAuthProvider Provider { get; set; }

		/// <summary>
		/// Validity period of signature in minute
		/// </summary>
		public int ValidityPeriod { get; set; }

		/// <summary>
		/// Authentication scheme (default: ApiAuth)
		/// </summary>
		public string AuthenticationScheme
		{
			get { return authenticationScheme ?? "ApiAuth"; }
			set { authenticationScheme = value; }
		}

		/// <summary>
		/// Public key header (default: X-PublicKey)
		/// </summary>
		public string ApiKeyHeader
		{
			get { return apiKeyHeader ?? "X-PublicKey"; }
			set { apiKeyHeader = value; }
		}

		/// <summary>
		/// Message for unauthorized request
		/// </summary>
		public string UnauthorizedMessage
		{
			get { return unauthorizedMessage ?? "Unauthorized Request"; }
			set { unauthorizedMessage = value; }
		}

		/// <summary>
		/// Auth API
		/// </summary>
		/// <param name="provider">typeof <see cref="IApiAuthProvider"/> implementation. This implementation must provide parameterless constructor</param>
		public AuthApiAttribute(System.Type provider)
		{
			if (provider == null)
				throw new System.ArgumentNullException("provider", "Provider must be defined.");
			if (!typeof(IApiAuthProvider).IsAssignableFrom(provider))
				throw new System.ArgumentException("API key provider must implement IApiAuthProvider.");
			try
			{
				//try create instance normally
				Provider = (IApiAuthProvider)System.Activator.CreateInstance(provider);
			}
			catch (System.MissingMethodException)
			{
				//search using Dependency Injection
				Provider = (IApiAuthProvider)ObjectFactory.GetMvcInstance(provider);
			}
		}

		/// <summary>
		/// Auth API
		/// </summary>
		/// <param name="provider">typeof <see cref="IApiAuthProvider"/> implementation. This implementation must provide parameterless constructor</param>
		public AuthApiAttribute(IApiAuthProvider provider)
		{
			if (provider != null)
				Provider = provider;
			else
				throw new System.ArgumentNullException("provider", "Provider must be defined.");
		}

		private bool IsValidTime(HttpRequestMessage request)
		{
			if (!request.Headers.Date.HasValue)
				return false;
			var utcNow = System.DateTime.UtcNow;
			var utcRequest = request.Headers.Date.Value.UtcDateTime;
			return utcNow.AddMinutes(-ValidityPeriod) <= utcRequest && utcRequest <= utcNow.AddMinutes(ValidityPeriod);
		}

		private bool IsAuthenticated(HttpActionContext actionContext)
		{
			var request = actionContext.Request;
			var headers = request.Headers;
			//check for all required auth validation
			if (!headers.Contains(ApiKeyHeader))
				return false;
			if (!IsValidTime(request))
				return false;
			if (headers.Authorization == null || headers.Authorization.Scheme != AuthenticationScheme)
				return false;
			string signature = headers.Authorization.Parameter;
			//forbid for same signature to be sent more than once
			if (MemoryCache.Default.Contains(signature))
				return false;
			string apiKey = headers.GetValues(ApiKeyHeader).FirstOrDefault();
			if (string.IsNullOrEmpty(apiKey) || !Provider.IsValidApiKey(apiKey))
				return false;
			string verifiedSignature = Provider.Signature(apiKey, request);
			if (verifiedSignature == signature)
			{
				//save signature to cache for a certain validity period
				MemoryCache.Default.Add(signature, apiKey, System.DateTimeOffset.UtcNow.AddMinutes(ValidityPeriod));
				return true;
			}
			return false;
		}

		/// <summary>
		/// Authentication handler. Return HTTP 401 Unauthorized when failed.
		/// </summary>
		/// <param name="actionContext"></param>
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (!IsAuthenticated(actionContext))
			{
				var response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, UnauthorizedMessage);
				response.Headers.WwwAuthenticate.Add(new System.Net.Http.Headers.AuthenticationHeaderValue(AuthenticationScheme));
				actionContext.Response = response;
			}
			else
			{
				base.OnActionExecuting(actionContext);
			}
		}
	}
}
