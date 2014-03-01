using System.ComponentModel;

namespace OpenLibrary.Extension
{
	/// <summary>
	/// Extension for <see cref="System.Enum"/>
	/// </summary>
	public static class EnumExtension
	{
		/// <summary>
		/// Get description of current enum from System.ComponentModel.DescriptionAttribute or ToString() method
		/// </summary>
		/// <param name="value">bind to enum</param>
		/// <returns>string</returns>
		public static string Description(this System.Enum value)
		{
			if (value == null)
				return string.Empty;
			var enumType = value.GetType();
			var defaultValue = System.Enum.GetName(enumType, value);
			if (defaultValue == null)
			{
				return string.Empty;
			}
			var field = enumType.GetField(defaultValue);
			if (field == null)
			{
				return defaultValue;
			}
			var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute),
														false);
			return attributes.Length == 0
				? defaultValue
				: ((DescriptionAttribute)attributes[0]).Description;
		}
	}
}
