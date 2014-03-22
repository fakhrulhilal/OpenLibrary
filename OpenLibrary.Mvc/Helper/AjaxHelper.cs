using System.Collections.Generic;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Ajax Helper
	/// </summary>
	public static class AjaxHelper
	{
		/// <summary>
		/// Ajax helper for sending ajax response.
		/// Data contain 4 keys:
		/// 1. success (boolean): indicate operation status
		/// 2. data (object): the data to be sent
		/// 3. errors (hash array): key value pair of errors (if any), the key is property of model class
		/// 4. html (string): html page (instead of object in data)
		/// </summary>
		/// <typeparam name="T">data type of input/output</typeparam>
		/// <param name="controller"></param>
		/// <param name="data">response data</param>
		/// <param name="success">indicate operation status</param>
		/// <param name="statusCode">HTTP status code</param>
		/// <param name="html">additional HTML code for response</param>
		/// <param name="errors">validation error fields</param>
		/// <returns></returns>
		public static System.Web.Mvc.JsonResult Ajax<T>(this System.Web.Mvc.Controller controller, T data, bool success, int statusCode = 200, string html = "", Dictionary<string, string[]> errors = null)
			where T : class
		{
			controller.HttpContext.Response.StatusCode = statusCode;
			return new System.Web.Mvc.JsonResult 
			{
				JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
				ContentType = "application/json",
				Data = new { success, data, errors, html },
				RecursionLimit = 5
			};
		}
	}
}
