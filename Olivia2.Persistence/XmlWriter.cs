using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Acly.Olivia2.Model;

namespace Acly.Olivia2.Persistence {

	class XmlWriter : StreamWriter {

		public XmlWriter(string filepath) : base(filepath) {
			WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
		}

		public void Begin(string nodename) {
			WriteLine("<" + nodename + ">");
		}

		public void Begin(object o, IdTable id) {
			var type = o.GetType();
			WriteLine("<" + type.Name + " id=\"" + id[o] + "\">");
			WriteProperties(o, id);
		}

		public void End(string nodename) {
			WriteLine("</" + nodename + ">");
		}

		public void End(object o) {
			WriteLine("</" + o.GetType().Name + ">");
		}

		public void Write(object o, IdTable id) {
			Begin(o, id);
			End(o);
		}
		
		void WriteProperties(object item, IdTable id) {
			foreach ( var property in item.GetType().GetProperties() ) {
				if ( !property.HasAttribute(typeof(SerializeAttribute)) ) continue;
				var value = property.GetValue(item, null);

				if ( property.PropertyType.HasInterface(typeof(INotifyPropertyChanged)) ) {
					// Property type is a data model object, serialise its ID by looking it up
					var idv = value == null ? "null" : id[value].ToString();
					WriteLine("<" + property.Name + ">" + idv + "</" + property.Name + ">");
				} else {
					// Integral data type, write down its string representation
					WriteLine("<" + property.Name + ">" + value + "</" + property.Name + ">");
				}
			}
		}
		
	}

}
