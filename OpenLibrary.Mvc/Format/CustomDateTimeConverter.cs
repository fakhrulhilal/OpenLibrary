using Newtonsoft.Json;
using OpenLibrary.Extension;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace OpenLibrary.Mvc.Formatter
{
	/// <summary>
	/// Custom <see cref="System.DateTime"/> parser for JSON
	/// </summary>
	public class CustomDateTimeConverter : DateTimeConverterBase
	{
		/// <summary>
		/// <see cref="System.DateTime"/> parse format
		/// </summary>
		public string Format { get; set; }

		/// <summary>
		/// Custom date format
		/// </summary>
		/// <param name="dateTimeFormat">date time format</param>
		public CustomDateTimeConverter(string dateTimeFormat)
		{
			Format = dateTimeFormat;
		}

		/// <summary>
		/// Use yyyy-MM-dd HH:mm:ss as default format
		/// </summary>
		public CustomDateTimeConverter()
		{ }

		/// <summary>
		/// Export to JSON
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteValue("");
				return;
			}
			string format = Format;
			//cari format sesuai dengan tipe data sumber
			if (string.IsNullOrEmpty(format))
			{
				format = "yyyy-MM-dd HH:mm:ss";
				var dataTypeAttribute = value.GetType().GetCustomAttributes(typeof(DataTypeAttribute), true);
				if (dataTypeAttribute.Length > 0)
				{
					var type = ((DataTypeAttribute)dataTypeAttribute[0]);
					format = type.DataType == DataType.Date ? "yyyy-MM-dd" : type.DataType == DataType.Time ? "HH:mm:ss" : format;
				}
			}
			if (value is System.DateTime)
				writer.WriteValue(((System.DateTime)value).ToString(format));
// ReSharper disable ConditionIsAlwaysTrueOrFalse
			else if (value is System.DateTime?)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode
				writer.WriteValue(((System.DateTime?)value).GetValueOrDefault().ToString(format));
// ReSharper restore HeuristicUnreachableCode
			else
				writer.WriteValue(value.To<string>(dateFormat: format));
		}

		/// <summary>
		/// Import from JSON
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
// ReSharper disable ConditionIsAlwaysTrueOrFalse
				return (reader.Value is System.DateTime || reader.Value is System.DateTime?)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
					? reader.Value
					: objectType == typeof(System.DateTime) ? (object)System.DateTime.MinValue : null;
			var stringDateTime = (string)reader.Value;
			if (!string.IsNullOrEmpty(Format))
			{
				string regex = Format.Replace("dd", @"\d{1,2}");
				regex = System.Text.RegularExpressions.Regex.Replace(regex, @"(?<!\\)d", @"\d{1,2}")
							  .Replace("HH", @"\d{1,2}").Replace("H", @"\d{1,2}")
							  .Replace("mm", @"\d{1,2}").Replace("m", @"\d{1,2}")
							  .Replace("ss", @"\d{1,2}").Replace("s", @"\d{1,2}")
							  .Replace("yyyy", @"\d{4}").Replace("yy", @"\d{2}")
							  .Replace("MMMM", @"\w+").Replace("MMM", @"\w+").Replace("MM", @"\d{1,2}").Replace("M", @"\d{1,2}")
							  .Replace(".", @"\.")
							  .Replace("-", @"\-");
				regex = string.Format("(?<format>{0})", regex);
				var matches = System.Text.RegularExpressions.Regex.Match(stringDateTime, regex);
				if (matches.Success && matches.Length > 0)
					stringDateTime = matches.Groups["format"].Value;
			}
			return stringDateTime.To(objectType, dateFormat: Format);
		}
	}

	/// <summary>
	/// Custom <see cref="System.DateTime"/> (Date) parser for JSON.
	/// Using yyyy-MM-dd as format.
	/// </summary>
	public class CustomDateConverter : CustomDateTimeConverter
	{
		/// <summary>
		/// Using yyyy-MM-dd as <see cref="System.DateTime"/> (Date) parser for JSON
		/// </summary>
		public CustomDateConverter()
			: base("yyyy-MM-dd")
		{ }
	}

	/// <summary>
	/// Custom <see cref="System.DateTime"/> of Time parser for JSON.
	/// Using HH:mm:ss as format.
	/// </summary>
	public class CustomTimeConverter : CustomDateTimeConverter
	{
		/// <summary>
		/// Using HH:mm:ss as <see cref="System.DateTime"/> (Time) parser for JSON
		/// </summary>
		public CustomTimeConverter()
			: base("HH:mm:ss")
		{ }
	}
}
