using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace OpenLibrary.Mvc.Formatter
{
	/// <summary>
	/// Time format since unix timestamp epoch.
	/// </summary>
	public class UnixDateTimeConverter : DateTimeConverterBase
	{
		/// <summary>
		/// Import from unix epoch time
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.Integer)
				throw new Exception(String.Format("Unexpected token parsing date. Expected Integer, got {0}.", reader.TokenType));

			var ticks = (long)reader.Value;

			var date = new DateTime(1970, 1, 1);
			date = date.AddSeconds(ticks);

			return date;
		}

		/// <summary>
		/// Export to unix epoch time
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			long ticks;
			if (value is DateTime)
			{
				var epoc = new DateTime(1970, 1, 1);
				var delta = ((DateTime)value) - epoc;
				if (delta.TotalSeconds < 0)
				{
					throw new ArgumentOutOfRangeException("value", "Unix epoc starts January 1st, 1970");
				}
				ticks = (long)delta.TotalSeconds;
			}
			else
			{
				throw new Exception("Expected date object value.");
			}
			writer.WriteValue(ticks);
		}
	}
}
