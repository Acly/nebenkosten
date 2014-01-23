using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
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

	public class PaymentDateConverter : IValueConverter {

		public const string StartValueString = "Startwert";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var date = (DateTime)value;
			if ( date == DateTime.MinValue ) {
				return StartValueString;
			}
			string result = parameter == null ? "" : parameter.ToString();
			return result + date.ToString("dd. MMMM yyyy");
		}

		public object ConvertBack(object value, Type type, object parameter, CultureInfo culture) {
			if ( value.ToString() == StartValueString ) {
				return DateTime.MinValue;
			}
			return DateTime.Parse(value.ToString());
		}
	}

	[ValueConversion(typeof(DateTime), typeof(Visibility))]
	public class IsStartDateConverter : IValueConverter {
		public object Convert(object date, Type t, object p, CultureInfo ci) {
			return (DateTime)date == DateTime.MinValue ? Visibility.Hidden : Visibility.Visible;
		}
		public object ConvertBack(object v, Type t, object p, CultureInfo ci) {
			throw new NotImplementedException();
		}
	}

	public partial class AdvancePayments : UserControl {

		public AdvancePayments() {
			InitializeComponent();			
		}

		public void OnNewPayment(object sender, ExecutedRoutedEventArgs e) {
			lock ( Lessee.Payments ) Lessee.Payments.Add(new AdvancePayment(DateTime.Now, Lessee.Payments.Current));
		}

		public Lessee Lessee {
			get { return (Lessee)lesseeList.SelectedItem; }
		}
	}
}
