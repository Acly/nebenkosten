using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class CostOptions : Base {
        
		internal CostOptions(Cost host, Lessee appliesTo) {
			cost = host;
			lessee = appliesTo;
		}

		public Cost Cost {
			get { return cost; }
		}

		[Serialize]
		public Lessee Lessee {
			get { return lessee; }
		}

		[Serialize]
		public double Amount {
			get { return amount; }
			set {
				if ( amount != value ) {
					amount = value;
					NotifyPropertyChanged("Amount");
				}
			}
		}

		[Serialize]
		public bool Affected {
			get { return affected; }
			set {
				if ( affected != value ) {
					affected = value;
					NotifyPropertyChanged("Affected");
					if ( Affected && Exempt ) Exempt = false;
				}
			}
		}

		[Serialize]
		public bool Exempt {
			get { return exempt; }
			set {
				if ( exempt != value ) {
					exempt = value;
					NotifyPropertyChanged("Exempt");
					if ( Exempt && Affected ) Affected = false;
				}
			}
		}

		public override string this[string property] {
			get {
				if ( property == "Amount" && Amount < 0 ) {
					return "Betrag muss positiv sein.";
				} 
				return null;
			}
		}

		Lessee lessee;
		Cost cost;
		double amount;
		bool affected = true;
		bool exempt = false;
	}

}
