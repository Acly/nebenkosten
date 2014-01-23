using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

    public class Property : Base {

		public const string DefaultName = "Neues Gebäude";

        public Property() {
			flats.CollectionChanged += (o, e) => {
				NotifyPropertyChanged("Flats");
				foreach ( Flat flat in e.NewItems ) {
					flat.PropertyChanged += (f, p) => NotifyPropertyChanged(p.PropertyName);
				}
			};
		}

		public Property(string n, string a)
			: this() {
				name = n;
				address = a;
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
        public string Address {
            get { return address; }
            set {
                if ( address != value ) {
                    address = value;
                    NotifyPropertyChanged("Address");
                }
            }
        }

		public ObservableCollection<Flat> Flats {
			get { return flats; }
		}

		public Flat CreateFlat() {
			var f = new Flat(this);
			Flats.Add(f);
			return f;
		}

		public PropertyManager PropertyManager { get; internal set; }

		public override string this[string property] {
			get {
				if ( property == "Name" && string.IsNullOrEmpty(Name) )
					return "Gebäudename muss angegeben werden";
				else return null;
			}
		}

		public override string ToString() {
			return Name;
		}

        string name = DefaultName;
        string address;
		ObservableCollection<Flat> flats = new ObservableCollection<Flat>();
    }

}
