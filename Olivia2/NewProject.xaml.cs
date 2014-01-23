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

	public partial class NewProject : UserControl {

		public NewProject() {
			InitializeComponent();

			ProjectManager projects = null;

			Loaded += (o, e) => {
				projects = (ProjectManager)FindResource("ProjectData");
				project.DataContext = new Project();
			};

			create.Click += (o, e) => {
				var p = project.DataContext as Project;
                if (useTemplate.IsChecked.Value)
                    p.ApplyTemplate((Project)template.SelectedItem);
				if ( p != null && p.Error == null )
					projects.Add(p);
				else if ( p != null )
					MessageBox.Show(p.Error, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
				project.DataContext = new Project();
				MainWindow.ActiveTask = 7;
			};
		}

		public static IEnumerable<string> Months {
			get { for ( int i = 1; i < 13; ++i ) yield return new DateTime(1, i, 1).ToString("MMMM"); }
		}

		public MainWindow MainWindow { get; set; }

	}
}
