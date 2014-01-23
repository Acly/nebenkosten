using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class ProjectManager : ObservableCollection<Project>, INotifyPropertyChanged {

		public ProjectManager() {}

		public event EventHandler CurrentProjectChanged;

		public new void Add(Project project) {
			base.Add(project);
			Current = project;
		}

		public Project Current {
			get { return current; }
			set {
				if ( current != value ) {
					current = value;
					if ( CurrentProjectChanged != null ) CurrentProjectChanged(this, new EventArgs());
					OnPropertyChanged(new PropertyChangedEventArgs("Current"));
					OnPropertyChanged(new PropertyChangedEventArgs("DoesProjectExist"));
				}
			}
		}

		public bool DoesProjectExist {
			get { return (current != Project.Empty && current != null); }
		}

		public Project current = Project.Empty;

	}

}
