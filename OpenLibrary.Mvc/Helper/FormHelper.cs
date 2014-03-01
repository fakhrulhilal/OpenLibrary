using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using OpenLibrary.Extension;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Helper for <see cref="System.Web.Mvc.FormCollection"/>
	/// </summary>
	public static class FormHelper
	{
		/// <summary>
		/// Helper method for converting array of string to numerical data type
		/// </summary>
		/// <typeparam name="T">Numerical data type, ex: int, short, float, etc</typeparam>
		/// <param name="numbers">array of string to be convert to numerical data type</param>
		/// <returns>List&lt;T&gt;</returns>
		private static List<T> Converter<T>(string[] numbers)
		{
			return numbers == null
				? new List<T>()
				: numbers.Select(id =>
				{
					//id = Regex.Replace(id, @"[^\d\.\-,]", "");
					if (string.IsNullOrEmpty(id))
						id = "0";
					return id.To<T>(dateFormat: "yyyy-MM-dd");
				}).ToList();
		}

		/// <summary>
		/// Get collection of value and convert them wanted data type
		/// </summary>
		/// <typeparam name="T">target data type</typeparam>
		/// <param name="form">Form to be search for id key</param>
		/// <param name="field">id key field name</param>
		/// <returns>List of T</returns>
		public static List<T> GetChecked<T>(this FormCollection form, string field)
		{
			return Converter<T>(form.GetValues(field));
		}

		/// <summary>
		/// Get collection of value and convert them wanted data type
		/// </summary>
		/// <typeparam name="T">target data type</typeparam>
		/// <param name="form">Form to be search for id key</param>
		/// <param name="field">id key field name</param>
		/// <returns>List of T</returns>
		public static List<T> GetChecked<T>(this NameValueCollection form, string field)
		{
			return Converter<T>(form.GetValues(field));
		}
	}
}
