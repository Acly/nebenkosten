using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

    public class Flat : Base {

		public const string DefaultName = "Neue Fläche";

		internal Flat(Property p) {
			property = p;
		}

		internal Flat(Property p, string n, double s) {
			property = p;
			name = n;
			size = s;
		}
				
        public Property Property {
            get { return property; }
            set {
                if ( property != value ) {
                    property = value;
                    NotifyPropertyChanged("Property");
                }
            }
        }

		[Serialize]
        public string Name {
            get { return name; }
            set {
                if ( name != value ) {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

		[Serialize]
        public double Size {
            get { return size; }
            set {
                if ( size != value ) {
                    size = value;
                    NotifyPropertyChanged("Size");
                }
            }
        }

		public override string this[string property] {
			get {
				if ( property == "Property" && Property == null )
					return "Gebäude zu dem die Fläche gehört muss angegeben werden.";
				else if ( property == "Name" && string.IsNullOrEmpty(Name) )
					return "Flächenbezeichnung muss angegeben werden.";
				else if ( property == "Size" && Size <= 0 )
					return "Fläche muss mehr als 0 m² messen.";
				else return null;
			}
		}

		public override string ToString() {
			return Name;
		}

        Property property;
        string name = DefaultName;
        double size = 0;
    }

}
