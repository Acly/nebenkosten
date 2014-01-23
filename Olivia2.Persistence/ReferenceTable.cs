using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Persistence {

	public struct Property {
		public Property(PropertyInfo pi, object instance) {
			Info = pi;
			Instance = instance;
		}
		public PropertyInfo Info;
		public object Instance;
	}

	public class ReferenceTable : Dictionary<Guid, IList<Property>> {

		public void Add(Guid id, PropertyInfo property, object instance) {
			if ( !ContainsKey(id) ) {
				this[id] = new List<Property>();
			}
			this[id].Add(new Property(property, instance));
		}

		public void Update(IdTable idtable) {
			foreach ( object o in idtable.Keys ) {
				var id = idtable[o];
				if ( ContainsKey(id) ) {
					foreach ( Property property in this[id] ) {
						property.Info.SetValue(property.Instance, o, null);
					}
					Remove(id);
				}
			}
		}
	}

}
