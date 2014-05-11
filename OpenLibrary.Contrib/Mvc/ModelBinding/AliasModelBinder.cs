using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace OpenLibrary.Mvc.ModelBinding
{
	/// <summary>
	/// Bind property with different alias. Use together with BindAliasAttribute.
	/// <see cref="http://ole.michelsen.dk/blog/bind-a-model-property-to-a-different-named-query-string-field/"/>
	/// </summary>
	public class AliasModelBinder : DefaultModelBinder
	{
		protected override PropertyDescriptorCollection GetModelProperties(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var output = base.GetModelProperties(controllerContext, bindingContext);
			var additional = new List<PropertyDescriptor>();
			foreach (var p in this.GetTypeDescriptor(controllerContext, bindingContext).GetProperties().Cast<PropertyDescriptor>())
			{
				foreach (var attr in p.Attributes.OfType<Annotation.BindAliasAttribute>())
				{
					additional.Add(new Annotation.BindAliasAttribute.AliasedPropertyDescriptor(attr.Alias, p));
					if (bindingContext.PropertyMetadata.ContainsKey(p.Name) &&
						!bindingContext.PropertyMetadata.ContainsKey(attr.Alias))
						bindingContext.PropertyMetadata.Add(attr.Alias, bindingContext.PropertyMetadata[p.Name]);
				}
			}

			return new PropertyDescriptorCollection(output.Cast<PropertyDescriptor>().Concat(additional).ToArray());
		}
	}
}
