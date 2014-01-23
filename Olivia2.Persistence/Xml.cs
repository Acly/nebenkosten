using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Acly.Olivia2.Model;

namespace Acly.Olivia2.Persistence {

	public static class Xml {

		public static void Write(PropertyManager properties, string filepath, IdTable ids) {
			var f = new XmlWriter(filepath);
			f.Begin("Properties");
			var list = new Olivia2.Model.Property[properties.Count];
			lock ( properties ) properties.CopyTo(list, 0);
			foreach (var p in list) {
				f.Begin(p, ids);
				f.Begin("Flats");
				var flatlist = new Flat[p.Flats.Count];
				lock ( p.Flats ) p.Flats.CopyTo(flatlist, 0);
				foreach ( var flat in flatlist ) f.Write(flat, ids);
				f.End("Flats");
				f.End(p);
			}
			f.End("Properties");
			f.Close();
		}

		public static void Write(LesseeManager lessees, string filepath, IdTable ids) {
			var f = new XmlWriter(filepath);
			f.Begin("Lessees");
			var list = new Lessee[lessees.Count];
			lock ( lessees ) lessees.CopyTo(list, 0);
			foreach ( var l in list ) {
				f.Begin(l, ids);
				f.Begin("Payments");
				var paymentlist = new AdvancePayment[l.Payments.Count];
				lock ( l.Payments ) l.Payments.CopyTo(paymentlist, 0);
				foreach ( var p in paymentlist ) f.Write(p, ids);
				f.End("Payments");
				f.End(l);
			}
			f.End("Lessees");
			f.Close();
		}

		public static void Write(ProjectManager projects, string path, IdTable ids) {
			foreach ( var p in projects ) {
				if ( p == Project.Empty ) continue;
				var f = new XmlWriter(path + p.Name + ".xml");
				f.Begin(p, ids);
				f.Begin("Assignments");
				var faclist = new FlatAssignmentCollection[p.Assignments.Count];
				lock ( p.Assignments ) p.Assignments.CopyTo(faclist, 0);
				foreach ( var fac in faclist ) {
					f.Begin(fac, ids);
					var falist = new FlatAssignment[fac.Count];
					lock ( fac ) fac.CopyTo(falist, 0);
					foreach ( var fa in falist ) f.Write(fa, ids);
					f.End(fac);
				}
				f.End("Assignments");
				f.Begin("Costs");
				var costlist = new Cost[p.Costs.Count];
				lock ( p.Costs ) p.Costs.CopyTo(costlist, 0);
				foreach ( var c in costlist ) {
					f.Begin(c, ids);
					f.Begin("Options");
					var optionslist = new CostOptions[c.Options.Count];
					lock ( c.Options ) c.Options.CopyTo(optionslist, 0);
					foreach ( var o in optionslist ) f.Write(o, ids);
					f.End("Options");
					f.End(c);
				}
				f.End("Costs");
				f.End(p);
				f.Close();
			}
		}

		public static void Read(string filepath, PropertyManager properties, ReferenceTable references, IdTable ids) {
			var xml = new XPathDocument(filepath).CreateNavigator();
			var pi = xml.Select("/Properties/Property");
			while ( pi.MoveNext() ) {
				var pnode = pi.Current;
				var p = properties.Create();
				ids[p] = new Guid(pnode.GetAttribute("id", ""));
				AssignProperties(pnode, p, references);
				var fi = pnode.Select("Flats/Flat");
				while ( fi.MoveNext() ) {
					var fnode = fi.Current;
					var f = p.CreateFlat();
					ids[f] = new Guid(fnode.GetAttribute("id", ""));
					AssignProperties(fnode, f, references);
				}
			}
		}

		public static void Read(string filepath, LesseeManager lessees, ReferenceTable references, IdTable ids) {
			var xml = new XPathDocument(filepath).CreateNavigator();
			var li = xml.Select("/Lessees/Lessee");
            var list = new List<Lessee>();
			while ( li.MoveNext() ) {
				var lnode = li.Current;
				var l = new Lessee();
				ids[l] = new Guid(lnode.GetAttribute("id", ""));
				AssignProperties(lnode, l, references);
				var pi = lnode.Select("Payments/AdvancePayment");
				while ( pi.MoveNext() ) {
					var pnode = pi.Current;
					var p = new AdvancePayment();
					ids[p] = new Guid(pnode.GetAttribute("id", ""));
					AssignProperties(pnode, p, references);
					l.Payments.Add(p);
				}
                list.Add(l);
			}
            foreach (var i in list.OrderBy(item => item.Name))
                lessees.Add(i);
		}

		public static void Read(string filepath, ProjectManager projects, ReferenceTable references, IdTable ids) {
			var xml = new XPathDocument(filepath).CreateNavigator();
			var project = new Project();
			var pnode = xml.SelectSingleNode("/Project");
			ids[project] = new Guid(pnode.GetAttribute("id", ""));
			AssignProperties(pnode, project, references);
			references.Update(ids);// force Project.Property assignment
			var aci = pnode.Select("Assignments/FlatAssignmentCollection");
			while ( aci.MoveNext() ) {
				var acnode = aci.Current;
				var flatid = acnode.SelectSingleNode("Flat").Value;
				var collection = project.Assignments.First(ac => ids[ac.Flat].ToString() == flatid);
				ids[collection] = new Guid(acnode.GetAttribute("id", ""));
				AssignProperties(acnode, collection, references);
				var ai = acnode.Select("FlatAssignment");
				while ( ai.MoveNext() ) {
					var anode = ai.Current;
					var a = new FlatAssignment(project);
					ids[a] = new Guid(anode.GetAttribute("id", ""));
					AssignProperties(anode, a, references);
					collection.Add(a);
				}
			}
			references.Update(ids);// force Assignments for CostOptions generation
			var ci = pnode.Select("Costs/Cost");
			while ( ci.MoveNext() ) {
				var cnode = ci.Current;
				var c = project.CreateCost();
				ids[c] = new Guid(cnode.GetAttribute("id", ""));
				AssignProperties(cnode, c, references);
				var oi = cnode.Select("Options/CostOptions");
				while ( oi.MoveNext() ) {
					var onode = oi.Current;
					var lesseeid = onode.SelectSingleNode("Lessee").Value;
					var option = c.Options.First(o => ids[o.Lessee].ToString() == lesseeid);
					ids[option] = new Guid(onode.GetAttribute("id", ""));
					AssignProperties(onode, option, references);
				}
			}
			projects.Add(project);
		}

		public static void AssignProperties(XPathNavigator node, object o, ReferenceTable references) {
			//var properties = o.GetType().GetProperties();
			foreach ( var property in o.GetType().GetProperties() ) {
				if ( !property.HasAttribute(typeof(SerializeAttribute)) ) continue;
				var pnode = node.SelectSingleNode(property.Name);
				if ( pnode == null ) continue;// use default value
				var val = pnode.Value;
				if ( property.PropertyType.IsEnum ) {
					property.SetValue(o, Enum.Parse(property.PropertyType, val), null);
				} else if ( property.PropertyType.IsValueType ) {
					var parse = property.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
					var value = parse.Invoke(null, new object[] { val });
					property.SetValue(o, value, null);
				} else if ( property.PropertyType.Name == "String" ) {
					property.SetValue(o, val, null);
				} else if ( val == "null" ) {
					property.SetValue(o, null, null);
				} else if ( property.CanWrite ) {
					references.Add(new Guid(val), property, o);
				}
			}
		}

	}

}
