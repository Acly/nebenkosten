using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Globalization;

using Acly.Olivia2.Model;

namespace Acly.Olivia2 {

	[ValueConversion(typeof(CostMode), typeof(int))]
	public class CostModeConverter : ObservableCollection<string>, IValueConverter {

		public CostModeConverter() {
			Add("Mietpartei");
			Add("Anzahl Mieter");
			Add("Mietfläche");
			Add("Manuelle Eingabe");			
		}

		public object Convert(object o, Type target, object parameter, CultureInfo info) {
			int v = (int)o;
			return v;
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo info) {
			int v = (int)o;
			return Enum.GetValues(typeof(CostMode)).GetValue(v);
		}
	}

	[ValueConversion(typeof(CostMode), typeof(string))]
	public class CostModeToStringConverter : IValueConverter {
		public object Convert(object o, Type target, object param, CultureInfo info) {
			return new CostModeConverter()[(int)o];
		}
		public object ConvertBack(object o, Type t, object p, CultureInfo ci) {
			throw new NotImplementedException();
		}
	}

	public partial class Costs : UserControl {

		public Costs() {
			InitializeComponent();

			add.Click += (o, e) => {
				lock ( Project.Costs ) costList.SelectedItem = Project.CreateCost();
			};
		}

		Project Project {
			get { return ((ProjectManager)FindResource("ProjectData")).Current; }
		}

		void OnRemoveCost(object sender, ExecutedRoutedEventArgs e) {
			Project.Costs.Remove((Cost)costList.SelectedItem);
		}

		void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e) {
			var tb = sender as TextBox;
			if ( !tb.IsKeyboardFocusWithin && tb.Text == Cost.DefaultName ) {
				e.Handled = true;
				tb.Focus();
			}
		}
	}
}
