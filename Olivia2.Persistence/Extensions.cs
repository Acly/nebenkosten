using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Persistence {

	public static class TypeExtensions {

		public static bool HasInterface(this Type type, Type iface) {
			return type.FindInterfaces((t, o) => t.ToString() == o.ToString(), iface).Length > 0;
		}
	}

	public static class PropertyExtension {

		public static bool HasAttribute(this PropertyInfo property, Type attribute) {
			return property.GetCustomAttributes(true).Any(
				(a) => { 
					return a.GetType().ToString() == attribute.ToString(); }
				);
		}
	}
}
