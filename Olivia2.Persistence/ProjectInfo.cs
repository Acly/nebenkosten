using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.XPath;

using Acly.Olivia2.Model;

namespace Acly.Olivia2.Persistence {
	public class ProjectInfo {
		public ProjectInfo(string name, string property, string timespan, string filepath) {
			Name = name;
			PropertyName = property;
			Timespan = timespan;
			Filepath = filepath;
		}
		public string Name { get; set; }
		public string PropertyName { get; set; }
		public string Timespan { get; set; }
		public string Filepath { get; set; }
	}

	public class ProjectInfoCollection : ObservableCollection<ProjectInfo> {
		public void Scan(string path, IdTable ids) {
			foreach ( var filepath in Directory.EnumerateFiles(path, "*.xml") ) {
				var xml = new XPathDocument(filepath).CreateNavigator();
				var node = xml.SelectSingleNode("/Project");
				if ( node != null ) {
					var project = new Project();
					var refs = new ReferenceTable();
					Xml.AssignProperties(node, project, refs);
					refs.Update(ids);
					var timespan = project.StartDate.ToShortDateString() + " bis " + project.StartDate.AddYears(1).ToShortDateString();
					Add(new ProjectInfo(project.Name, project.Property.Name, timespan, filepath));
				}
			}
		}
	}
}
