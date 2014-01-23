using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Acly.Olivia2.Model;

namespace Acly.Olivia2.Persistence {

	public class Observer<T> where T : INotifyPropertyChanged {

		public event EventHandler Changed;

		public Observer(string name, ObservableCollection<T> target) {
			Name = name;
			Collection = target;
			foreach ( var item in Collection ) 
				item.PropertyChanged += OnChange;
			Collection.CollectionChanged += (o, e) => {
				if ( e.Action == NotifyCollectionChangedAction.Add ) {
					foreach ( T item in e.NewItems )
						item.PropertyChanged += OnChange;
				}
				OnChange(o, e);
			};
		}

		protected virtual void OnChange(object sender, EventArgs e) {
			if ( Changed != null ) Changed(this, e);
		}
				
		public string Name { set; get; }
		public ObservableCollection<T> Collection { get; set; }
	}

}
