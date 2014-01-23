using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class FlatAssignmentCollection : ObservableCollection<FlatAssignment> {

		public FlatAssignmentCollection(Project host, Flat flat) {
			this.flat = flat;
			project = host;
		}

		[Serialize]
		public Flat Flat {
			get { return flat; }
		}

		public Project Project {
			get { return project; }
		}

		public new void Add(FlatAssignment assignment) {
			assignment.HostingCollection = this;
			base.Add(assignment);
		}

		// Duration in Months the Flat of this FAC remains vaccant (has no assignments)
		public int VacantDuration {
			get { return 24 - this.Sum(fa => fa.EndIntervalIndex - fa.StartIntervalIndex + 1); }
		}
		
		Flat flat;
		Project project;
	}

}
