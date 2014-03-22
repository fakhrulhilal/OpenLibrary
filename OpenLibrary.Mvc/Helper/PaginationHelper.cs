using System.Collections.Generic;
using System.Web.Mvc;
using OpenLibrary.Extension;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Helper for generating pagination compatible with Twitter Bootstrap
	/// </summary>
	public static class PaginationHelper
	{
		private const string PREFIX = "PaginationHelper_";

		/// <summary>
		/// HTML template for pagination container.
		/// {0} -> first page <see cref="HtmlFirstStyle"/>
		/// {1} -> previous page <see cref="HtmlPrevStyle"/>
		/// {2} -> all page <see cref="HtmlPageStyle"/>
		/// {3} -> next page <see cref="HtmlNextStyle"/>
		/// {4} -> last page <see cref="HtmlLastStyle"/>
		/// </summary>
		public static string HtmlPaginationStyle = @"<ul class=""pagination pagination-right"">{0} {1} {2} {3} {4}</ul>";

		/// <summary>
		/// HTML template for each page.
		/// {0} -> URL to page
		/// {1} -> page number
		/// </summary>
		public static string HtmlPageStyle = @"<li><a href='{0}'>{1}</a></li>";

		/// <summary>
		/// HTML template for current active page.
		/// {0} -> active page number
		/// </summary>
		public static string HtmlCurrentPageStyle = @"<li class='active'><span>{0}</span></li>";

		/// <summary>
		/// HTML template when page over than maximum page per group
		/// </summary>
		public static string HtmlMorePageStyle = @"<li class='disabled'><span>...</span></li>";

		/// <summary>
		/// HTML template for previous page.
		/// {0} -> URL to previous page
		/// </summary>
		public static string HtmlPrevStyle = @"<li><a class='prev-page' href='{0}' title='previous page'>&lt;</a></li>";

		/// <summary>
		/// HTML template for next page.
		/// {0} -> URL to next page
		/// </summary>
		public static string HtmlNextStyle = @"<li><a class='next-page' href='{0}' title='next page'>&gt;</a></li>";

		/// <summary>
		/// HTML template for first page.
		/// {0} -> URL to first page
		/// </summary>
		public static string HtmlFirstStyle = @"<li><a class='first-page' href='{0}' title='first page'>&lt;&lt;</a></li>";

		/// <summary>
		/// HTML template for last page.
		/// {0} -> URL to last page
		/// </summary>
		public static string HtmlLastStyle = @"<li><a class='last-page' href='{0}' title='last page'>&gt;&gt;</a></li>";

		private static Dictionary<string, string> styles;
		private static Dictionary<string, string> HtmlStyles
		{
			get
			{
				return styles ?? (styles = new Dictionary<string, string>
				{
					{ "paginationStyle", HtmlPaginationStyle },
					{ "pageStyle", HtmlPageStyle },
					{ "currentPageStyle", HtmlCurrentPageStyle },
					{ "morePageStyle", HtmlMorePageStyle },
					{ "prevPageStyle", HtmlPrevStyle },
					{ "nextPageStyle", HtmlNextStyle },
					{ "firstPageStyle", HtmlFirstStyle },
					{ "lastPageStyle", HtmlLastStyle }
				});
			}
		}

		/// <summary>
		/// Set HTML pagination style
		/// </summary>
		/// <param name="controller">bind to controller</param>
		/// <param name="paginationStyle">HTML template pagination container (dibutuhkan {0} sampai {4})</param>
		/// <param name="pageStyle">HTML template for each page (needed: {0} for link target, {1} for page number)</param>
		/// <param name="currentPageStyle">HTML template current active page (needed {0} for active page number)</param>
		/// <param name="morePageStyle">HTML template when pages over than maximum page per group</param>
		/// <param name="prevPageStyle">HTML template for previous page (needed {0} for link target)</param>
		/// <param name="nextPageStyle">HTML template for next page (needed {0} for link target)</param>
		/// <param name="firstPageStyle">HTML template for first page (needed {0} for link target))</param>
		/// <param name="lastPageStyle">HTML template for last page (needed {0} for link target)</param>
		public static void SetPaginationStyle(this Controller controller,
			string paginationStyle = null, string pageStyle = null, string currentPageStyle = null,
			string morePageStyle = null, string prevPageStyle = null, string nextPageStyle = null,
			string firstPageStyle = null, string lastPageStyle = null)
		{
			controller.TempData[PREFIX + "_styles"] = new Dictionary<string, string>
			{
				{ "paginationStyle", paginationStyle ?? HtmlPaginationStyle },
				{ "pageStyle", pageStyle ?? HtmlPageStyle },
				{ "currentPageStyle", currentPageStyle ?? HtmlCurrentPageStyle },
				{ "morePageStyle", morePageStyle ?? HtmlMorePageStyle },
				{ "prevPageStyle", prevPageStyle ?? HtmlPrevStyle },
				{ "nextPageStyle", nextPageStyle ?? HtmlNextStyle },
				{ "firstPageStyle", firstPageStyle ?? HtmlFirstStyle },
				{ "lastPageStyle", lastPageStyle ?? HtmlLastStyle },
			};
		}

		/// <summary>
		/// Generate HTML pagination compatible with Twitter Bootstrap
		/// </summary>
		/// <param name="offsetItem">generate offset query (start from 0), usefull for querying in database</param>
		/// <param name="totalItem">total data</param>
		/// <param name="itemPerPage">item per page</param>
		/// <param name="currentPage">current page</param>
		/// <param name="maxPagePerGroup">max page to be shown (will be replaced by ... when over)</param>
		/// <param name="baseUrl">additional base url</param>
		/// <param name="htmlStyles">HTML paging styles</param>
		/// <returns></returns>
		public static string Paginate(out int offsetItem, int totalItem, int itemPerPage = 20, int currentPage = 1, 
									  int maxPagePerGroup = 10, string baseUrl = null, 
									  Dictionary<string, string> htmlStyles = null)
		{
			string htmlOutput = "";
			//default offsetItem untuk asumsi halaman 1
			offsetItem = (currentPage - 1) * itemPerPage;
			if (totalItem < 1)
				return string.Empty;
			if (htmlStyles == null)
				htmlStyles = HtmlStyles;
			foreach (var htmlStyle in HtmlStyles)
				if (!htmlStyles.ContainsKey(htmlStyle.Key) || string.IsNullOrEmpty(htmlStyles[htmlStyle.Key]))
					htmlStyles[htmlStyle.Key] = htmlStyle.Value;
			//paksa halaman ke halaman 1 jika diisi dengan angka minus
			if (currentPage < 1)
				currentPage = 1;
			baseUrl = baseUrl ?? "";
			//tambahkan tanda tanya diakhir untuk memisahkan dengan query
			if (!baseUrl.Contains("?"))
				baseUrl = System.Text.RegularExpressions.Regex.IsMatch(baseUrl, @"([\w\[\]]+)=([^&]*)") ? "?" + baseUrl : baseUrl + "?";
			string lastChar = baseUrl.Substring(baseUrl.Length - 1);
			if (lastChar != "&" && lastChar != "?")
				baseUrl += "&";
			//hitung jumlah halaman
// ReSharper disable RedundantCast
			int totalPage = System.Math.Ceiling((float)totalItem / (float)itemPerPage).To<int>();
// ReSharper restore RedundantCast
			//paksa ke halaman terakhir jika yang diminta melebihi
			if (currentPage > totalPage)
				currentPage = totalPage;
			//tidak perlu generate paginasi jika halamannya cuman 1
			if (totalPage <= 1)
				return string.Empty;
			//menentukan start awal halaman
// ReSharper disable RedundantCast
			int temp = System.Math.Floor((float)currentPage / (float)maxPagePerGroup).To<int>();
// ReSharper restore RedundantCast
			if (currentPage % maxPagePerGroup == 0)
				temp -= 1;
			int startPageGroup = temp * maxPagePerGroup + 1;
			//link ke halaman pertama
			string htmlFirstPage = currentPage == 1 ? "" : string.Format(htmlStyles["firstPageStyle"], baseUrl + "page=1");
			//link ke halaman terakhir
			string htmlLastPage = currentPage == totalPage ? "" : string.Format(htmlStyles["lastPageStyle"], baseUrl + "page=" + totalPage);
			if (startPageGroup > maxPagePerGroup)
				htmlOutput += htmlStyles["morePageStyle"];
			int totalPageShown = 1;
			for (int page = startPageGroup; page <= totalPage; page++)
			{
				//non aktifkan halaman sekarang
				if (page == currentPage)
					htmlOutput += string.Format(htmlStyles["currentPageStyle"], page);
				//untuk halaman standar
				else
					htmlOutput += string.Format(htmlStyles["pageStyle"], baseUrl + "page=" + page, page);
				//batasi halaman sesuai jumlah halaman per group
				if (totalPageShown == maxPagePerGroup)
				{
					htmlOutput += htmlStyles["morePageStyle"];
					break;
				}
				totalPageShown++;
			}
			//link previous page tidak aktif jika sekarang adalah halaman 1
			string htmlPrevPage = currentPage == 1 ?
				"" : string.Format(htmlStyles["prevPageStyle"], baseUrl + "page=" + (currentPage - 1));
			//link next page tidak aktif jika sekarang adalah halaman terakhir
			string htmlNextPage = currentPage == totalPage ?
				"" : string.Format(htmlStyles["nextPageStyle"], baseUrl + "page=" + (currentPage + 1));
			//update offsetItem sesuai dengan currentPage
			offsetItem = (currentPage - 1) * itemPerPage;
			//hasil akhir paginasi
			return string.Format(htmlStyles["paginationStyle"], htmlFirstPage, htmlPrevPage, htmlOutput, htmlNextPage, htmlLastPage);
		}

		/// <summary>
		/// Generate HTML pagination compatible with Twitter Bootstrap
		/// </summary>
		/// <param name="controller">bind to controller</param>
		/// <param name="offsetItem">generate offset query (start from 0), usefull for querying in database</param>
		/// <param name="totalItem">total data</param>
		/// <param name="itemPerPage">item per page</param>
		/// <param name="currentPage">current page (default will be searched in controller routing, query, etc)</param>
		/// <param name="maxPagePerGroup">max page to be shown (will be replaced by ... when over)</param>
		/// <param name="baseUrl">additional base url (default will be built automatically from controller)</param>
		/// <returns></returns>
		public static void Paginate(this Controller controller, 
			out int offsetItem, int totalItem, int itemPerPage = 20, 
			int? currentPage = null, int maxPagePerGroup = 10, string baseUrl = null)
		{
			if (!currentPage.HasValue)
			{
				int? page = 0;
				//try searching from routed data
				try
				{
					page = controller.ControllerContext.RouteData.Values["page"].To<int?>();
				}
				catch (System.NullReferenceException) { }
				//try searching from controller parameter
				if (!page.HasValue || page < 1)
				{
					try
					{
						page = controller.ValueProvider.GetValue("page").AttemptedValue.To<int?>();
					}
					catch (System.NullReferenceException) { }
				}
				//try searching from query string
				if (!page.HasValue || page < 1)
				{
					try
					{
						page = controller.Request.QueryString["page"].To<int?>();
					}
					catch (System.NullReferenceException) { }
					catch (System.NotImplementedException) { }
				}
				currentPage = page.HasValue && page > 0 ? page : 1;
			}
			//generate baseUrl based on query string
			if (baseUrl == null)
			{
				//clone query string
				string queryString = controller.Request.QueryString.ToString();
				if (string.IsNullOrEmpty(queryString))
					baseUrl = "";
				else
				{
					var query = System.Web.HttpUtility.ParseQueryString(queryString);
					//remove page if any
					if (!string.IsNullOrEmpty(query["page"]))
						query.Remove("page");
					baseUrl = query.ToString();
				}
			}
			controller.TempData["pagination"] = Paginate(out offsetItem, totalItem, itemPerPage, currentPage.Value,
														 maxPagePerGroup, baseUrl, 
														 controller.TempData[PREFIX + "_styles"] as Dictionary<string, string>);
		}

		/// <summary>
		/// Show HTML pagination that already set using <see cref="Paginate(Controller,out int,int,int,int?,int,string)"/>
		/// </summary>
		/// <param name="helper"></param>
		/// <returns>Twitter bootstrap compatible HTML pagination</returns>
		public static MvcHtmlString Pagination(this HtmlHelper helper)
		{
			string pagination;
			try
			{
				pagination = (string)helper.ViewContext.TempData[PREFIX + "pagination"];
			}
			catch (KeyNotFoundException)
			{
				pagination = "";
			}
			return new MvcHtmlString(pagination);
		}
	}
}
