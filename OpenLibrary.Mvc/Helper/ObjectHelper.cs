using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OpenLibrary.Extension;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Helper for general object
	/// </summary>
	public static class ObjectHelper
	{

		/// <summary>
		/// Serialize object to application/x-www-form-urlencoded format.
		/// Currently doesn't support IEnumerable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="fields">property to be serialize (default: generated)</param>
		/// <param name="prefix">prefix to be added for every key</param>
		/// <returns></returns>
		public static string SerializeToWwwForm<T>(this T data, PropertyInfo[] fields = null, string prefix = "")
		{
			if (fields == null)
				fields = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty);
			if (data is IEnumerable)
				return SerializeEnumerableToWwwForm((IEnumerable)data, prefix);
			var pairs = new List<string>();
			prefix = string.IsNullOrEmpty(prefix) ? "" : prefix + ".";
			foreach (var field in fields)
			{
				object value = field.GetValue(data, null);
				if (value == null)
					pairs.Add(string.Format("{0}{1}=", prefix, System.Web.HttpUtility.UrlEncode(field.Name)));
				else if (!(value is string) && value is IEnumerable)
					pairs.Add(SerializeEnumerableToWwwForm((IEnumerable)value, field.Name));
				else
					pairs.Add(string.Format("{0}{1}={2}",
						prefix,
						System.Web.HttpUtility.UrlEncode(field.Name),
						System.Web.HttpUtility.UrlEncode(value.To<string>(dateFormat: "yyyy-MM-dd"))));
			}
			return string.Join("&", pairs);
		}

		private static string SerializeEnumerableToWwwForm(this IEnumerable collection, string prefix = "")
		{
			if (collection == null)
				return string.Empty;
			System.Type type = collection.GetType();
			System.Type underlyingType = type.BaseType == typeof(System.Array) ? type.GetElementType() : type.GetGenericArguments()[0];
			bool isPrimitive = underlyingType.IsPrimitive();
			var fields = underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty);
			int counter = 0;
			var pairs = new List<string>();
			//var dottedPrefix = string.IsNullOrEmpty(prefix) ? "" : prefix + ".";
			foreach (var item in collection)
				pairs.Add(isPrimitive
							  ? string.Format("{0}[{1}]={2}", prefix, counter++, item.To<string>(dateFormat: "yyyy-MM-dd"))
							  : item.SerializeToWwwForm(fields, string.Format("{0}[{1}]", prefix, counter++)));
			return string.Join("&", pairs);
		}
	}
}
