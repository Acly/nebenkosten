using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class FlatAssignment : Base {

		public FlatAssignment(Project project) {
			Project = project;
			start = min = project.StartDate;
			end = max = project.EndDate;
		}

		[Serialize]
		public Lessee Lessee {
			get { return lessee; }
			set {
				lessee = value;
				NotifyPropertyChanged("Lessee");
			}
		}

		[Serialize]
		public DateTime Start {
			get { return start; }
			set {
				if ( start != value ) {
					start = value;
					startinterval = TimeInterval.Find(Project.TimeIntervals, i => i.Start == value, 0);
					NotifyPropertyChanged("StartIntervalIndex");
					NotifyPropertyChanged("Start");
				}
			}
		}

		[Serialize]
		public DateTime End {
			get { return end; }
			set {
				if ( end != value ) {
					end = value;
					endinterval = TimeInterval.Find(Project.TimeIntervals, i => i.End == value, 23);
					NotifyPropertyChanged("EndIntervalIndex");
					NotifyPropertyChanged("End");
				}
			}
		}

		public int StartIntervalIndex {
			get { return startinterval; }
			set {
				if ( startinterval != value && value > 0 ) {
					startinterval = value;
					start = Project.TimeIntervals[startinterval].Start;
					NotifyPropertyChanged("StartIntervalIndex");
					NotifyPropertyChanged("Start");
				}
			}
		}

		public int EndIntervalIndex {
			get { return endinterval; }
			set {
				if ( endinterval != value && value > 0 ) {
					endinterval = value;
					end = Project.TimeIntervals[endinterval].End;
					NotifyPropertyChanged("EndIntervalIndex");
					NotifyPropertyChanged("End");
				}
			}
		}

		public double Duration {
			get { return EndIntervalIndex - StartIntervalIndex + 1; }
		}

		public bool IsDefault {
			get { return Start == min && End == max; }
		}

		public override string this[string property] {
			get {
				if ( property == "Lessee" && Lessee == null )
					return "Mietpartei muss für den Mietzeitraum angegeben werden.";
				else if ( property == "Start" && (Start < min || Start > max) )
					return "Begin des Mietzeitraums ist außerhalb des Abrechnungszeitraums.";
				else if ( property == "End" && (End < min || End > max) )
					return "Ende des Mietzeitraums ist außerhalb des Abrechnungszeitraums.";
				else if ( (property == "Start" || property == "End") && End <= Start )
					return "Anfang des Mietzeitraums muss zeitlich vor dem Ende des Mietzeitraums liegen.";
				else return null;
			}
		}

		public FlatAssignmentCollection HostingCollection {
			get { return collection; }
			internal set { collection = value; }
		}

		public Project Project { get; protected set; }

		FlatAssignmentCollection collection;
		Lessee lessee;
		DateTime start;
		DateTime end;
		DateTime min;
		DateTime max;
		int startinterval = 0;
		int endinterval = 23;
	}

}
