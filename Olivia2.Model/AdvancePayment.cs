using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

    public class AdvancePayment : Base {

		public AdvancePayment() {
			Start = DateTime.Now;
			Amount = 0;
		}

        public AdvancePayment(DateTime start, double amount) {
			Start = start;
			Amount = amount;
        }

		[Serialize]
        public DateTime Start {
            get { return start; }
            set {
                if ( start != value ) {
                    start = value;
                    NotifyPropertyChanged("Start");
                }
            }
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

		public override string this[string property] {
			get {
				if ( Amount < 0 ) return "Vorauszahlungsbetrag muss positiv sein.";
				return null;
			}
		}

        DateTime start;
        double amount;
    }

}
