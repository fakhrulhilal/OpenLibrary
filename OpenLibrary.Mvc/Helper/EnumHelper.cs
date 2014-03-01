using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using OpenLibrary.Extension;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Helper for <see cref="System.Enum"/>
	/// </summary>
	public static class EnumHelper
	{

		/// <summary>
		/// Convert enum to selection list item for use in dropdownlist widget.
		/// </summary>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public static List<SelectListItem> ToSelectionList(this System.Type enumType)
		{
			var selectionList = new List<SelectListItem>();
			if (!enumType.IsEnum)
				return selectionList;
			var fields = enumType.GetFields();
			selectionList.AddRange(from field in fields
								   where !field.Name.Equals("value__")
								   let attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
								   select new SelectListItem
								   {
									   Text = (attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : field.Name),
									   Value = field.GetRawConstantValue().ToString()
								   });
			return selectionList;
		}

		/// <summary>
		/// Get all values of <see cref="System.Enum"/> and convert to integer type
		/// </summary>
		/// <typeparam name="T">tipe data output untuk value</typeparam>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public static List<T> Values<T>(this System.Type enumType) where T : struct
		{
			if (!typeof(T).IsInteger())
				return new List<T>();
			return enumType.ToSelectionList().Select(m => m.Value.To<T>()).ToList();
		}

		/// <summary>
		/// Get all text from <see cref="System.Enum"/>
		/// </summary>
		/// <param name="enumType"></param>
		/// <returns></returns>
		public static List<string> Captions(this System.Type enumType)
		{
			return enumType.ToSelectionList().Select(m => m.Text).ToList();
		}
	}
}
