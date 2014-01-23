using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Acly.Olivia2.Persistence;

namespace Acly.Olivia2 {

    public partial class MainWindow : Window {

	     public MainWindow() {
			InitializeComponent();
			var lessees = (LesseeManager)FindResource("LesseeData");
			var properties = (PropertyManager)FindResource("PropertyData");
			var projects = (ProjectManager)FindResource("ProjectData");
			var projectinfo = (ProjectInfoCollection)FindResource("ProjectInfo");

			if ( properties.Count == 0 ) {
				taskBar.Loaded += (o, _) => taskBar.SelectedIndex = 3;
				NewProjectTask.IsEnabled = FlatsTask.IsEnabled = false;
			}
			properties.CollectionChanged += (p, _) => {
				NewProjectTask.IsEnabled = FlatsTask.IsEnabled = properties.Count > 0;
			};

			OpenProjectTask.IsEnabled = projectinfo.Count > 0;
			projectinfo.CollectionChanged += (pi, _) => OpenProjectTask.IsEnabled = projectinfo.Count > 0;
			
			taskBar.DataContextChanged += (tb, e) => {
				if ( e.NewValue != null ) {
					projectHeader.Visibility = Visibility.Visible;
					var flats = projects.Current.Property.Flats;
					FlatAssignmentsTask.IsEnabled = flats.Count > 0;
					flats.CollectionChanged += (f, _) => FlatAssignmentsTask.IsEnabled = flats.Count > 0;
				}
			};

			 
			var settings = (Settings)FindResource("Settings");
			var serializer = (Serializer)FindResource("Serializer");
			serializer.PropertyManagerReference = properties;
			serializer.LesseeManagerReference = lessees;
			serializer.ProjectManagerReference = projects;
			serializer.PropertyFilepath = settings.PropertyFilepath;
			serializer.LesseeFilepath = settings.LesseeFilepath;
			serializer.ProjectPath = settings.ProjectPath;

			save.Click += (o, e) => serializer.SaveNow();

			Loaded += (o, _) => {
				EventHandler statuschange = (obj, args) => {
					status.Content = "Letzte Änderung um " + DateTime.Now.ToShortTimeString();
					save.IsEnabled = true;
					save.Content = "Speichern";
					if ( projects.Current != null ) projects.Current.Result.Calculate();
				};
				var propobs = new Observer<Acly.Olivia2.Model.Property>("Properties", properties);
				propobs.Changed += statuschange;
				propobs.Changed += (p, e) => serializer.PropertiesChanged = true;
				var lesseeobs = new Observer<Lessee>("Lessees", lessees);
				lesseeobs.Changed += statuschange;
				lesseeobs.Changed += (l, e) => serializer.LesseesChanged = true;
				var projectobs = new Observer<Project>("Project", projects);
				projectobs.Changed += statuschange;
				projectobs.Changed += (p, e) => serializer.ProjectsChanged = true;

				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += (w, a) => serializer.SavePeriodically(w as BackgroundWorker);
				worker.RunWorkerCompleted += (w, a) => {
					if ( a.Error != null ) MessageBox.Show("Serialization Worker died. " + a.Error.Message, "Error");
				};
				worker.ProgressChanged += (w, progress) => {
					if ( progress.ProgressPercentage == 0 ) {
						save.IsEnabled = false;
						save.Content = "Speichert...";
					} else if ( !serializer.UnsavedChanges ) {// could be new changes while it was saving...
						status.Content = "Daten Gespeichert um " + DateTime.Now.ToShortTimeString();
						save.Content = "Gespeichert";
					}
				};
				worker.RunWorkerAsync();
			};

			NewProjectWindow.MainWindow = OpenProjectWindow.MainWindow = this;			 
        }

		 public int ActiveTask {
			 get { return taskBar.SelectedIndex; }
			 set { taskBar.SelectedIndex = value; }
		 }
    }
}
