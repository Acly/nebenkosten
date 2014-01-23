using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Acly.Olivia2.Model;

namespace Acly.Olivia2 {

    public partial class Flats : UserControl {

		public Flats() {
			InitializeComponent();

			add.Click += (o, e) => {
				lock ( Property.Flats ) Flat = Property.CreateFlat();
			};
		}

		public void OnRemoveItem(object sender, ExecutedRoutedEventArgs e) {
			Property.Flats.Remove(Flat);
		}

		Flat Flat {
			get { return (Flat)flatList.SelectedItem; }
			set { flatList.SelectedItem = value; }
		}

		Property Property {
			get { return (Property)property.SelectedItem; }
		}

		void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e) {
			var tb = sender as TextBox;
			if ( !tb.IsKeyboardFocusWithin && tb.Text == Flat.DefaultName ) {
				e.Handled = true;
				tb.Focus();
			}
		}

    }
}
