using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace OpenLibrary.Extension
{
	/// <summary>
	/// 
	/// </summary>
	public class MappingConfiguration
	{
		/// <summary>
		/// Original property name
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		/// Mapped field to table
		/// </summary>
		public string ColumnName { get; set; }

		public override string ToString()
		{
			return string.Format("{0} => {1}", PropertyName, ColumnName);
		}
	}

	/// <summary>
	/// Extension for <see cref="object"/>
	/// </summary>
	public static class ObjectExtension
	{
		/// <summary>
		/// Digunakan untuk mendeteksi apakah suatu tipe data merupakan tipe data bilangan bulat atau tidak
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsInteger(this System.Type type)
		{
			return new[]
			{
				typeof(short), typeof(short?), typeof(ushort), typeof(ushort?),
				typeof(int), typeof(int?), typeof(uint), typeof(uint?),
				typeof(long), typeof(long?), typeof(ulong), typeof(ulong?)
			}.Contains(type);
		}

		/// <summary>
		/// Digunakan untuk mendeteksi apakah suatu tipe data bukan termasuk tipe data complex
		/// </summary>
		/// <param name="type">tipe data</param>
		/// <returns>boolean</returns>
		public static bool IsPrimitive(this System.Type type)
		{
			return new[]
			{
				typeof(decimal), typeof(decimal?),
				typeof(System.Single), typeof(System.Single?),
				typeof(short), typeof(short?), typeof(ushort), typeof(ushort?),
				typeof(int), typeof(int?), typeof(uint), typeof(uint?),
				typeof(long), typeof(long?), typeof(ulong), typeof(ulong?),
				typeof(string), typeof(char),
				typeof(double), typeof(double?),
				typeof(System.DateTime), typeof(System.DateTime?),
				typeof(System.TimeSpan), typeof(System.TimeSpan?),
				typeof(System.DateTimeOffset), typeof(System.DateTimeOffset?),
				typeof(bool), typeof(bool?)
			}.Contains(type);
		}

		/// <summary>
		/// Digunakan untuk mendeteksi apakah suatu tipe data bukan termasuk tipe data complex
		/// </summary>
		/// <param name="data">object yang akan dicek</param>
		/// <returns>boolean</returns>
		public static bool IsPrimitive(this object data)
		{
			return data.GetType().IsPrimitive();
		}

		/// <summary>
		/// Digunakan untuk mengecek apakah suatu tipe data bisa bernilai null atau tidak
		/// </summary>
		/// <param name="type"></param>
		/// <returns>boolean</returns>
		public static bool IsNullable(this System.Type type)
		{
			if (!type.IsValueType)
				return true;
			return System.Nullable.GetUnderlyingType(type) != null;
		}

		/// <summary>
		/// Check wether current object is null or not
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsNull(this object data)
		{
			if (data == null || data == System.DBNull.Value)
				return true;
			if (data is string &&
				(string.IsNullOrEmpty((string)data) ||
				((string)data).Trim().ToUpper() == "NULL"))
				return true;
			return false;
		}

		/// <summary>
		/// Safely convert to another data type
		/// </summary>
		/// <param name="type">target data type</param>
		/// <param name="sender"></param>
		/// <param name="isExcelDate">define wether to convert from old excel (&lt;= 2003) date (applied only for <see cref="System.DateTime"/>)</param>
		/// <param name="dateFormat">import date format (applied only for <see cref="System.DateTime"/>)</param>
		/// <param name="culture">default culture for convertion</param>
		/// <returns>dynamic</returns>
		public static dynamic To(this object sender, System.Type type, bool isExcelDate = false, string dateFormat = "", System.Globalization.CultureInfo culture = null)
		{
			System.Func<dynamic> defaultValue = () => type == typeof(string) ? default(string) : System.Activator.CreateInstance(type);
			if (!type.IsPrimitive() || sender == null)
				return defaultValue();
			//jika tipe data sumber & tujuan sudah sama, maka tidak perlu dilakukan konversi
			var sourceType = sender.GetType();
			if (type == sourceType ||
				(type.IsNullable() && System.Nullable.GetUnderlyingType(type) == sourceType))
				return sender;
			if (sourceType.IsNullable() && System.Nullable.GetUnderlyingType(sourceType) == type)
				return sender.IsNull() ? defaultValue() : sender;
			#region Konversi bilangan bulat
			if (type == typeof(int) || type == typeof(int?) ||
				type == typeof(short) || type == typeof(short?) ||
				type == typeof(long) || type == typeof(long?))
			{
				//hapus selain angka
				if (sender is string)
					sender = System.Text.RegularExpressions.Regex.Replace((string)sender, @"[^\-\d]", "");
				if (sender.IsNull())
				{
					if (type.IsNullable())
						return null;
					return defaultValue();
				}
				try
				{
					//khusus untuk nullable, convert dulu ke tipe data dasarnya dulu
					if (type.IsNullable())
					{
						sender = System.Convert.ChangeType(sender, System.Nullable.GetUnderlyingType(type));
						return (System.ComponentModel.TypeDescriptor.GetConverter(type)).ConvertFrom(sender);
					}
					return System.Convert.ChangeType(sender, type);
				}
				catch (System.InvalidCastException) { return defaultValue(); }
				catch (System.ArgumentNullException) { return defaultValue(); }
				catch (System.FormatException) { return defaultValue(); }
				catch (System.OverflowException) { return defaultValue(); }
			}
			#endregion
			#region Konversi bilangan pecahan
			if (type == typeof(float) || type == typeof(float?) ||
				type == typeof(decimal) || type == typeof(decimal?) ||
				type == typeof(double) || type == typeof(double?))
			{
				//hapus selain angka, titik (.), dan koma (,)
				if (sender is string)
					sender = System.Text.RegularExpressions.Regex.Replace(sender as string, @"[^\-\d\.,]", "");
				if (sender.IsNull())
					return type.IsNullable() ? null : defaultValue();
				try
				{
					//khusus untuk nullable, convert dulu ke tipe data dasarnya dulu
					if (type.IsNullable())
					{
						sender = System.Convert.ChangeType(sender, System.Nullable.GetUnderlyingType(type));
						return (System.ComponentModel.TypeDescriptor.GetConverter(type)).ConvertFrom(sender);
					}
					return System.Convert.ChangeType(sender, type, System.Threading.Thread.CurrentThread.CurrentCulture);
				}
				catch (System.InvalidCastException) { return defaultValue(); }
				catch (System.ArgumentNullException) { return defaultValue(); }
				catch (System.FormatException) { return defaultValue(); }
				catch (System.OverflowException) { return defaultValue(); }
			}
			#endregion
			#region Konversi boolean
			if (type == typeof(bool) || type == typeof(bool?))
			{
				if (sender is string && !string.IsNullOrEmpty((string)sender))
				{
					string word = System.Text.RegularExpressions.Regex.Replace((string)sender, @"[^a-z0-9]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).ToLower().Trim();
					switch (word)
					{
						case "true":
						case "1":
							return true;
						default:
							return false;
					}
				}
				//berikan null jika diperbolehkan
				if ((sender == System.DBNull.Value || (sender is string && string.IsNullOrEmpty((string)sender))) &&
					type.IsNullable())
					return null;
				//nilai default
				return false;
			}
			#endregion
			#region Konversi ke DateTime
			if (type == typeof(System.DateTime) || type == typeof(System.DateTime?))
			{
				if (sender.IsNull())
					return type.IsNullable() ? null : defaultValue();
				//test dengan culture sesuai dengan current thread
				var testingCulture = culture ?? System.Threading.Thread.CurrentThread.CurrentCulture;
				var usCulture = new System.Globalization.CultureInfo("en-US");
				System.Func<string, dynamic> onError = format =>
				{
					//test dengan culture sesuai yang aktif
					if (culture == null)
						return To(sender, type, false, format, testingCulture);
					//test dengan culture en-US
					if (!testingCulture.Equals(usCulture))
						return To(sender, type, false, format, usCulture);
					return defaultValue();
				};
				try
				{
					dateFormat = dateFormat.Trim();
					return isExcelDate
							   ? System.DateTime.FromOADate(System.Convert.ToDouble(sender))
							   : !string.IsNullOrEmpty(dateFormat)
									 ? System.DateTime.ParseExact(sender.ToString(), dateFormat, testingCulture)
									 : System.Convert.ToDateTime(sender);
				}
				catch (System.InvalidCastException) { return onError(dateFormat); }
				catch (System.ArgumentNullException) { return onError(dateFormat); }
				catch (System.ArgumentException) { return onError(dateFormat); }
				catch (System.FormatException) { return onError(dateFormat); }
			}
			#endregion
			#region Konversi ke string
			if (type == typeof(string))
			{
				try
				{
					if (sender.IsNull())
						return null;
					//konversi dari byte
					if (sourceType == typeof(byte[]))
					{
						string sBinary = "";
						byte[] buffer = (byte[])sender;
						for (int i = 0; i < buffer.Length; i++)
							sBinary += buffer[i].ToString("x2");
						return sBinary;
					}
					if (sender is System.DateTime && !string.IsNullOrEmpty(dateFormat))
						return culture != null
							? System.Convert.ToDateTime(sender).ToString(dateFormat, culture)
							: System.Convert.ToDateTime(sender).ToString(dateFormat);
					return culture != null ? System.Convert.ToString(sender, culture) : System.Convert.ToString(sender);
				}
				catch (System.NullReferenceException) { return defaultValue(); }
				catch (System.FormatException) { return defaultValue(); }
				catch (System.InvalidCastException) { return defaultValue(); }
			}
			#endregion
			#region Konversi ke enumeration
			if (type.IsEnum || (type.IsNullable() && System.Nullable.GetUnderlyingType(type).IsEnum))
			{
				try
				{
					//khusus untuk nullable, convert dulu ke tipe data dasarnya dulu
					var baseType = type.IsNullable() ? System.Nullable.GetUnderlyingType(type) : type;
					var output = System.Enum.Parse(baseType, sender.ToString());
					return baseType == type
							   ? output
							   : (System.ComponentModel.TypeDescriptor.GetConverter(type)).ConvertFrom(output);
				}
				catch (System.NullReferenceException) { return defaultValue(); }
				catch (System.InvalidCastException) { return defaultValue(); }
				catch (System.ArgumentNullException) { return defaultValue(); }
				catch (System.FormatException) { return defaultValue(); }
				catch (System.OverflowException) { return defaultValue(); }
			}
			#endregion
			//khusus untuk nullable, convert dulu ke tipe data dasarnya dulu
			try
			{
				if (sender.IsNull())
					return type.IsNullable() ? null : defaultValue();
				if (type.IsNullable())
				{
					sender = System.Convert.ChangeType(sender, System.Nullable.GetUnderlyingType(type));
					return (System.ComponentModel.TypeDescriptor.GetConverter(type)).ConvertFrom(sender);
				}
				//biarkan apa adanya untuk yang tidak dikenal
				return System.Convert.ChangeType(sender, type);
			}
			catch (System.InvalidCastException) { return defaultValue(); }
			catch (System.NullReferenceException) { return defaultValue(); }
			catch (System.ArgumentNullException) { return defaultValue(); }
			catch (System.FormatException) { return defaultValue(); }
			catch (System.OverflowException) { return defaultValue(); }
		}

		/// <summary>
		/// Safely convert to another data type
		/// </summary>
		/// <typeparam name="T">target data type</typeparam>
		/// <param name="sender"></param>
		/// <param name="isExcelDate">define wether to convert from old excel (&lt;= 2003) date (applied only for <see cref="System.DateTime"/>)</param>
		/// <param name="dateFormat">import date format (applied only for <see cref="System.DateTime"/>)</param>
		/// <param name="culture">default culture for convertion</param>
		/// <returns>T</returns>
		public static T To<T>(this object sender, bool isExcelDate = false, string dateFormat = "", System.Globalization.CultureInfo culture = null)
		{
			return sender.To(typeof(T), isExcelDate, dateFormat, culture);
		}

		/// <summary>
		/// Digunakan untuk memap object ke entitas baru. Didasarkan pada kesamaan nama property dan tipe data.
		/// Rule copy nilai:
		/// <list type="bullet">
		///		<item>
		///			<description>Pada source tidak ada attribute NotMapped</description>
		///		</item>
		///		<item>
		///			<description>Jika pada source ada attribute Column, maka nilainya yg akan dipakai sebagai nama field sebagai ganti nama property</description>
		///		</item>
		///		<item>
		///			<description>Jika pada target tidak nullable, tapi pada source nullable namun ada nilainya dan base/underlying typenya sama</description>
		///		</item>
		///		<item>
		///			<description>Jika pada target nullable, dan source tidak nullable tapi base/underlying typenya sama (nilai default tidak akan diconvert ke null)</description>
		///		</item>
		///		<item>
		///			<description>Jika target adalah Enum &amp; sumber adalah bilangan bulat (short, int, long) atau sebaliknya</description>
		///		</item>
		///		<item>
		///			<description>Nama property dan tipe data pada source &amp; sama</description>
		///		</item>
		/// </list>
		/// </summary>
		/// <typeparam name="T">class target</typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static T Map<T>(this object source) where T : class
		{
			T destination = System.Activator.CreateInstance<T>();
			if (source == null)
				return destination;
			if (source is System.Data.SqlClient.SqlDataReader)
				return SqlDataReaderMapper<T>(source as System.Data.SqlClient.SqlDataReader);
			var propertyDestination = destination.GetType();
			var propertySource = source.GetType();
			var properties = propertySource.GetProperties(BindingFlags.Instance |
														  BindingFlags.GetProperty |
														  BindingFlags.Public);
			var isDestinationChildOfSource = propertyDestination == propertySource || propertyDestination.IsSubclassOf(propertySource);
			foreach (var pSource in properties)
			{
				//skip jika pada source terdapat attribute NotMapped
				if (System.Attribute.IsDefined(pSource, typeof(NotMappedAttribute)))
					continue;
				//gunakan nama kolom dari attribute Column sebagai ganti nama property jika ada
				var aliasAttribute =
					pSource.GetCustomAttributes(typeof(NotMappedAttribute), true);
				string fieldName =
					//abaikan penggunaan attribute Column jika destination adalah inherit dari source
					!isDestinationChildOfSource &&
					aliasAttribute.Length > 0
						? ((ColumnAttribute)aliasAttribute[0]).Name
						: pSource.Name;
				var pOutput = propertyDestination.GetProperty(fieldName, BindingFlags.Instance |
																		 BindingFlags.Public |
																		 BindingFlags.SetProperty) ??
					//gunakan nama property asli jika tidak ada
							  propertyDestination.GetProperty(pSource.Name, BindingFlags.Instance |
																			BindingFlags.Public |
																			BindingFlags.SetProperty);
				//skip jika pada tujuan tidak ada
				if (pOutput == null)
					continue;
				try
				{
					var sourceType = pSource.PropertyType.IsNullable()
										 ? System.Nullable.GetUnderlyingType(pSource.PropertyType)
										 : pSource.PropertyType;
					var outputType = pOutput.PropertyType.IsNullable()
										 ? System.Nullable.GetUnderlyingType(pOutput.PropertyType)
										 : pOutput.PropertyType;
					var nilai = pSource.GetValue(source, null);
					if (pOutput != null &&
						(pOutput.PropertyType == pSource.PropertyType ||
						//kondisi khusus: jika tipe source nullable tapi base typenya sama asalkan nilainya ada
						 (pSource.PropertyType.IsNullable() && sourceType == pOutput.PropertyType && nilai != null) ||
						//atau jika target nullable tapi base typenya sama dengan tipe data source
						 (pOutput.PropertyType.IsNullable() && outputType == pSource.PropertyType) ||
						//diperbolehkan jika target enum dan sumber merupakan bilangan bulat atau sebaliknya
						 ((pOutput.PropertyType.IsEnum || pOutput.PropertyType.IsInteger()) &&
						  (sourceType.IsEnum || sourceType.IsInteger() || (pSource.PropertyType.IsNullable() && nilai != null))) ||
						 ((pOutput.PropertyType.IsNullable() && (outputType.IsEnum || outputType.IsInteger())) &&
						  (pSource.PropertyType.IsEnum || pSource.PropertyType.IsInteger()))) &&
						pOutput.CanWrite)
						pOutput.SetValue(destination, nilai.To(pOutput.PropertyType), null);
				}
				catch (TargetInvocationException) { }
				catch (TargetException) { }
				catch (System.NullReferenceException) { }
			}
			return destination;
		}

		/// <summary>
		/// Extract pasangan nama column &amp; property.
		/// Jika tidak ada attribute Column, maka menggunakan nama column yg sama dengan nama property.
		/// Yang mempunyai attribute NotMap akan diskip
		/// </summary>
		/// <param name="type"></param>
		/// <param name="excludeAttributes">attribute to be excluded</param>
		/// <returns></returns>
		internal static List<MappingConfiguration> ExtractColumn(this System.Type type, System.Type[] excludeAttributes = null)
		{
			var output = new List<MappingConfiguration>();
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var property in properties)
			{
				if (System.Attribute.IsDefined(property, typeof(NotMappedAttribute)))
					continue;
				bool skip = false;
				if (excludeAttributes != null)
					foreach (var attribute in excludeAttributes)
					{
						if (System.Attribute.IsDefined(property, attribute, true))
						{
							skip = true;
							break;
						}
					}
				if (skip)
					continue;
				output.Add(new MappingConfiguration
				{
					ColumnName = System.Attribute.IsDefined(property, typeof(ColumnAttribute)) ? ((ColumnAttribute)property.GetCustomAttributes(typeof(ColumnAttribute), true)[0]).Name : property.Name,
					PropertyName = property.Name
				});
			}
			return output;
		}

		/// <summary>
		/// Extract pasangan nama column &amp; property.
		/// Jika tidak ada attribute Column, maka menggunakan nama column yg sama dengan nama property.
		/// Yang mempunyai attribute NotMap akan diskip
		/// </summary>
		/// <typeparam name="T">tipe data object yg akan diextract</typeparam>
		/// <param name="data">object yg akan diextract</param>
		/// <param name="excludeAttributes">attribute to be excluded</param>
		/// <returns></returns>
		public static List<MappingConfiguration> ExtractColumn<T>(this T data, System.Type[] excludeAttributes = null)
			where T : class
		{
			var output = new List<MappingConfiguration>();
			if (data == null)
				return output;
			return data.GetType().ExtractColumn(excludeAttributes);
		}

		/// <summary>
		/// Extract pasangan nama column &amp; property hanya yang mempunyai attribute yang ditentukan.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="attributes">filter property yang mempunyai attribute ini</param>
		/// <returns></returns>
		public static List<MappingConfiguration> ExtractColumnWithAttributes(this System.Type type, System.Type[] attributes)
		{
			var output = new List<MappingConfiguration>();
			if (type == null)
				return output;
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			output.AddRange(
				from property in properties
				where attributes.Any(attribute => System.Attribute.IsDefined(property, attribute, true))
				select new MappingConfiguration
				{
					ColumnName =
						System.Attribute.IsDefined(property, typeof(ColumnAttribute))
							? ((ColumnAttribute)property.GetCustomAttributes(typeof(ColumnAttribute), true)[0]).Name
							: property.Name,
					PropertyName = property.Name
				});
			return output;
		}

		/// <summary>
		/// Extract pasangan nama column &amp; property hanya yang mempunyai attribute yang ditentukan.
		/// </summary>
		/// <typeparam name="T">tipe data object yg akan diextract</typeparam>
		/// <param name="data">object yg akan diextract</param>
		/// <param name="attributes">filter property yang mempunyai attribute ini</param>
		/// <returns></returns>
		internal static List<MappingConfiguration> ExtractColumnWithAttributes<T>(this T data, System.Type[] attributes)
			where T : class
		{
			return data == null ? new List<MappingConfiguration>() : typeof(T).ExtractColumnWithAttributes(attributes);
		}

		/// <summary>
		/// Map SqlDataReader to strong type data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static T SqlDataReaderMapper<T>(this System.Data.SqlClient.SqlDataReader reader)
			where T : class
		{
			T destination = System.Activator.CreateInstance<T>();
			if (reader == null || reader.IsClosed)
				return destination;
			var mappings = ExtractColumn(destination);
			foreach (var mapping in mappings)
			{
				try
				{
					destination.SetFieldValue(mapping.PropertyName, reader[mapping.ColumnName]);
				}
				catch (KeyNotFoundException) { }
			}
			return destination;
		}

		/// <summary>
		/// Abstraction layer untuk mendapatkan nilai variable. 
		/// Digunakan supaya tidak diperlukan editing sewaktu tipe data variable direfactore dari nullable ke non nullable.
		/// Digunakan bersamaan dengan Nullable&lt;T&gt;.GetValueOrDefault()
		/// </summary>
		/// <typeparam name="T">tipe data variable</typeparam>
		/// <param name="data">variable</param>
		/// <returns></returns>
		public static T GetValueOrDefault<T>(this T data) where T : struct
		{
			return data;
		}

		/// <summary>
		/// Get attribute value.
		/// </summary>
		/// <typeparam name="TAttribute">typeof attribute</typeparam>
		/// <typeparam name="TExpected">typeof return value from attribute</typeparam>
		/// <param name="data"></param>
		/// <param name="expression">get public property from attribute using lambda expression</param>
		/// <returns></returns>
		public static TExpected GetAttributeValue<TAttribute, TExpected>(this object data, System.Func<TAttribute, TExpected> expression)
			where TAttribute : System.Attribute
		{
			if (data == null)
				return default(TExpected);
			TAttribute attribute = data.GetType()
				.GetMember(data.ToString())[0].GetCustomAttributes(typeof(TAttribute), false)
				.Cast<TAttribute>()
				.SingleOrDefault();
			return attribute == null ? default(TExpected) : expression(attribute);
		}

		/// <summary>
		/// Get property value from entity
		/// </summary>
		/// <param name="data">entity</param>
		/// <param name="propertyName">property name</param>
		/// <returns>object</returns>
		public static dynamic GetFieldValue<T>(this T data, string propertyName)
			where T : class
		{
			if (data.IsPrimitive())
				return null;
			System.Type type = data.GetType();
			var defaultValue = System.Activator.CreateInstance(type);
			try
			{
				var property = data.GetType()
								   .GetProperty(propertyName,
												BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty);
				var output = property.GetValue(data, null);
				return output.To(property.PropertyType);
			}
			catch (System.ArgumentNullException) { return defaultValue; }
			catch (TargetException) { return defaultValue; }
			catch (TargetInvocationException) { return defaultValue; }
			catch (TargetParameterCountException) { return defaultValue; }
			catch (System.NullReferenceException) { return defaultValue; }
		}

		/// <summary>
		/// Set property value in entity
		/// </summary>
		/// <param name="data">entity</param>
		/// <param name="propertyName">property name</param>
		/// <param name="propertyValue">new property value</param>
		/// <param name="isExcelDate">determine wether <paramref name="propertyValue"/> is old excel date (excel prior to 1997), only applied when <paramref name="propertyName"/> in entity is System.DateTime</param>
		/// <param name="dateFormat">date format (only applied when <paramref name="propertyName"/> in entity is System.DateTime</param>
		public static void SetFieldValue<T>(this T data, string propertyName, object propertyValue, bool isExcelDate = false, string dateFormat = "")
			where T : class 
		{
			if (data.IsPrimitive())
				return;
			var columnInfo = data.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			//pastikan field bisa ditulis
			if (columnInfo == null || !columnInfo.CanWrite)
				return;
			//set nilai, convert sesuai tipe data property field
			columnInfo.SetValue(data, propertyValue.To(columnInfo.PropertyType, isExcelDate, dateFormat), null);
		}
	}
}
