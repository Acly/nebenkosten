using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Acly.Olivia2.Model;

namespace Acly.Olivia2.Persistence {

public class IdTable : Dictionary<object, Guid> {

	public new Guid this[object key] {
		get {
			if ( !ContainsKey(key) ) base[key] = Guid.NewGuid();
			return base[key];
		}
		set { base[key] = value; }
	}

}

}
