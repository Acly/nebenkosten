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

    public partial class Buildings : UserControl {

        public Buildings() {
            InitializeComponent();

			add.Click += (o, e) => {
				lock ( PropertyManager ) Property = PropertyManager.Create();
			};
        }

		public void OnRemoveItem(object sender, ExecutedRoutedEventArgs e) {
			PropertyManager.Remove(Property);
		}

		Property Property {
			get { return (Property)propertyList.SelectedItem; }
			set { 
				propertyList.SelectedItem = value;
				propertyList.Focus();
			}
		}

		PropertyManager PropertyManager {
			get { return (PropertyManager)FindResource("PropertyData"); }
		}

		void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e) {
			var tb = sender as TextBox;
			if ( !tb.IsKeyboardFocusWithin && tb.Text == Property.DefaultName ) {
				e.Handled = true;
				tb.Focus();
			}
		}
    }
}
