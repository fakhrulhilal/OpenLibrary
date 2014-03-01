using System.Collections.Generic;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Ajax Helper
	/// </summary>
	public static class AjaxHelper
	{
		/// <summary>
		/// Ajax helper untuk menghandle respon ajax
		/// </summary>
		/// <typeparam name="T">tipe data input/output</typeparam>
		/// <param name="controller"></param>
		/// <param name="data">data respon balikan</param>
		/// <param name="success">menyatakan apakah operasi berhasil atau tidak</param>
		/// <param name="statusCode">kode status HTTP</param>
		/// <param name="html">additional HTML code for response</param>
		/// <param name="errors">field validasi error</param>
		/// <returns></returns>
		public static System.Web.Mvc.JsonResult Ajax<T>(this System.Web.Mvc.Controller controller, T data, bool success, int statusCode = 200, string html = "", Dictionary<string, string[]> errors = null)
			where T : class
		{
			controller.HttpContext.Response.StatusCode = statusCode;
			return new System.Web.Mvc.JsonResult 
			{
				JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
				ContentType = "application/json",
				Data = new
				{
					success = success,
					data = data,
					errors = errors,
					html = html
				},
				RecursionLimit = 5
			};
		}
	}
}
