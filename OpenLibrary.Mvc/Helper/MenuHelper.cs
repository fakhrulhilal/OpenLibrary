using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Navigation menu entity
	/// </summary>
	public class Menu
	{
		/// <summary>
		/// Identity key
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Caption
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Link URL
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Sequence for display, smaller will be shown first
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// Parent menu ID
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Determine wether menu is active or not. Non active menu will not be shown
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Determine wether menu is visible or not in interface
		/// </summary>
		public bool IsVisible { get; set; }

		/// <summary>
		/// Parent menu (will be build automatically)
		/// </summary>
		public Menu Parent { get; set; }

		/// <summary>
		/// Menu children (will be beuild automatically)
		/// </summary>
		public List<Menu> ChildMenus { get; set; }

		/// <summary>
		/// Translated version of <seealso cref="Caption"/> of current culture
		/// </summary>
		public string TranslatedCaption { get; set; }
	}

	/// <summary>
	/// Helper for generating menu compatible with Twitter Bootstrap navigation menu
	/// </summary>
	public static class MenuHelper
	{
		private static List<Menu> RootMenus { get; set; }
		private static List<Menu> AllMenus { get; set; }
		private static string BaseUrl { get; set; }

		private static string GetBaseUrl(this HtmlHelper helper)
		{
			if (string.IsNullOrEmpty(BaseUrl))
				BaseUrl = UrlHelper.GenerateContentUrl("~/", helper.ViewContext.HttpContext).TrimEnd(new char[] { '/' });
			return BaseUrl;
		}

		private static List<Menu> GetAllMenus(this HtmlHelper helper)
		{
			return (from m in AllMenus
					where m.IsVisible
					orderby m.ParentId ascending, m.Priority ascending
					select m).ToList();
		}

		/// <summary>
		/// Get all root menu
		/// </summary>
		/// <returns></returns>
		public static List<Menu> GetAllRootMenu(this HtmlHelper helper)
		{
			//root menu is all menu that has no parent
			return RootMenus ?? (RootMenus = (from rm in GetAllMenus(helper)
											  where !rm.ParentId.HasValue
											  select rm).ToList());
		}

		/// <summary>
		/// Get parent menu
		/// </summary>
		/// <param name="helper">bind to this HTML helper</param>
		/// <param name="childId">child menu id</param>
		/// <returns>Menu</returns>
		public static Menu GetParentMenu(this HtmlHelper helper, int childId)
		{
			var childMenu = AllMenus.FirstOrDefault(m => m.Id == childId);
			if (childMenu == null)
				return null;
			return AllMenus.FirstOrDefault(m => m.Id == childMenu.ParentId);
		}

		/// <summary>
		/// Get root menu from spesific menu
		/// </summary>
		/// <param name="id">id of menu to be searched for its root menu</param>
		/// <returns></returns>
		public static Menu GetRootMenu(int id)
		{
			Menu parent = AllMenus.FirstOrDefault(m => m.Id == id);
			if (parent != null && parent.ParentId.HasValue)
				parent = GetRootMenu(parent.ParentId.Value);
			return parent;
		}

		/// <summary>
		/// Get root menu from spesific menu
		/// </summary>
		/// <param name="menu">menu to be searched for its root menu</param>
		/// <returns></returns>
		public static Menu GetRootMenu(Menu menu)
		{
			return GetRootMenu(menu.Id);
		}

		/// <summary>
		/// Get current menu from routing value
		/// </summary>
		/// <param name="helper">bind to this HTML helper</param>
		/// <returns>Menu</returns>
		public static Menu GetCurrentMenu(this HtmlHelper helper)
		{
			var allMenus = GetAllMenus(helper);
			string areaName = ControllerHelper.Routing(helper, RoutingType.Area);
			string controllerName = ControllerHelper.Routing(helper, RoutingType.Controller);
			//generate routing untuk posisi sekarang
			string route = UrlHelper.GenerateUrl(null, null, controllerName,
												 new RouteValueDictionary(new { area = areaName }), helper.RouteCollection,
												 helper.ViewContext.Controller.ControllerContext.RequestContext, true);
			//base URL tidak disimpan dalam database menu
			route = Regex.Replace(route ?? "", "^(" + GetBaseUrl(helper) + ")", "", RegexOptions.IgnoreCase);
			Menu menu = allMenus.FirstOrDefault(m => m.Url.Equals(route, StringComparison.InvariantCultureIgnoreCase));
			return menu;
		}

		/// <summary>
		/// Get current root menu from current controller
		/// </summary>
		/// <param name="helper">bind to this HTML helper</param>
		/// <returns>Menu</returns>
		public static Menu GetCurrentRootMenu(this HtmlHelper helper)
		{
			var menu = GetCurrentMenu(helper);
			return menu != null
					   ? GetRootMenu(menu.Id)
					   : null;
		}

		/// <summary>
		/// Get child menu
		/// </summary>
		/// <param name="parentId">parent menu id</param>
		/// <param name="recursive">determine wether search for child menu recursively</param>
		/// <returns></returns>
		public static List<Menu> GetChildMenu(int parentId, bool recursive = true)
		{
			var parent = AllMenus.FirstOrDefault(m => m.Id == parentId);
			if (parent == null)
				return new List<Menu>();
			var childMenus = (from cm in AllMenus
							  where cm.ParentId == parentId
							  select cm).ToList();
			parent.ChildMenus = childMenus;
			if (recursive)
			{
				foreach (var childMenu in childMenus)
				{
					childMenu.Parent = parent;
					childMenu.ChildMenus = GetChildMenu(childMenu.Id);
				}
			}
			return childMenus;
		}

		private static MvcHtmlString BuildChildSubMenu(int parentId, bool recursive = true, string baseUrl = "")
		{
			var html = new StringBuilder();
			var parent = AllMenus.FirstOrDefault(m => m.Id == parentId);
			if (parent == null || !parent.IsActive)
				return new MvcHtmlString(string.Empty);
			var childMenus = GetChildMenu(parent.Id);
			if (childMenus.Count > 0)
			{
				var tagParent = new TagBuilder("li");
				var tagAnchorParent = new TagBuilder("a");
				tagParent.AddCssClass("dropdown-submenu");
				tagAnchorParent.Attributes.Add("href", "#");
				tagAnchorParent.InnerHtml = parent.TranslatedCaption;
				tagParent.InnerHtml += tagAnchorParent.ToString();
				var tagContainerChild = new TagBuilder("ul");
				tagContainerChild.AddCssClass("dropdown-menu");
				foreach (var childMenu in childMenus)
				{
					if (!childMenu.IsActive)
						continue;
					var htmlChild = new StringBuilder();
					if (childMenu.ChildMenus.Count > 0)
					{
						htmlChild.Append((object)BuildChildSubMenu(childMenu.Id, recursive, baseUrl));
					}
					else
					{
						var tagChild = new TagBuilder("li");
						var tagAnchorChild = new TagBuilder("a");
						tagAnchorChild.Attributes.Add("href", baseUrl + childMenu.Url);
						tagAnchorChild.InnerHtml = childMenu.TranslatedCaption;
						tagChild.InnerHtml = tagAnchorChild.ToString();
						htmlChild.Append((object)tagChild);
					}
					if (htmlChild.Length > 0)
						tagContainerChild.InnerHtml += htmlChild.ToString();
				}
				tagParent.InnerHtml += tagContainerChild.ToString();
				html.Append((object)tagParent);
			}
			return new MvcHtmlString(html.Length > 0 ? html.ToString() : string.Empty);
		}

		/// <summary>
		/// Build HTML child menu from root menu
		/// </summary>
		/// <param name="helper">bind to this HTML helper</param>
		/// <param name="rootMenuId">root menu</param>
		/// <param name="setActiveRootMenu">default root menu to be set active</param>
		/// <param name="isShowEmptyChildMenu">wether to display empty parent which has no child or not</param>
		/// <returns>MvcHtmlString</returns>
		public static MvcHtmlString BuildChildMenu(this HtmlHelper helper, int rootMenuId, bool setActiveRootMenu = false, bool isShowEmptyChildMenu = false)
		{
			var allMenu = GetAllMenus(helper);
			var html = new StringBuilder();
			var parent = allMenu.FirstOrDefault<Menu>(m => m.Id == rootMenuId);
			List<Menu> childMenus = GetChildMenu(rootMenuId);
			if (parent == null || 
				!parent.IsActive ||
				(childMenus.Count < 1 && !isShowEmptyChildMenu && (string.IsNullOrEmpty(parent.Url) || parent.Url.Trim() == "#")))
				return new MvcHtmlString(string.Empty);
			var tagParent = new TagBuilder("li");
			var tagAnchorParent = new TagBuilder("a");
			if (setActiveRootMenu)
				tagParent.AddCssClass("active");
			//untuk default menu yang tidak punya submenu
			tagAnchorParent.Attributes.Add("href", GetBaseUrl(helper) + parent.Url);
			tagAnchorParent.InnerHtml = parent.TranslatedCaption;
			//jika menu tersebut punya submenu
			if (childMenus.Count > 0)
			{
				tagParent.AddCssClass("dropdown");
				tagAnchorParent.Attributes["href"] = "#";
				tagAnchorParent.AddCssClass("dropdown-toggle");
				tagAnchorParent.Attributes.Add("data-toggle", "dropdown");
				tagAnchorParent.InnerHtml += " <b class=\"caret\"></b>";
				tagParent.InnerHtml += tagAnchorParent.ToString();
				var tagContainerChild = new TagBuilder("ul");
				tagContainerChild.AddCssClass("dropdown-menu");
				tagContainerChild.Attributes.Add("role", "menu");
				foreach (var childMenu in childMenus)
				{
					if (!childMenu.IsActive)
						continue;
					var htmlChild = new StringBuilder();
					if (childMenu.ChildMenus.Count > 0)
					{
						htmlChild.Append((string)BuildChildSubMenu(childMenu.Id, true, GetBaseUrl(helper)).ToString());
					}
					else
					{
						var tagChild = new TagBuilder("li");
						var tagAnchorChild = new TagBuilder("a");
						tagAnchorChild.Attributes.Add("href", GetBaseUrl(helper) + childMenu.Url);
						tagAnchorChild.InnerHtml = childMenu.TranslatedCaption;
						tagChild.InnerHtml = tagAnchorChild.ToString();
						htmlChild.Append(tagChild);
					}
					if (htmlChild.Length > 0)
						tagContainerChild.InnerHtml += htmlChild.ToString();
					//html.Append(htmlChild.ToString());
				}

				tagParent.InnerHtml += tagContainerChild.ToString();
			}
			else
			{
				tagParent.InnerHtml += tagAnchorParent.ToString();
			}
			html.Append((object)tagParent);
			return new MvcHtmlString(html.Length > 0 ? html.ToString() : string.Empty);
		}

		/// <summary>
		/// Build HTML menu tree from root to leaf node
		/// </summary>
		/// <param name="helper">bind to this HTML helper</param>
		/// <param name="menus">hierarchyal menu list</param>
		/// <param name="currentRootMenuId">current active root menu Id</param>
		/// <param name="cssClasses">CSS class to be added in root menu</param>
		/// <param name="isShowEmptyChildMenu">wether to display empty parent which has no child or not</param>
		/// <returns>MvcHtmlString</returns>
		public static MvcHtmlString BuildMenuTree(this HtmlHelper helper, List<Menu> menus, int? currentRootMenuId = null, string[] cssClasses = null, bool isShowEmptyChildMenu = false)
		{
			AllMenus = menus;
			var tagRoot = new TagBuilder("ul");
			tagRoot.AddCssClass("nav");
			if (cssClasses != null && cssClasses.Length > 0)
				Array.ForEach(cssClasses, tagRoot.AddCssClass);
			if (AllMenus.Count < 1)
				return new MvcHtmlString(tagRoot.ToString());
			var allRootMenus = GetAllRootMenu(helper);
			if (!currentRootMenuId.HasValue)
			{
				var currentRootMenu = GetCurrentRootMenu(helper);
				if (currentRootMenu != null)
					currentRootMenuId = currentRootMenu.Id;
			}
			foreach (var rootMenu in allRootMenus.Where(rootMenu => rootMenu.IsActive).ToList())
				tagRoot.InnerHtml += BuildChildMenu(helper, rootMenu.Id, currentRootMenuId.HasValue && currentRootMenuId.Value == rootMenu.Id, isShowEmptyChildMenu).ToString();
			return new MvcHtmlString(tagRoot.ToString());
		}
	}
}
