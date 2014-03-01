using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace OpenLibrary.Mvc.ModelBinding
{
	/// <summary>
	/// Handle binding floating numeric that compatible with comma as thousand separator and dot as decimal separator or vice versa
	/// </summary>
	public class FloatingNumericModelBinder : IModelBinder
	{
		/// <summary>
		/// Main process of model binding
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="bindingContext"></param>
		/// <returns></returns>
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			object result = null;
			//handle pemisah angka desimal dan ribuan untuk bilangan 
			if (bindingContext.ModelType == typeof(double) ||
				bindingContext.ModelType == typeof(double?) ||
				bindingContext.ModelType == typeof(decimal) ||
				bindingContext.ModelType == typeof(decimal?))
			{
				string modelName = bindingContext.ModelName;
				var valueResult = bindingContext.ValueProvider.GetValue(modelName);
				string attemptedValue = valueResult.AttemptedValue;
				bindingContext.ModelState.SetModelValue(modelName, valueResult);
				// Depending on cultureinfo the NumberDecimalSeparator can be "," or "."
				// Both "." and "," should be accepted, but aren't.
				string wantedSeperator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
				string alternateSeperator = (wantedSeperator == "," ? "." : ",");

				if (attemptedValue.IndexOf(wantedSeperator) == -1 &&
					attemptedValue.IndexOf(alternateSeperator) != -1)
					attemptedValue = attemptedValue.Replace(alternateSeperator, wantedSeperator);
				try
				{
					if (bindingContext.ModelType == typeof(Double) || bindingContext.ModelType == typeof(double?))
						result = double.Parse(attemptedValue, NumberStyles.Any);
					if (bindingContext.ModelType == typeof(Decimal) || bindingContext.ModelType == typeof(decimal?))
						result = decimal.Parse(attemptedValue, NumberStyles.Any);
				}
				catch (FormatException)
				{
					var requiredAttribute = new RequiredAttribute();
					//anggap bukan error jika model adalah nullable
					if (string.IsNullOrEmpty(attemptedValue) &&
						bindingContext.ModelType != typeof(decimal?) &&
						bindingContext.ModelType != typeof(double?))
						bindingContext.ModelState.AddModelError(modelName, requiredAttribute.FormatErrorMessage(bindingContext.ModelMetadata.DisplayName));
				}
			}
			return result;
		}
	}
}
