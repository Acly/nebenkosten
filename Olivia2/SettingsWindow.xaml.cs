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
using Acly.Olivia2.Persistence;

namespace Acly.Olivia2 {
	public partial class SettingsWindow : UserControl {
		public SettingsWindow() {
			InitializeComponent();
			LoadData();

			ApplyButton.Click += (o, e) => {
				Settings.Save();
			};
		}

		void LoadData() {
			var refs = new ReferenceTable();
			var lessees = (LesseeManager)FindResource("LesseeData");
			var properties = (PropertyManager)FindResource("PropertyData");
			var projects = (ProjectManager)FindResource("ProjectData");
			var projectinfo = (ProjectInfoCollection)FindResource("ProjectInfo");
			var serializer = (Serializer)FindResource("Serializer");
			Xml.Read(Settings.PropertyFilepath, properties, refs, serializer.IdTable);
			Xml.Read(Settings.LesseeFilepath, lessees, refs, serializer.IdTable);
			projectinfo.Scan(Settings.ProjectPath, serializer.IdTable);
			refs.Update(serializer.IdTable);
		}

		public Settings Settings {
			get {
				return (Settings)FindResource("Settings");
			}
		}
	}
}
