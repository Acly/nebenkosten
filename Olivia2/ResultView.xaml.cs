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
using System.Data;
using System.ComponentModel;
using System.Globalization;

using Acly.Olivia2.Model;

namespace Acly.Olivia2 {
	
	public class ColorConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo info) {

			var cell = values[0] as DataGridCell;
			var row = values[1] as DataRow;
			if ( cell != null && row != null ) {
				var content = row[cell.Column.SortMemberPath];
				double val = 1;
				if ( content != null && double.TryParse(content.ToString(), out val) && val < 0 )
					return Brushes.DarkRed;
			}

			return Brushes.Black;
		}

		public object[] ConvertBack(object value, Type[] types, object param, CultureInfo ci) {
			throw new NotImplementedException();
		}
	}

	public class FontWeightConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo info) {

			var cell = values[0] as DataGridCell;
			var row = values[1] as DataRow;
			var rowheader = row.ItemArray[0].ToString();
			if ( cell != null && row != null ) {
				if ( rowheader == "Kostenpunkt" || rowheader == "Kosten" || rowheader == "Vorrauszahlungen" ||
					rowheader == "Summe" )
					return FontWeights.Bold;
				if ( cell.Column.Header.ToString() == "Cost" && rowheader != "Mietdauer" &&
					rowheader != "Anzahl Mieter" && rowheader != "Mietfläche" )
					return FontWeights.Bold;
			}

			return FontWeights.Normal;
		}

		public object[] ConvertBack(object value, Type[] types, object param, CultureInfo ci) {
			throw new NotImplementedException();
		}
	}

	public partial class ResultView : UserControl {
		public ResultView() {
			InitializeComponent();

			var projects = (ProjectManager)FindResource("ProjectData");
			projects.CurrentProjectChanged += (_, e) => {
				if ( current != null ) {
					current.PropertyChanged -= GenerateTable;
					current.PropertyChanged -= GenerateLesseeList;
				}
				current = projects.Current.Result;
				current.PropertyChanged += GenerateTable;
				current.PropertyChanged += GenerateLesseeList;
			};

			PrintOverviewButton.Click += (_, __) => Printer.Print(projects.Current, null, true);
			PrintSingleButton.Click += (_, __) => Printer.Print(projects.Current, (Lessee)LesseeSelection.SelectedItem);
			PrintAllButton.Click += (_, __) => Printer.Print(projects.Current);
		}

		ResultTable current;

		void GenerateLesseeList(object o, PropertyChangedEventArgs e) {
			var result = (ResultTable)o;
			LesseeSelection.ItemsSource = result.Lessees.Keys;
			if ( result.Lessees.Count > 0 )	LesseeSelection.SelectedIndex = 0;
		}

		void GenerateTable(object o, PropertyChangedEventArgs e) {
			var result = (ResultTable)o;
			var lessees = result.Lessees.OrderBy(l => l.Value.Lessee.Name);

			var table = new DataTable();
			table.Columns.Add("Cost");
			table.Columns.Add("CostMode");
			table.Columns.Add("Total");
			foreach ( var lessee in lessees ) {
				var column = table.Columns.Add(lessee.Value.Lessee.Name);
			}
			table.Columns.Add("Vacancy");
			table.Columns.Add("Landlord");
			table.Columns.Add("Error");
			
			var names = table.NewRow();
			names["Cost"] = "Kostenpunkt";
			names["CostMode"] = "Schlüssel";
			names["Total"] = "Gesamtkosten";
			names["Vacancy"] = "Leerstand";
			names["Landlord"] = "Vermieter";
			names["Error"] = "Fehler";
			table.Rows.Add(names);
			var timespan = table.NewRow();
			var members = table.NewRow();
			var flat = table.NewRow();
			timespan["CostMode"] = "Mietdauer";
			members["CostMode"] = "Anzahl Mieter";
			flat["CostMode"] = "Mietfläche";
			foreach ( var info in result.Lessees.Values ) {
				names[info.Lessee.Name] = info.Lessee.Name;
				timespan[info.Lessee.Name] = info.Duration / 2.0 + " Monate";
				members[info.Lessee.Name] = info.AverageMembers;
				flat[info.Lessee.Name] = info.AverageFlatSize.ToString("####.#") + " m²";
			}
			if ( result.Vacancy.Duration > 0 ) 
				flat["Vacancy"] = result.Vacancy.AverageFlatSize.ToString("####.#") + " m²";
			table.Rows.Add(timespan);
			table.Rows.Add(members);
			table.Rows.Add(flat);
			foreach ( var cost in result.Costs ) {
				var row = table.NewRow();
				row["Cost"] = cost.Name;
				row["CostMode"] = CostModeToString(cost.Mode);
				row["Total"] = cost.Amount.ToString("F");
				foreach ( var lessee in result.Lessees.Keys ) {
					if ( result.Lessees[lessee].Costs.ContainsKey(cost) )
						row[lessee.Name] = result.Lessees[lessee].Costs[cost].ToString("F");
				}
				if ( result.Landlord.Costs.ContainsKey(cost) )
					row["Landlord"] = result.Landlord.Costs[cost].ToString("F");
				if ( result.Vacancy.Costs.ContainsKey(cost) )
					row["Vacancy"] = result.Vacancy.Costs[cost].ToString("F");
				row["Error"] = result.Error.Costs[cost].ToString("F");
				table.Rows.Add(row);
			}
			var costs = table.NewRow();
			var payments = table.NewRow();
			var sums = table.NewRow();
			costs["Cost"] = "Kosten";
			payments["Cost"] = "Vorrauszahlungen";
			sums["Cost"] = "Summe";
			foreach ( var info in result.Lessees.Values ) {
				var totalcosts = info.Costs.Sum(c => c.Value);
				costs[info.Lessee.Name] = totalcosts.ToString("F");
				payments[info.Lessee.Name] = info.AdvancePayment.ToString("F");
				sums[info.Lessee.Name] = (info.AdvancePayment - totalcosts).ToString("F");
			}
			var costssum = result.Landlord.Costs.Sum(c => c.Key.Amount);
			var paymentssum = result.Lessees.Sum(v => v.Value.AdvancePayment);
			var vacancysum = result.Vacancy.Costs.Sum(v => v.Value);
			var landlordsum = result.Landlord.Costs.Sum(c => c.Value);
			costs["Total"] = costssum.ToString("F");
			payments["Total"] = paymentssum.ToString("F");
			sums["Total"] = (paymentssum - costssum).ToString("F");
			costs["Vacancy"] = vacancysum.ToString("F");
			sums["Vacancy"] = (-vacancysum).ToString("F");
			costs["Landlord"] = landlordsum.ToString("F");
			sums["Landlord"] = (-landlordsum).ToString("F");
			sums["Error"] = result.Error.Costs.Sum(ec => ec.Value).ToString("F");
			table.Rows.Add(costs);
			table.Rows.Add(payments);
			table.Rows.Add(sums);

			Table.ItemsSource = table.DefaultView;
			
		 }

		public static string CostModeToString(CostMode c) {
			switch ( c ) {
				case CostMode.External: return "Extern";
				case CostMode.Flat: return "Mietfläche";
				case CostMode.Lessee: return "Mietpartei";
				case CostMode.Member: return "Anzahl Mieter";
			}
			return "Unbekannt";
		}
	}
}
