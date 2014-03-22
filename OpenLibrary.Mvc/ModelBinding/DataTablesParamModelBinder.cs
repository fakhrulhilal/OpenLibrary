using System.Collections.Generic;
using System.ComponentModel;
using OpenLibrary.Extension;

namespace OpenLibrary.Mvc.ModelBinding
{
	// ReSharper disable InconsistentNaming
	/// <summary>
	/// Process ModelBinding for DataTables (DataTables.net) <see cref="DataTablesModelBinder"/>
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Class |
						   System.AttributeTargets.Parameter |
						   System.AttributeTargets.Interface |
						   System.AttributeTargets.Interface, 
						   AllowMultiple = false, Inherited = false)]
	public sealed class DataTablesParamAttribute : System.Web.Mvc.CustomModelBinderAttribute
	{
		/// <summary>
		/// Determine wether object should be processed for this ModelBinder or not
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Match(object obj)
		{
			return obj is DataTablesParam;
		}

		/// <summary>
		/// Get ModelBinder for this object
		/// </summary>
		/// <returns></returns>
		public override System.Web.Mvc.IModelBinder GetBinder()
		{
			return new DataTablesModelBinder();
		}
	}

	/// <summary>
	/// Parse parameter from request to <see cref="DataTablesParam"/> 
	/// </summary>
	public class DataTablesModelBinder : System.Web.Mvc.IModelBinder
	{
		/// <summary>
		/// Main process of model binding
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="bindingContext"></param>
		/// <returns></returns>
		public object BindModel(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{
			var model = new DataTablesParam();
			if (bindingContext.ModelType == typeof(DataTablesParam))
			{
				var provider = bindingContext.ValueProvider;
				//var controller = controllerContext.HttpContext;
				if (provider.GetValue("iDisplayStart") != null)
					model.iDisplayStart = provider.GetValue("iDisplayStart").AttemptedValue.To<int>();
				if (provider.GetValue("iDisplayLength") != null)
					model.iDisplayLength = provider.GetValue("iDisplayLength").AttemptedValue.To<int>();
				if (provider.GetValue("sSearch") != null)
					model.sSearch = provider.GetValue("sSearch").AttemptedValue.To<string>();
				if (provider.GetValue("iColumns") != null)
					model.iColumns = provider.GetValue("iColumns").AttemptedValue.To<int>();
				string index = "";
				if (provider.GetValue("iSortingCols") != null)
				{
					model.iSortingCols = provider.GetValue("iSortingCols").AttemptedValue.To<int>();
					model.iSortCols = model.iSortCols ?? new Dictionary<int, SortOrder>();
					for (int i = 0; i < model.iSortingCols; i++)
					{
						index = i.ToString();
						if (provider.GetValue("bSortable_" + index) != null &&
							provider.GetValue("bSortable_" + index).AttemptedValue.To<bool>() &&
							provider.GetValue("iSortCol_" + index) != null &&
							provider.GetValue("iSortDir_" + index) != null)
						{
							int key = provider.GetValue("iSortCol_" + index).AttemptedValue.To<int>();
							SortOrder sortOrder = provider.GetValue("iSortDir_" + index).AttemptedValue.To<string>().Trim().ToLower() == "desc" ? SortOrder.Desc : SortOrder.Asc;
							model.iSortCols[key] = sortOrder;
						}
					}
				}
				model.sSearchs = model.sSearchs ?? new Dictionary<int, string>();
				for (int i = 0; i < model.iColumns; i++)
				{
					index = i.ToString();
					if (provider.GetValue("bSearchable_" + index) != null &&
						provider.GetValue("bSearchable_" + index).AttemptedValue.To<bool>() &&
						provider.GetValue("sSearch_" + index) != null)
					{
						model.sSearchs[i] = provider.GetValue("sSearch_" + index).AttemptedValue.To<string>();
					}
				}
			}
			return model;
		}
	}

	#region Type Helper

	/// <summary>
	/// DataTables metadata
	/// </summary>
	public class DataTablesParam
	{
		/// <summary>
		/// Number of records that the table can display in the current draw.
		/// </summary>
		public int? iDisplayLength { get; set; }

		/// <summary>
		/// Display start point in the current data set.
		/// </summary>
		public int? iDisplayStart { get; set; }

		/// <summary>
		/// Global search field
		/// </summary>
		public string sSearch { get; set; }

		/// <summary>
		/// Individual column filter.
		/// Index is return of sSearch_(int).
		/// </summary>
		public Dictionary<int, string> sSearchs { get; set; }

		/// <summary>
		/// Number of columns being displayed (useful for getting individual column search info)
		/// </summary>
		public int iColumns { get; set; }

		/// <summary>
		/// True if the global filter should be treated as a regular expression for advanced filtering, false if not.
		/// </summary>
		public bool? bRegex { get; set; }

		/// <summary>
		/// Number of columns to sort on
		/// </summary>
		public int iSortingCols { get; set; }

		/// <summary>
		/// Column being sorted on (you will need to decode this number for your database).
		/// Index is return of iSortCol_(int).
		/// </summary>
		public Dictionary<int, SortOrder> iSortCols { get; set; }

		/// <summary>
		/// Information for DataTables to use for rendering.
		/// </summary>
		public string sEcho { get; set; }

		/// <summary>
		/// Indicator for if a column is flagged as searchable or not on the client-side
		/// </summary>
		public Dictionary<int, bool> bSearchable { get; set; }

		/// <summary>
		/// Dump sorting columns applicable to SQL query
		/// </summary>
		/// <param name="columns">list of columns</param>
		/// <returns>SQL order by ('order by' keyword not included)</returns>
		public string DumpSort(List<string> columns)
		{
			var sorts = new List<string>();
			foreach (KeyValuePair<int, SortOrder> item in iSortCols)
			{
				if (columns[item.Key] != null)
					sorts.Add(string.Format("{0} {1}", columns[item.Key], item.Value.ToString().ToLower()));
			}
			return sorts.Count > 0 ? string.Join(", ", sorts) : null;
		}

		/// <summary>
		/// Dump sorting columns applicable to SQL query.
		/// Column name will be __COLUMN[x]__ where [x] is index from iSortCols.
		/// </summary>
		/// <returns>SQL order by ('order by' keyword not included)</returns>
		public string DumpSort()
		{
			var sorts = new List<string>();
			foreach (KeyValuePair<int, SortOrder> item in iSortCols)
				sorts.Add(string.Format("__COLUMN{0}__ {1}", item.Key, item.Value.ToString().ToLower()));
			return sorts.Count > 0 ? string.Join(", ", sorts) : null;
		}
	}

	/// <summary>
	/// DataTables JSON response format
	/// </summary>
	public class DataTablesResult<T>
		where T : class
	{
		/// <summary>
		/// Total records, before filtering (i.e. the total number of records in the database)
		/// </summary>
		public int iTotalRecords { get; set; }

		/// <summary>
		/// Total records, after filtering (i.e. the total number of records after filtering has been applied - 
		/// not just the number of records being returned in this result set)
		/// </summary>
		public int iTotalDisplayRecords { get; set; }

		/// <summary>
		/// An unaltered copy of sEcho sent from the client side. 
		/// This parameter will change with each draw (it is basically a draw count) - 
		/// so it is important that this is implemented. Note that it strongly recommended for security reasons 
		/// that you 'cast' this parameter to an integer in order to prevent Cross Site Scripting (XSS) attacks.
		/// </summary>
		public string sEcho { get; set; }

		/// <summary>
		/// Deprecated Optional - this is a string of column names, comma separated (used in combination with sName) 
		/// which will allow DataTables to reorder data on the client-side if required for display. 
		/// Note that the number of column names returned must exactly match the number of columns in the table. 
		/// For a more flexible JSON format, please consider using mData.
		/// </summary>
		[System.Obsolete("Note that this parameter is deprecated and will be removed in v1.10. Please now use mData")]
		public string sColumns { get; set; }

		/// <summary>
		/// The data in a 2D array. Note that you can change the name of this parameter with sAjaxDataProp
		/// </summary>
		public IEnumerable<T> aaData { get; set; }
	}

	/// <summary>
	/// Column sort order
	/// </summary>
	public enum SortOrder
	{
		/// <summary>
		/// Ascending
		/// </summary>
		[Description("Ascending")]
		Asc,

		/// <summary>
		/// Descending
		/// </summary>
		[Description("Descending")]
		Desc
	}

	#endregion
}
