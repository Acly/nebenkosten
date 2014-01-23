using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;

using Acly.Olivia2.Persistence;
using Acly.Olivia2.Model;

namespace Acly.Olivia2 {

	public class Settings {

		public Settings() {
			if ( File.Exists("Settings.xml") ) LoadFile("Settings.xml");
			else LoadDefaults();
			CheckFiles();
		}

		static string Val(XPathNavigator x, string p) { return x.SelectSingleNode(p).Value; }

		void LoadFile(string filepath) {
			var xml = new XPathDocument(filepath).CreateNavigator();
			ProjectPath = Val(xml, "/Settings/ProjectPath");
			PropertyFilepath = Val(xml, "/Settings/PropertyFilepath");
			LesseeFilepath = Val(xml, "/Settings/LesseeFilepath");
			AutoSave = Boolean.Parse(Val(xml, "/Settings/AutoSave"));
		}

		void LoadDefaults() {
			ProjectPath = "Data/";
			PropertyFilepath = "Data/Properties.xml";
			LesseeFilepath = "Data/Lessees.xml";
			AutoSave = true;
			Save();
		}

		void CheckFiles() {
			try {
				if ( !Directory.Exists(ProjectPath) )
					Directory.CreateDirectory(ProjectPath);
				if ( !File.Exists(PropertyFilepath) )
					Xml.Write(new PropertyManager(), PropertyFilepath, new IdTable());
				if ( !File.Exists(LesseeFilepath) )
					Xml.Write(new LesseeManager(), LesseeFilepath, new IdTable());
			} catch ( Exception ) {
				throw new Exception("Settings.xml broken, delete it and rerun the application to reset it.");
			}
		}

		public void Save() {
			var f = new StreamWriter("Settings.xml");
			f.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			f.WriteLine("<Settings>");
			f.WriteLine("	<ProjectPath>" + ProjectPath + "</ProjectPath>");
			f.WriteLine("	<PropertyFilepath>" + PropertyFilepath + "</PropertyFilepath>");
			f.WriteLine("	<LesseeFilepath>" + LesseeFilepath + "</LesseeFilepath>");
			f.WriteLine("	<AutoSave>" + AutoSave + "</AutoSave>");
			f.WriteLine("</Settings>");
			f.Close();
		}

		public string ProjectPath { get; set; }
		public string PropertyFilepath { get; set; }
		public string LesseeFilepath { get; set; }
		public bool AutoSave { get; set; }

	}

}
