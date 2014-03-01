
namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Provider for <see cref="AuthApiAttribute"/>
	/// </summary>
	public interface IApiAuthProvider
	{
		/// <summary>
		/// Determine wether API key is valid or not
		/// </summary>
		/// <param name="apiKey">API key from HTTP header</param>
		/// <returns></returns>
		bool IsValidApiKey(string apiKey);

		/// <summary>
		/// Build signature based on HTTP request
		/// </summary>
		/// <param name="apiKey">API key from HTTP header</param>
		/// <param name="request">HTTP request</param>
		/// <returns></returns>
		string Signature(string apiKey, System.Net.Http.HttpRequestMessage request);
	}
}
