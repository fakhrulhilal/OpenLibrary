using System;
using System.Web.Mvc;
using OpenLibrary.Mvc.Helper;

namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Implementation of Post-Redirect-Get design pattern.
	/// Action GET -&gt; for viewing only (import ModelState if any).
	/// Action POST -&gt; only for handling form (export ModelState when redirecting).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class PostRedirectGetAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Key penanda uniq antar action (default: '/Area/Controller/Action')
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// PRG with default key: /Area/Controller/Action
		/// </summary>
		public PostRedirectGetAttribute()
		{ }

		/// <summary>
		/// PRG with certain key (POST action must match key in GET action)
		/// </summary>
		/// <param name="key"></param>
		public PostRedirectGetAttribute(string key)
		{
			Key = key;
		}

		/// <summary>
		/// Export/import ModelState
		/// </summary>
		/// <param name="filterContext"></param>
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var controller = filterContext.Controller;
			string method = filterContext.RequestContext.HttpContext.Request.HttpMethod.ToLower();
			string key = Key ?? "/" + string.Join("/", new[] 
			{ 
				controller.Routing(RoutingType.Area), 
				controller.Routing(RoutingType.Controller), 
				controller.Routing(RoutingType.Action)
			}).TrimStart(new[] { '/' });
			//backup ModelState jika error & hanya untuk method POST
			if ((filterContext.Result is RedirectResult || filterContext.Result is RedirectToRouteResult) &&
				!controller.ViewData.ModelState.IsValid &&
				method == "post")
			{

				controller.TempData[key] = controller.ViewData.ModelState;
			}
			else if (filterContext.Result is ViewResult && method == "get")
			{
				//cek import model state jika ada
				if (controller.TempData[key] != null)
					controller.ViewData.ModelState.Merge((ModelStateDictionary)controller.TempData[key]);
			}
		}
	}
}
