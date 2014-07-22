using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OpenLibrary.Extension
{
	/// <summary>
	/// XML helper for serialization or deserialization
	/// </summary>
	public static class XmlExtension
	{
		/// <summary>
		/// Convert strong type object to XML string
		/// </summary>
		/// <typeparam name="T">data type of object</typeparam>
		/// <param name="data"></param>
		/// <param name="settings">custom XML serializer setting</param>
		/// <returns>XML string</returns>
		public static string ToXml<T>(this T data, XmlWriterSettings settings = null) where T : class
		{
			var serializer = new XmlSerializer(typeof(T));
			string output;
			using (var writer = new StringWriter())
			using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				var ns = new XmlSerializerNamespaces();
				ns.Add("", "");
				serializer.Serialize(xmlWriter, data, ns);
				output = writer.ToString();
			}
			return output;
		}

		/// <summary>
		/// Parse XML string to strong type object
		/// </summary>
		/// <typeparam name="T">data type of requested object</typeparam>
		/// <param name="data">XML string</param>
		/// <returns>strong type object</returns>
		public static T FromXml<T>(this string data) where T : class
		{
			if (string.IsNullOrEmpty(data))
				return default(T);
			data = data.Trim();
			var type = typeof(T);
			var rootAttribute = type.GetCustomAttributes(typeof(XmlRootAttribute), false);
			var deserializer = rootAttribute.Length > 0
				                   ? new XmlSerializer(type)
				                   : new XmlSerializer(type, new XmlRootAttribute(type.Name) { IsNullable = true });
			T output;
			using (var reader = new StringReader(data))
			{
				output = (T)deserializer.Deserialize(reader);
			}
			return output;
		}
	}
}
