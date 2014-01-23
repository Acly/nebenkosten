using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
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

	[ValueConversion(typeof(string), typeof(int))]
	public class TitleConverter : ObservableCollection<string>, IValueConverter {
		public TitleConverter() {
			Add("Herr");
			Add("Frau");
		}

		public object Convert(object o, Type target, object parameter, CultureInfo info) {
			var i = IndexOf(o.ToString());
			return i != -1 ? i : 0;
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo info) {
			var i = (int)o;
			return this[i];
		}
	}


    public partial class Lessees : UserControl {

        public Lessees() {
			InitializeComponent();
		}

		public void OnRemoveItem(object sender, ExecutedRoutedEventArgs e) {
			lock ( LesseeManager ) LesseeManager.Remove(Lessee);
		}

		public void OnNewItem(object sender, ExecutedRoutedEventArgs e) {
			lock ( LesseeManager) Lessee = LesseeManager.Create();
			Lessee.Payments.Add(new AdvancePayment(DateTime.MinValue, 0));
		}

		Lessee Lessee {
			get { return (Lessee)lesseeList.SelectedItem; }
			set { lesseeList.SelectedItem = value; }
		}

		LesseeManager LesseeManager {
			get { return (LesseeManager)FindResource("LesseeData"); }
		}

		void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e) {
			var tb = sender as TextBox;
			if ( !tb.IsKeyboardFocusWithin && tb.Text == Lessee.DefaultName ) {
				e.Handled = true;
				tb.Focus();
			}
		}

    }
}
