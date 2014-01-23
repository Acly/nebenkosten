using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public enum CostMode {
		Lessee, Member, Flat, External
	}

	public class Cost : Base {

		public const string DefaultName = "Neue Ausgabe";

		internal Cost() { }

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
		public CostMode Mode {
			get { return mode; }
			set {
				if ( mode != value ) {
					if ( mode == CostMode.External ) {
						foreach ( var a in Options ) a.Amount = 0;
					}
					mode = value;
					if ( mode == CostMode.External ) {
						CalculateTotalAmountFromExternal();
					}
					NotifyPropertyChanged("Mode");
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

		[Serialize]
		public bool AffectsVacancy {
			get { return vacancy; }
			set {
				if ( vacancy != value ) {
					vacancy = value;
					NotifyPropertyChanged("Vacancy");
				}
			}
		}

        [Serialize]
        public double VacancyAmount {
            get { return vacancyAmount; }
            set {
                vacancyAmount = value;
                NotifyPropertyChanged("VacancyAmount");
                CalculateTotalAmountFromExternal();
            }
        }

		public void AddLessee(Lessee l) {
			if ( options.Count(o => o.Lessee == l) < 1 ) {
				var o = new CostOptions(this, l);
				o.PropertyChanged += (s, e) => {
					if (e.PropertyName == "Amount") {
						CalculateTotalAmountFromExternal();
					}
				};
				options.Add(o);
			}
		}

		public void RemoveLessee(Lessee l) {
			var opt = options.FirstOrDefault(o => o.Lessee == l);
			if ( opt != null ) options.Remove(opt);
		}

		void CalculateTotalAmountFromExternal() {
            if (Mode == CostMode.External) {
                Amount = 0;
                if (AffectsVacancy) Amount += VacancyAmount;
                foreach (var a in Options) {
                    if (a.Affected && !a.Exempt) Amount += a.Amount;
                }
            }
		}

		public ObservableCollection<CostOptions> Options {
			get { return options; }
		}

		public override string this[string property] {
			get {
				if ( property == "Name" ) {
					if ( string.IsNullOrEmpty(Name) ) return "Name für den Kostenpunkt muss angegeben werden.";
				}
				return null;
			}
		}
		
		string name = DefaultName;
		CostMode mode = CostMode.Lessee;
		double amount = 0;
		bool vacancy = true;
        double vacancyAmount = 0;
		ObservableCollection<CostOptions> options = new ObservableCollection<CostOptions>();
	}

}
