using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

    public class Lessee : Base {

		public const string DefaultName = "Neue Mietpartei";
		
        public Lessee() {
			payments = new AdvancePaymentCollection(this);
			payments.CollectionChanged += (o, e) => {
				NotifyPropertyChanged("Payments");
				foreach ( AdvancePayment payment in e.NewItems ) {
					payment.PropertyChanged += (p, args) => NotifyPropertyChanged(args.PropertyName);
				}
			};
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
		public int Members { 
            get { return members; }
            set {
                if ( members != value ) {                    
                    members = value;
                    NotifyPropertyChanged("Members");
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

		public AdvancePaymentCollection Payments {
            get { return payments; }
        }
		
		public override string this[string property] {
			get {
				if ( property == "Name" && string.IsNullOrEmpty(Name) )
					return "Name der Mietpartei muss angegeben werden.";
				else if ( property == "Members" && Members < 1 )
					return "Einer oder mehr Mieter müssen der Mietpartei angehören.";
				else return null;
			}
		}

		public override string ToString() {
			return Name;
		}

        string name = DefaultName;
        int members = 1;
        string address;
        AdvancePaymentCollection payments;
    }

}
