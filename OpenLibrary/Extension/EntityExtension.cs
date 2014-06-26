using System.Reflection;
using OpenLibrary.Annotation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OpenLibrary.Extension
{
	/// <summary>
	/// Extension for instance of class
	/// </summary>
	public static class EntityExtension
	{
		/// <summary>
		/// Extract nama field dari sebuah object
		/// </summary>
		/// <param name="type">type</param>
		/// <param name="excludes">daftar nama field yang diexclude dari output (default: CreatedTime, CreatedBy, ModifiedTime, ModifiedBy)</param>
		/// <returns>List&lt;ColumnOption&gt;</returns>
		public static List<MappingOption> ExtractField(this System.Type type, string[] excludes = null)
		{
			//tidak diperkenankan untuk tipe data primitive
			if (type.IsPrimitive())
				return null;
			//secara default, exclude CreatedTime, CreatedBy, ModifiedTime, ModifiedBy
			if (excludes == null)
				excludes = new[] { "CreatedTime", "CreatedBy", "ModifiedTime", "ModifiedBy" };
			List<MappingOption> output = new List<MappingOption>();
			var properties = type.GetProperties();
			foreach (var property in properties)
			{
				//skip yang diexclude
				if (excludes.Contains(property.Name))
					continue;
				//tidak perlu ambil property yg tidak dimap ke table
				if (System.Attribute.IsDefined(property, typeof(NotMappedAttribute)))
					continue;
				//hanya extract non complex type
				if (!property.PropertyType.IsPrimitive())
					continue;
				string fieldName = null;
				string captionName = null;
				int? width = null;
				int? sequence = null;
				System.Type dataType = property.PropertyType;
				var mappingAttributes = property.GetCustomAttributes(typeof(MappingOptionAttribute), false);
				if (mappingAttributes.Length > 0)
				{
					var mappingAttribute = ((MappingOptionAttribute)mappingAttributes[0]);
					if (!string.IsNullOrEmpty(mappingAttribute.Caption))
						captionName = mappingAttribute.Caption;
					if (!string.IsNullOrEmpty(mappingAttribute.Field))
						fieldName = mappingAttribute.Field;
					if (mappingAttribute.Width != default(int))
						width = mappingAttribute.Width;
					if (mappingAttribute.Sequence.HasValue)
						sequence = mappingAttribute.Sequence.Value;
					if (mappingAttribute.Type != null)
						dataType = mappingAttribute.Type;
				}
				if (string.IsNullOrEmpty(captionName))
				{
					//jika punya atribut display, gunakan nilainya sebagai pengganti caption
					var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), false);
					if (displayAttribute.Length > 0)
						captionName = ((DisplayAttribute)displayAttribute[0]).GetName();
				}
				fieldName = fieldName ?? property.Name;
				captionName = captionName ?? property.Name;
				width = width ?? 50;
				sequence = sequence ?? (output.Max(m => m.Sequence) ?? 0) + 1;
				output.Add(new MappingOption
				{
					Field = fieldName,
					Caption = captionName,
					Sequence = sequence,
					Width = width.GetValueOrDefault(),
					Type = dataType
				});
			}
			return output;
		}

		/// <summary>
		/// Extract nama field dari sebuah object
		/// </summary>
		/// <param name="data">object</param>
		/// <param name="excludes">daftar nama field yang diexclude dari output (default: CreatedTime, CreatedBy, ModifiedTime, ModifiedBy)</param>
		/// <returns>string[]</returns>
		public static List<MappingOption> ExtractField<T>(this T data, string[] excludes = null)
			where T : class
		{
			return ExtractField(typeof(T), excludes);
		}

		/// <summary>
		/// Compare property field from <paramref name="oldEntity"/> to <paramref name="newEntity"/>
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="oldEntity">old entity</param>
		/// <param name="newEntity">new entity</param>
		/// <param name="excludeProperties">excludes property</param>
		/// <param name="emptyString">empty string description</param>
		/// <param name="noChangeString">no change string description</param>
		/// <returns></returns>
		public static string Compare<T>(this T oldEntity, T newEntity, string[] excludeProperties, string emptyString = "(EMPTY)", string noChangeString = "(NO CHANGE)")
			where T : class
		{
			var excludes = excludeProperties != null && excludeProperties.Length > 0
							   ? excludeProperties.ToList()
							   : new List<string>();
			var properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
			var changes = new List<string>();
			foreach (var property in properties)
			{
				if (excludes.Contains(property.Name) ||
					!property.PropertyType.IsPrimitive())
					continue;
				string fieldName = property.Name;
				var oldValue = oldEntity.GetFieldValue(property.Name);
				var newValue = newEntity.GetFieldValue(property.Name);
				string oldString = oldValue == null ? emptyString : oldValue.ToString();
				string newString = newValue == null ? emptyString : newValue.ToString();
				if (oldValue is System.DateTime)
				{
					oldString = ((System.DateTime)oldValue).ToString("yyyy-MM-dd");
					newString = ((System.DateTime)newValue).ToString("yyyy-MM-dd");
				}
				else if (oldValue is bool)
				{
					oldString = ((bool)oldValue) ? "yes" : "no";
					newString = ((bool)newValue) ? "yes" : "no";
				}
#if NET40
				var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().ToList();
#else
				var displayAttribute = property.GetCustomAttributes<DisplayAttribute>().ToList();
#endif
				if (displayAttribute.Any())
					fieldName = displayAttribute.First().GetName();
				changes.Add(oldString == newString
								? string.Format("{0} = {1} {2}", fieldName, oldString, noChangeString)
								: string.Format("{0} = {1} -> {2}", fieldName, oldString, newString));
			}
			return string.Join(", ", changes);
		}

		/// <summary>
		/// Compare property field from <paramref name="oldEntity"/> to <paramref name="newEntity"/>
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="oldEntity">old entity</param>
		/// <param name="newEntity">new entity</param>
		/// <param name="excludeAttributes">excludes property which has certain attribute</param>
		/// <param name="emptyString">empty string description</param>
		/// <param name="noChangeString">no change string description</param>
		/// <returns></returns>
		public static string Compare<T>(this T oldEntity, T newEntity, System.Type[] excludeAttributes,
										string emptyString = "(EMPTY)", string noChangeString = "(NO CHANGE)")
			where T : class
		{
			return Compare(oldEntity, newEntity, GetExcluded<T>(excludeAttributes), emptyString, noChangeString);
		}

		/// <summary>
		/// Compare property field from <paramref name="oldEntity"/> to <paramref name="newEntity"/>
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="oldEntity">old entity</param>
		/// <param name="newEntity">new entity</param>
		/// <param name="excludeProperties">excludes property</param>
		/// <param name="emptyString">empty string description</param>
		/// <param name="noChangeString">no change string description</param>
		/// <returns></returns>
		public static string Compare<T>(this T oldEntity, T newEntity, System.Func<T, object> excludeProperties,
										string emptyString = "(EMPTY)", string noChangeString = "(NO CHANGE)")
			where T : class
		{
			return Compare(oldEntity, newEntity, GetExcluded(excludeProperties), emptyString, noChangeString);
		}

		/// <summary>
		/// Compare property field from <paramref name="oldEntity"/> to <paramref name="newEntity"/>
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="oldEntity">old entity</param>
		/// <param name="newEntity">new entity</param>
		/// <param name="emptyString">empty string description</param>
		/// <param name="noChangeString">no change string description</param>
		/// <returns></returns>
		public static string Compare<T>(this T oldEntity, T newEntity, string emptyString = "(EMPTY)", string noChangeString = "(NO CHANGE)")
			where T : class
		{
			return Compare(oldEntity, newEntity, new string[0], emptyString, noChangeString);
		}

		/// <summary>
		/// Describe entity property and value
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="entity">entity</param>
		/// <param name="excludeProperties">excludes property</param>
		/// <param name="emptyString">empty string description</param>
		/// <returns></returns>
		public static string Describe<T>(this T entity, string[] excludeProperties, string emptyString = "(EMPTY)")
			where T : class
		{
			var excludes = excludeProperties != null && excludeProperties.Length > 0
							   ? excludeProperties.ToList()
							   : new List<string>();
			var properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
			var description = new List<string>();
			foreach (var property in properties)
			{
				if (excludes.Contains(property.Name) ||
					!property.PropertyType.IsPrimitive())
					continue;
				string fieldName = property.Name;
				var value = entity.GetFieldValue(property.Name);
				string wordValue = value == null ? emptyString : value.ToString();
				if (value is System.DateTime)
					wordValue = ((System.DateTime)value).ToString("yyyy-MM-dd");
				else if (value is bool)
					wordValue = ((bool)value) ? "yes" : "no";
#if NET40
				var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().ToList();
#else
				var displayAttribute = property.GetCustomAttributes<DisplayAttribute>().ToList();
#endif
				if (displayAttribute.Any())
					fieldName = displayAttribute.First().GetName();
				description.Add(string.Format("{0} = {1}", fieldName, wordValue));
			}
			return string.Join(", ", description);
		}

		/// <summary>
		/// Describe entity property and value
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="entity">entity</param>
		/// <param name="excludeProperties">excludes property</param>
		/// <param name="emptyString">empty string description</param>
		/// <returns></returns>
		public static string Describe<T>(this T entity, System.Func<T, object> excludeProperties, string emptyString = "(EMPTY)")
			where T : class
		{
			return Describe(entity, GetExcluded(excludeProperties), emptyString);
		}

		/// <summary>
		/// Describe entity property and value
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="entity">entity</param>
		/// <param name="excludeAttributes">excludes property which has certain attribute</param>
		/// <param name="emptyString">empty string description</param>
		/// <returns></returns>
		public static string Describe<T>(this T entity, System.Type[] excludeAttributes, string emptyString = "(EMPTY)")
			where T : class
		{
			return Describe(entity, GetExcluded<T>(excludeAttributes), emptyString);
		}

		/// <summary>
		/// Describe entity property and value
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="entity">entity</param>
		/// <param name="emptyString">empty string description</param>
		/// <returns></returns>
		public static string Describe<T>(this T entity, string emptyString = "(EMPTY)") where T : class
		{
			return Describe(entity, new string[0], emptyString);
		}

		private static string[] GetExcluded<T>(System.Func<T, object> excludeProperties) where T : class
		{
			var excludes = new string[0];
			if (excludeProperties != null)
			{
				var keySelector = excludeProperties(System.Activator.CreateInstance<T>());
				var properties = keySelector.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
				if (properties.Length > 0)
					excludes = properties.Select(m => m.Name).ToArray();
			}
			return excludes;
		}

		private static string[] GetExcluded<T>(System.Type[] excludeAttributes) where T : class 
		{
			var excludes = new List<string>();
			//only attribute type
			var excludeList = excludeAttributes.Where(type => typeof(System.Attribute).IsAssignableFrom(type)).ToList();
			var properties = typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
			if (excludeList.Count > 0 && properties.Length > 0)
			{
				//only exclude property which certain attribute
#if NET40
				excludes.AddRange(from property in properties
								  where excludeList.Any(type => property.IsDefined(type, false))
								  select property.Name);
#else
				excludes.AddRange(from property in properties
								  where excludeList.Any(property.IsDefined)
								  select property.Name);
#endif
			}
			return excludes.ToArray();
		}
	}
}
