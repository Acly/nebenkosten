using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Acly.Olivia2.Model {
   
    public class AdvancePaymentCollection : ObservableCollection<AdvancePayment>  {
		
        internal AdvancePaymentCollection(Lessee host) {
			lessee = host;
		}

		public new void Add(AdvancePayment payment) {
			base.Add(payment);
		}

        /// <summary>Retrieve the amount of advance payments per month at the given date.</summary>
        public double Get(DateTime time) {
            double result = this[0].Amount;
			DateTime last = this[0].Start;

            foreach ( var p in this ) {
				if ( p.Start <= time && p.Start > last ) {
					result = p.Amount;
				}
            }
            return result;
        }

		/// <summary>Retrieve the amount of the most recent advance payment.</summary>
		public double Current {
			get { return Get(DateTime.MaxValue); }
		}

		public DateTime LastChange {
			get { return this.Max(payment => payment.Start); }
		}

		public Lessee Lessee {
			get { return lessee; }
		}

		Lessee lessee;
    }
}
