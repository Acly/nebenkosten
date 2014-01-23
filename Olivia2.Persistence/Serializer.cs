using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using Acly.Olivia2.Model;

namespace Acly.Olivia2.Persistence {

	public class Serializer {

		public Serializer() {
			IdTable = new IdTable();
			Interval = 10000;
			Timestep = 1000;
			IsRunning = false;
		}

		public int Interval { get; set; }
		public int Timestep { get; set; }
		public DateTime LastSave { get; protected set; }
		public bool IsRunning { get; protected set; }

		private object UnsavedChangesChanging = new object();

		// Run this as a BackgroundWorker method. Will report 0 whenever it starts saving and 1 whenever it is
		// done saving through the ProgressChanged event.
		public void SavePeriodically(BackgroundWorker worker) {
			IsRunning = true;
			LastSave = DateTime.Now;
			worker.WorkerReportsProgress = true;
			try {
				while ( !ShutdownRequest ) {
					var t = (DateTime.Now - LastSave).TotalMilliseconds;
					if ( (SaveRequest || t >= Interval) && UnsavedChanges ) {
						bool props, lessees, projects;
						lock ( UnsavedChangesChanging ) {
							SaveRequest = false;			// This section allows changes to be registered
							props = PropertiesChanged;		// while the writing commences. While they may be picked
							lessees = LesseesChanged;		// up on the current write, this makes sure there is another
							projects = ProjectsChanged;		// write coming up in case they aren't
							PropertiesChanged = LesseesChanged = ProjectsChanged = false;
						}
						worker.ReportProgress(0);
						if ( props ) Xml.Write(PropertyManagerReference, PropertyFilepath, IdTable);
						if ( lessees ) Xml.Write(LesseeManagerReference, LesseeFilepath, IdTable);
						if ( projects ) Xml.Write(ProjectManagerReference, ProjectPath, IdTable);
						LastSave = DateTime.Now;
						worker.ReportProgress(1);
					}
					Thread.Sleep(Timestep);
				}
			} finally {
				IsRunning = false;
			}
		}
		
		private bool SaveRequest = false;
		private bool ShutdownRequest = false;

		public void SaveNow() { SaveRequest = true; }

		public void Shutdown() {
			ShutdownRequest = true;			
		}

		private bool _PropertiesChanged;
		private bool _LesseesChanged;
		private bool _ProjectsChanged;

		public bool PropertiesChanged {
			get { return _PropertiesChanged; }
			set { lock ( UnsavedChangesChanging ) _PropertiesChanged = value; }
		}
		public bool LesseesChanged { 
			get { return _LesseesChanged; }
			set { lock ( UnsavedChangesChanging ) _LesseesChanged = value; }
		}
		public bool ProjectsChanged {
			get { return _ProjectsChanged; }
			set { lock ( UnsavedChangesChanging ) _ProjectsChanged = value; }
		}
		public bool UnsavedChanges { 
			get { return PropertiesChanged || LesseesChanged || ProjectsChanged; } 
		}

		public PropertyManager PropertyManagerReference { get; set; }	// These are changed from
		public LesseeManager LesseeManagerReference { get; set; }		// the outside. Local copies
		public ProjectManager ProjectManagerReference { get; set; }		// will be made while saving

		public IdTable IdTable { get; protected set; } 

		public string PropertyFilepath { get; set; }
		public string LesseeFilepath { get; set; }
		public string ProjectPath { get; set; }

	}

}
