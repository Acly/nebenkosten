using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class LesseeManager : ObservableCollection<Lessee> {
        
		public Lessee Create() {
			var l = new Lessee();
			Add(l);
			return l;
		}

	}

}
