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

using Acly.Olivia2.Persistence;
using Acly.Olivia2.Model;

namespace Acly.Olivia2 {
	public partial class OpenProject : UserControl {

		public static readonly DependencyProperty ProjectSelectedProperty
			= DependencyProperty.Register("ProjectSelected", typeof(bool), typeof(OpenProject));

		public OpenProject() {
			InitializeComponent();

			SetValue(ProjectSelectedProperty, false);
			ProjectList.SelectionChanged += (o, e) => {
				SetValue(ProjectSelectedProperty, ProjectList.SelectedItems != null);
			};

			OpenButton.Click += (_, __) => {
				var refs = new ReferenceTable();
				var filepath = ((ProjectInfo)ProjectList.SelectedItem).Filepath;
				var projects = (ProjectManager)FindResource("ProjectData");
				var ids = ((Serializer)FindResource("Serializer")).IdTable;
				Xml.Read(filepath, projects, refs, ids);
				refs.Update(ids);
				projects.Current.Result.Calculate();
				MainWindow.ActiveTask = 7;
			};
		}

		public MainWindow MainWindow { get; set; }
	}
}
