using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	[AttributeUsage(AttributeTargets.Property)]
	public class SerializeAttribute : Attribute {	}

    public abstract class Base : INotifyPropertyChanged, IDataErrorInfo {

        public event PropertyChangedEventHandler PropertyChanged;
		        
        protected void NotifyPropertyChanged(string prop) {
            if ( PropertyChanged != null ) {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

		public string Error {
			get {
				foreach ( var p in GetType().GetProperties() ) {
					if ( this[p.Name] != null ) return this[p.Name];
				}
				return null;
			}
		}

		public abstract string this[string property] { get; }
    }

}
