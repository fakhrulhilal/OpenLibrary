using System;
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
		/// <summary>
		/// HTML template for pagination container
		/// </summary>
		private static string htmlPaginationStyle = @"<div class='pagination pagination-right'><ul>{0} {1} {2} {3} {4}</ul></div>";

		/// <summary>
		/// HTML template for each page 
		/// </summary>
		private static string htmlPageStyle = @"<li><a href='{0}'>{1}</a></li>";

		/// <summary>
		/// HTML template for current active page
		/// </summary>
		private static string htmlCurrentPageStyle = @"<li class='active'><span>{0}</span></li>";

		/// <summary>
		/// HTML template when page over than maximum page per group
		/// </summary>
		private static string htmlMorePageStyle = @"<li class='disabled'><span>...</span></li>";

		/// <summary>
		/// HTML template for previous page
		/// </summary>
		private static string htmlPrevStyle = @"<li><a class='prev-page' href='{0}' title='previous page'>&lt;</a></li>";

		/// <summary>
		/// HTML template for next page
		/// </summary>
		private static string htmlNextStyle = @"<li><a class='next-page' href='{0}' title='next page'>&gt;</a></li>";

		/// <summary>
		/// HTML template for first page
		/// </summary>
		private static string htmlFirstStyle = @"<li><a class='first-page' href='{0}' title='first page'>&lt;&lt;</a></li>";

		/// <summary>
		/// HTML template for last page
		/// </summary>
		private static string htmlLastStyle = @"<li><a class='last-page' href='{0}' title='last page'>&gt;&gt;</a></li>";

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
			htmlPaginationStyle = paginationStyle ?? htmlPaginationStyle;
			htmlPageStyle = pageStyle ?? htmlPageStyle;
			htmlCurrentPageStyle = currentPageStyle ?? htmlCurrentPageStyle;
			htmlMorePageStyle = morePageStyle ?? htmlMorePageStyle;
			htmlPrevStyle = prevPageStyle ?? htmlPrevStyle;
			htmlNextStyle = nextPageStyle ?? htmlNextStyle;
			htmlFirstStyle = firstPageStyle ?? htmlFirstStyle;
			htmlLastStyle = lastPageStyle ?? htmlLastStyle;
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
		/// <returns></returns>
		public static string Paginate(out int offsetItem, int totalItem, int itemPerPage = 20, int currentPage = 1, int maxPagePerGroup = 10, string baseUrl = null)
		{
			string htmlOutput = "";
			//default offsetItem untuk asumsi halaman 1
			offsetItem = (currentPage - 1) * itemPerPage;
			if (totalItem < 1)
				return string.Empty;
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
			int totalPage = Math.Ceiling((float)totalItem / (float)itemPerPage).To<int>();
			//paksa ke halaman terakhir jika yang diminta melebihi
			if (currentPage > totalPage)
				currentPage = totalPage;
			//tidak perlu generate paginasi jika halamannya cuman 1
			if (totalPage <= 1)
				return string.Empty;
			//menentukan start awal halaman
			int temp = Math.Floor((float)currentPage / (float)maxPagePerGroup).To<int>();
			if (currentPage % maxPagePerGroup == 0)
				temp -= 1;
			int startPageGroup = temp * maxPagePerGroup + 1;
			//link ke halaman pertama
			string htmlFirstPage = currentPage == 1 ? "" : string.Format(htmlFirstStyle, baseUrl + "page=1");
			//link ke halaman terakhir
			string htmlLastPage = currentPage == totalPage ? "" : string.Format(htmlLastStyle, baseUrl + "page=" + totalPage);
			if (startPageGroup > maxPagePerGroup)
				htmlOutput += htmlMorePageStyle;
			int totalPageShown = 1;
			for (int page = startPageGroup; page <= totalPage; page++)
			{
				//non aktifkan halaman sekarang
				if (page == currentPage)
					htmlOutput += string.Format(htmlCurrentPageStyle, page);
				//untuk halaman standar
				else
					htmlOutput += string.Format(htmlPageStyle, baseUrl + "page=" + page, page);
				//batasi halaman sesuai jumlah halaman per group
				if (totalPageShown == maxPagePerGroup)
				{
					htmlOutput += htmlMorePageStyle;
					break;
				}
				totalPageShown++;
			}
			//link previous page tidak aktif jika sekarang adalah halaman 1
			string htmlPrevPage = currentPage == 1 ?
				"" : string.Format(htmlPrevStyle, baseUrl + "page=" + (currentPage - 1));
			//link next page tidak aktif jika sekarang adalah halaman terakhir
			string htmlNextPage = currentPage == totalPage ?
				"" : string.Format(htmlNextStyle, baseUrl + "page=" + (currentPage + 1));
			//update offsetItem sesuai dengan currentPage
			offsetItem = (currentPage - 1) * itemPerPage;
			//hasil akhir paginasi
			return string.Format(htmlPaginationStyle, htmlFirstPage, htmlPrevPage, htmlOutput, htmlNextPage, htmlLastPage);
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
		public static void Paginate(this Controller controller, out int offsetItem, int totalItem, int itemPerPage = 20, int? currentPage = null, int maxPagePerGroup = 10, string baseUrl = null)
		{
			if (!currentPage.HasValue)
			{
				int? page = 0;
				//try searching from routed data
				try
				{
					page = controller.ControllerContext.RouteData.Values["page"].To<int?>();
				}
				catch (NullReferenceException) { }
				//try searching from controller parameter
				if (!page.HasValue || page < 1)
				{
					try
					{
						page = controller.ValueProvider.GetValue("page").AttemptedValue.To<int?>();
					}
					catch (NullReferenceException) { }
				}
				//try searching from query string
				if (!page.HasValue || page < 1)
				{
					try
					{
						page = controller.Request.QueryString["page"].To<int?>();
					}
					catch (NullReferenceException) { }
					catch (NotImplementedException) { }
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
			controller.TempData["pagination"] = Paginate(out offsetItem, totalItem, itemPerPage, currentPage.Value, maxPagePerGroup, baseUrl);
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
				pagination = (string)helper.ViewContext.TempData["pagination"];
			}
			catch (KeyNotFoundException)
			{
				pagination = "";
			}
			return new MvcHtmlString(pagination);
		}
	}
}
