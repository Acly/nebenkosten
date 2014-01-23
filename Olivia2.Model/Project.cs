using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class Project : Base {

		public static Project Empty = new Project("- Keine -");

		public Project() {
			StartMonth = 1;
			StartYear = DateTime.Now.Year;
			Name = "Abrechnung " + StartYear;
			Result = new ResultTable(this);
			Costs.CollectionChanged += (_, e) => {
				NotifyPropertyChanged("Costs");
                if (e.Action == NotifyCollectionChangedAction.Add)
				    foreach ( Cost c in e.NewItems ) NotifyIfChanged(c);
			};
		}

		void NotifyIfChanged(Cost c) {
			c.PropertyChanged += (__, p) => NotifyPropertyChanged(p.PropertyName);
			foreach ( var option in c.Options ) {
				option.PropertyChanged += (___, o) => NotifyPropertyChanged(o.PropertyName);
			}
			c.Options.CollectionChanged += (collection, e) => {
                if (e.Action == NotifyCollectionChangedAction.Add)
				    foreach ( CostOptions option in e.NewItems )
					    option.PropertyChanged += (_, arg) => NotifyPropertyChanged(arg.PropertyName);
			};
		}

		void OnFlatAssignmentCollectionChange(object sender, NotifyCollectionChangedEventArgs e) {
			if ( e.Action == NotifyCollectionChangedAction.Add ) {
				foreach ( var a in e.NewItems ) {
					var assignment = (FlatAssignment)a;
					assignment.PropertyChanged += (___, p) => NotifyPropertyChanged(p.PropertyName);
					foreach ( var cost in Costs ) {
						cost.AddLessee(assignment.Lessee);
					}
				}
			} else if ( e.Action == NotifyCollectionChangedAction.Remove ) {
				foreach ( var a in e.OldItems ) {
					var assignment = (FlatAssignment)a;
					foreach ( var cost in Costs ) {
						cost.RemoveLessee(assignment.Lessee);
					}
				}
			}
			NotifyPropertyChanged("Assignments");
		}

		private Project(string name) {
			Name = name;
			Result = new ResultTable(this);
		}

        public void ApplyTemplate(Project other) {
            foreach (FlatAssignmentCollection assignmentcol in other.Assignments) {
                var fac = Assignments.First((c) => c.Flat == assignmentcol.Flat);
                foreach (FlatAssignment assignment in assignmentcol) {
                    if (assignment.End == other.EndDate) {
                        var fa = new FlatAssignment(this);
                        fa.StartIntervalIndex = 0;
                        fa.EndIntervalIndex = 23;
                        fa.Lessee = assignment.Lessee;
                        fac.Add(fa);
                    }
                }
            }
            foreach (Cost cost in other.Costs) {
                var c = CreateCost();
                c.AffectsVacancy = cost.AffectsVacancy;
                c.Mode = cost.Mode;
                c.Name = cost.Name;
                foreach (CostOptions options in cost.Options) {                    
                    var co = c.Options.FirstOrDefault((o) => o.Lessee == options.Lessee);
                    if (co != null) {
                        co.Affected = options.Affected;
                        co.Exempt = options.Exempt;
                    }
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
		public Property Property {
			get { return property; }
			set {
				if ( property != value ) {
					if ( property != null ) assignments.CollectionChanged -= OnPropertyAssignmentsChange;
					property = value;
					assignments = new ObservableCollection<FlatAssignmentCollection>();
					assignments.CollectionChanged += OnPropertyAssignmentsChange;
					foreach ( var f in value.Flats ) {
						assignments.Add(new FlatAssignmentCollection(this, f));
					}
					NotifyPropertyChanged("Property");
				}
			}
		}

		void OnPropertyAssignmentsChange(object o, NotifyCollectionChangedEventArgs e) {
			NotifyPropertyChanged("Assignments");
			if ( e.Action == NotifyCollectionChangedAction.Add ) {
				foreach ( var ac in e.NewItems ) {
					var item = (FlatAssignmentCollection)ac;
					item.CollectionChanged += OnFlatAssignmentCollectionChange;
				}
			} else if ( e.Action == NotifyCollectionChangedAction.Remove ) {
				foreach ( var ac in e.OldItems ) {
					var item = (FlatAssignmentCollection)ac;
					item.CollectionChanged -= OnFlatAssignmentCollectionChange;
					foreach ( var a in item ) {
						foreach ( var cost in Costs ) {
							cost.RemoveLessee(a.Lessee);
						}
					}
				}
			}
		}

		public int StartMonth {
			get { return StartDate.Month; }
			set {
				var date = new DateTime(StartYear, value, 1);
				StartDate = date;
			}
		}

		public int StartYear {
			get { return StartDate.Year; }
			set {
				var date = new DateTime(value, StartMonth, 1);
				StartDate = date;
			}
		}

		[Serialize]
		public DateTime StartDate {
			get { return start; }
			set {
				if ( start != value ) {
					start = value;
					IntervalStartDates = new ObservableCollection<string>(TimeIntervals.Select(i => i.ToString()));
					IntervalEndDates = new ObservableCollection<string>(TimeIntervals.Select(i => i.ToEndString()));
					NotifyPropertyChanged("StartDate");
					NotifyPropertyChanged("EndDate");
					NotifyPropertyChanged("StartMonth");
					NotifyPropertyChanged("StartYear");
					NotifyPropertyChanged("IntervalStartDates");
					NotifyPropertyChanged("IntervalEndDates");
				}
			}
		}

		public DateTime EndDate {
			get { return start.AddYears(1).AddSeconds(-1); }
		}

		public IList<TimeInterval> TimeIntervals {
			get { return TimeInterval.Within(StartDate, EndDate).ToList(); }
		}

		public ObservableCollection<string> IntervalStartDates { get; private set; }
		public ObservableCollection<string> IntervalEndDates { get; private set; }
		public ResultTable Result { get; private set; }

		public ObservableCollection<FlatAssignmentCollection> Assignments {
			get { return assignments; }
		}

		public ObservableCollection<Cost> Costs {
			get { return costs; }
		}

		public Cost CreateCost() {
			Cost c = new Cost();
			foreach ( var ac in Assignments ) {
				foreach ( var a in ac ) {
					c.AddLessee(a.Lessee);
				}
			}
			costs.Add(c);
			return c;
		}

		public override string this[string property] {
			get {
				if ( property == "Name" && string.IsNullOrEmpty(Name) )
					return "Projektname muss angegeben werden.";
				else if ( property == "Property" && Property == null )
					return "Gebäude für das die Abrechnung erstellt wird muss gewählt werden.";
				else return null;
			}
		}
		
		public override string ToString() {
			return Name;
		}

		string name;
		Property property = new Property();
		DateTime start;
		ObservableCollection<FlatAssignmentCollection> assignments = new ObservableCollection<FlatAssignmentCollection>();
		ObservableCollection<Cost> costs = new ObservableCollection<Cost>();
	}

}
