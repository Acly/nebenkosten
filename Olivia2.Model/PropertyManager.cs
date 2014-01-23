using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;

namespace Acly.Olivia2.Model {

    public class PropertyManager : ObservableCollection<Property> {
		
        public PropertyManager() {}

		public new void Add(Property property) {
			base.Add(property);
			property.PropertyManager = this;
		}

		public Property Create() {
			var p = new Property();
			Add(p);
			return p;
		}
     }

}
