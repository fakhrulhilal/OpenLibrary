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
			string fieldName = null;
			string captionName = null;
			int? width = null;
			int? sequence = null;
			System.Type dataType = null;
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
				fieldName = null;
				captionName = null;
				width = null;
				sequence = null;
				dataType = property.PropertyType;
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
				//if (string.IsNullOrEmpty(fieldName))
				//{
				//	//jika punya atribut column, gunakan nilainya sebagai pengganti nama field
				//	var columnAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), false);
				//	if (columnAttribute.Length > 0)
				//		fieldName = ((ColumnAttribute)columnAttribute[0]).Name;
				//}
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
	}
}
