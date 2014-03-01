using System.Collections.Generic;
using System.Web.Mvc;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Generate breadcrumb compatible with Twitter Bootstrap
	/// </summary>
	public static class BreadcrumbHelper
	{
		/// <summary>
		/// Generate breadcrumb compatible with Twitter Bootstrap
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="cssClassess">additional css class</param>
		/// <param name="homeCaption">default home caption</param>
		/// <returns></returns>
		public static MvcHtmlString Breadcrumb(this HtmlHelper helper, string[] cssClassess = null, string homeCaption = "Home")
		{
			string childFormat = @"<li><a href=""{0}"">{1}</a> <span class=""divider"">{2}</span></li>";
			var htmlContainer = new TagBuilder("ul");
			var html = new List<string>();
			htmlContainer.AddCssClass("breadcrumb");
			if (cssClassess != null && cssClassess.Length > 0)
				foreach (var css in cssClassess)
					htmlContainer.AddCssClass(css);
			//add home menu
			html.Insert(0, string.Format(childFormat, "/", homeCaption, "/"));
			var currentLeafMenu = helper.GetCurrentMenu();
			if (currentLeafMenu != null)
			{
				var parentMenu = helper.GetParentMenu(currentLeafMenu.Id);
				html.Insert(1, "<li class=\"active\">" + currentLeafMenu.Caption + "</li>");
				//search for navigation from current menu to root menu
				do
				{
					html.Insert(1, string.Format(childFormat, "#", parentMenu.Caption, "/"));
					parentMenu = helper.GetParentMenu(parentMenu.Id);
					if (parentMenu == null)
						break;
				} while (true);
			}
			htmlContainer.InnerHtml = string.Join("", html);
			return new MvcHtmlString(htmlContainer.ToString());
		}
	}
}
