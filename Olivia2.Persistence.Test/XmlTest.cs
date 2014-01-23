using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Acly.Olivia2.Persistence;
using Acly.Olivia2.Model;

namespace Olivia2.Persistence.Test {

	[TestClass]
	public class XmlTest {
		public XmlTest() {}

		[TestMethod]
		public void TestSerialise() {
			var id = new IdTable();
			var props = new PropertyManager();
			var lessees = new LesseeManager();
			Xml.Write(props, "Properties.xml", id);
			Xml.Write(lessees, "Lessees.xml", id);
			var projects = new ProjectManager();
			projects.Add(new Project());
			projects.Current.Property = props[0];
			projects.Current.CreateCost();
			var fac = projects.Current.Assignments.First(a => a.Flat == props[0].Flats[0]);
			var assignment = new FlatAssignment(projects.Current);
            assignment.Start = DateTime.Now;
            assignment.End = DateTime.Now.AddMonths(3);
			assignment.Lessee = lessees[0];
			fac.Add(assignment);
			Xml.Write(projects, "", id);
		}

		[TestMethod]
		public void TestDeserialise() {
			var id = new IdTable();
			var refs = new ReferenceTable();
			var props = new PropertyManager();
			var lessees = new LesseeManager();
			var projects = new ProjectManager();
			Xml.Read("D:\\Projects\\Olivia2\\Olivia2.Persistence.Test\\Properties.xml", props, refs, id);
			Xml.Read("D:\\Projects\\Olivia2\\Olivia2.Persistence.Test\\Lessees.xml", lessees, refs, id);
			Xml.Read("D:\\Projects\\Olivia2\\Olivia2.Persistence.Test\\Abrechnung 2011.xml", projects, refs, id);
			refs.Update(id);
			Equals(props[0].Name, "Haus1");
			Equals(props[0].Flats[0].Name, "Wohnung A");
			Equals(projects.Current.Assignments[0].Flat, props[0].Flats[0]);
		}
	}
}
