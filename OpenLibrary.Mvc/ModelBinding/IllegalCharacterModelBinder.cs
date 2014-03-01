using System;
using System.Web.Mvc;
using OpenLibrary.Mvc.Attribute;
using OpenLibrary.Extension;

namespace OpenLibrary.Mvc.ModelBinding
{
	/// <summary>
	/// Filter string for blacklisted characters.
	/// See also <seealso cref="AllowCharacterAttribute"/>
	/// </summary>
	public class IllegalCharacterModelBinder : IModelBinder
	{
		/// <summary>
		/// Define illegal/blacklisted characters
		/// </summary>
		public string IllegalCharacters { get; set; }

		private string errorMessage;

		/// <summary>
		/// Remove illegal character for all string input
		/// </summary>
		/// <param name="illegalCharacters"></param>
		public IllegalCharacterModelBinder(string illegalCharacters)
		{
			illegalCharacters = illegalCharacters.Replace("\\", "\\\\");
			IllegalCharacters = System.Text.RegularExpressions.Regex.Replace(illegalCharacters, "([\\?\\+\\*\\.\\|\\(\\)\\{\\}\\[\\]])", "\\$1");
		}

		/// <summary>
		/// Remove illegal character for all string input using default
		/// </summary>
		public IllegalCharacterModelBinder()
			: this("?[]{}()*^#|+~!")
		{ }

		/// <summary>
		/// Main process of model binding
		/// </summary>
		/// <param name="controllerContext"></param>
		/// <param name="bindingContext"></param>
		/// <returns></returns>
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			object result = null;
			errorMessage = "{0} contains illegal characters, i.e. one of {1}.";
			if (bindingContext.ModelType == typeof(String))
			{
				string modelName = bindingContext.ModelName;
				var valueResult = bindingContext.ValueProvider.GetValue(modelName);
				string modelValue = valueResult != null ? valueResult.AttemptedValue.Trim() : "";
				if (valueResult != null)
					bindingContext.ModelState.SetModelValue(modelName, new ValueProviderResult(valueResult.RawValue, modelValue, valueResult.Culture));
				else
					bindingContext.ModelState.SetModelValue(modelName, new ValueProviderResult("", modelValue, System.Threading.Thread.CurrentThread.CurrentCulture));
				if (bindingContext.ModelMetadata != null && bindingContext.ModelMetadata.ContainerType != null)
				{
					//cek apakah ada karakter khusus yg diperbolehkan
					string legalCharacters = "__DEFAULT__";
					var legalCharacterAttribute = bindingContext.ModelMetadata.ContainerType.GetProperty(modelName).GetCustomAttributes(typeof(AllowCharacterAttribute), false);
					if (legalCharacterAttribute.Length > 0)
					{
						legalCharacters = ((AllowCharacterAttribute)legalCharacterAttribute[0]).LegalCharacters;
						errorMessage = ((AllowCharacterAttribute)legalCharacterAttribute[0]).ErrorMessage;
					}
					//jika kosong, berarti boleh semuanya
					if (legalCharacterAttribute.Length > 0 && string.IsNullOrEmpty(legalCharacters))
						return (result = modelValue);
					//jika ada daftar karakter yg diperbolehkan, maka hapus dari daftar karakter ilegal
					if (!string.IsNullOrEmpty(legalCharacters) && legalCharacters != "__DEFAULT__")
					{
						char[] characters = legalCharacters.ToCharArray();
						foreach (char character in characters)
							IllegalCharacters = IllegalCharacters.Replace(character.ToString(), "");
					}
				}
				//validasi jika ada karakter yang tidak diperkenankan -> hanya jika illegal character ditentukan
				if (!string.IsNullOrEmpty(IllegalCharacters) && System.Text.RegularExpressions.Regex.IsMatch(modelValue, "[" + IllegalCharacters + "]"))
					bindingContext.ModelState.AddModelError(modelName, string.Format(errorMessage, string.IsNullOrEmpty(bindingContext.ModelMetadata.DisplayName) ? modelName : bindingContext.ModelMetadata.DisplayName));
				result = modelValue;
			}
			return result;
		}
	}
}
