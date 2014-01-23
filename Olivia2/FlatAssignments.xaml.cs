using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Acly.Olivia2.Model;

namespace Acly.Olivia2 {

	public partial class FlatAssignments : UserControl {

		public FlatAssignments() {
			InitializeComponent();
			Reset();
            ((ProjectManager)FindResource("ProjectData")).CurrentProjectChanged += (p, _) => Reset();

			Loaded += (o, e) => {
				AssignDefaultFlat();
				Project.Property.Flats.CollectionChanged += (f, _) => AssignDefaultFlat();
			};

			apply.Click += (o, e) => {
				var fa = assignment.DataContext as FlatAssignment;
				if ( fa != null && fa.Error == null ) {
					var fac = Project.Assignments.First(a => a.Flat == Flat);
					lock ( fac ) fac.Add(fa);
					Reset();
				} else if ( fa != null && fa.Error != null ) {
					MessageBox.Show(fa.Error, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			};

			delete.Click += (o, e) => {
				var fa = assignments.SelectedItem as FlatAssignment;
				if ( fa != null ) {
					fa.HostingCollection.Remove(fa);
					Reset();
				}
			};

			assignments.SelectedItemChanged += (o, e) => {
				if ( e.NewValue != null ) assignment.DataContext = e.NewValue;
			};

			newAssignment.Click += (o, e) => Reset();
		}

		void AssignDefaultFlat() {
			if ( flat.SelectedItem == null && Project.Property.Flats.Count > 0 ) 
				flat.SelectedItem = Flat = Project.Property.Flats[0];
		}

		public Flat Flat { get; set; }

		Project Project {
			get { return ((ProjectManager)FindResource("ProjectData")).Current; }
		}

		void Reset() {
			assignment.DataContext = new FlatAssignment(Project);
			// assignments.SelectedItem = null;
		}

	}
}
