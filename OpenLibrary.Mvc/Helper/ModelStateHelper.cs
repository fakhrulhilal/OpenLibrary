using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Helper for <see cref="System.Web.Mvc.ModelStateDictionary"/>
	/// </summary>
	public static class ModelStateHelper
	{
		/// <summary>
		/// Convert ModelState to collection of KeyValuePair for available error only
		/// </summary>
		/// <param name="modelState"></param>
		/// <returns></returns>
		public static List<KeyValuePair<string, string[]>> Errors(this ModelStateDictionary modelState)
		{
			if (!modelState.IsValid)
			{
				return modelState.ToDictionary(kvp => kvp.Key,
											   kvp => kvp.Value.Errors
														 .Select(e => e.ErrorMessage).ToArray())
								 .Where(m => m.Value.Any())
								 .ToList();
			}
			return null;
		}

		/// <summary>
		/// Convert ModelState to native javascript object (array of hash) for available error only
		/// </summary>
		/// <param name="modelState"></param>
		/// <returns></returns>
		public static Dictionary<string, string[]> ToAjaxDto(this ModelStateDictionary modelState)
		{
			if (!modelState.IsValid)
			{
				return modelState.ToDictionary(kvp => kvp.Key,
											   kvp => kvp.Value.Errors
														 .Select(e => e.ErrorMessage)
														 .ToArray())
								 .Where(m => m.Value.Any())
								 .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
			return null;
		}
	}
}
