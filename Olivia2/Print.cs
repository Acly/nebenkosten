//--------------------------------------------------------------------------------//
// Print.cs - Print Output
//
// Functions to print the review table on paper
//--------------------------------------------------------------------------------//

using System;
using System.Printing;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.IO.Packaging;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;

using Acly.Olivia2.Model;

namespace Acly.Olivia2 {

    public class Printer {

        // Print - brings up the print dialogue and starts printing
        public static bool Print(Project project, Lessee lessee = null, bool overview = false) {
			PrintDocumentImageableArea area = null;
			var writer = PrintQueue.CreateXpsDocumentWriter(ref area);
			var document = CreateDocument(project, lessee, overview);
			document.PageWidth = area.MediaSizeWidth;
			document.PageHeight = area.MediaSizeHeight;
			document.ColumnWidth = document.PageWidth;
			document.PagePadding = new Thickness(40);
			IDocumentPaginatorSource source = document;
			try {
				writer.WriteAsync(source.DocumentPaginator);
			} catch ( Exception ) {
				return false;
			}
            return true;
        }
		
 
		static FlowDocument CreateDocument(Project project, Lessee lessee, bool overview) {
			var doc = new FlowDocument();

			if ( lessee != null && !overview ) {// Print 1 page for a single lessee
				WritePage(doc, new Lessee[] { lessee }, project, false, true);
			} else if ( lessee == null && overview ) {// Partition lessees 8 at a time
				int i = 0;
				var list = new List<Lessee>();
				foreach ( Lessee l in project.Result.Lessees.Keys.OrderBy(l => l.Name) ) {
					list.Add(l);
					if ( ++i == 8 ) {
						WritePage(doc, list, project, true, false);
						i = 0;
						list = new List<Lessee>();
					}
				}
				if ( i > 6 ) {
					WritePage(doc, list.Take(6).ToList(), project, true, false);
					WritePage(doc, new Lessee[] { list[list.Count - 1] }, project, true, true);
				} else WritePage(doc, list, project, true, true);
			} else if ( lessee == null && !overview ) {// Print 1 page per lessee for all lessees
				foreach ( Lessee l in project.Result.Lessees.Keys ) {
					WritePage(doc, new Lessee[] { l }, project, false, false);
				}
			} else {
				throw new ArgumentException("Arguments invalid");
			}

			return doc;
		}


		static void WritePage(FlowDocument doc, IList<Lessee> lessees, Project project, bool overview, bool lastpage) {
			var result = project.Result;
			double scale = overview ? 0.8 : 1;
			var llv = overview && lastpage;// print landlord and vacancy columns

			// create some useful fonts and brushes
			var heading = CreateStyle(20.0 * scale, FontWeights.Bold);
			var big = CreateStyle(16 * scale, FontWeights.Bold);
			var bold = CreateStyle(11 * scale, FontWeights.Bold);
			var normal = CreateStyle(11 * scale);
			var small = CreateStyle(9 * scale);
			var red = CreateStyle(11 * scale, FontWeights.Bold, Brushes.Red);

			// Nebenkostenabrechnung <jahr>/<jahr+1>
			// <name mietpartei/Übersicht>
			// Vom <abrechnungszeitraum anfang> bis zum <abrechnungszeitraum ende>
			var r = doc.Blocks;
			r.Add(Text("Nebenkostenabrechnung " + project.StartYear + " / " + project.EndDate.Year, heading));
			if ( r.Count != 1 ) r.LastBlock.BreakPageBefore = true;// unless this is the first page, insert a page break
			r.Add(Text(overview ? "Übersicht" : lessees[0].Name, big));
			var start = overview ? project.StartDate : result.Lessees[lessees[0]].StartDate;
			var end = overview ? project.EndDate : result.Lessees[lessees[0]].EndDate;
			r.Add(Text("Zeitraum: " + start.ToShortDateString() + " bis " + end.ToShortDateString(), bold));
			

			/* Kostenart            Schlüssel       Gesamtkosten                    _______
			 *                      Mieter                              Mieter i
			 *                      Personen/Firmen                     i.count
			 *                      Mietfläche                          i.room.size
			 *                      Mietdauer                           n Monate    _______
			 * Kostenpunkt j        j.type          j.value             k(i, j)     _______    
			 * Nebenkosten gesamt   in Euro         K(false)            K(i)
			 * Vorauszahlungen      in Euro         P()                 P(i)
			 * Nachzahlung/Erstattung in Euro       Z(false)            Z(i)
			 *\____________________|_______________|______________|_/ \_____________/
			 *       same for single leccee and overview                single first
			 */
			var t = new Table();
			t.CellSpacing = 0;
			for ( var i = 0; i < 3 + lessees.Count; ++i ) {
				t.Columns.Add(new TableColumn() { Width = i == 0 ? new GridLength(160 * scale) : new GridLength(110 * scale) });
			}
			if ( overview && lastpage ) {
				t.Columns.Add(new TableColumn() { Width = new GridLength(110 * scale) });
				t.Columns.Add(new TableColumn() { Width = new GridLength(110 * scale) });
			}

			var headgroup = new TableRowGroup();
			headgroup.Rows.Add(Row(
				Text("Kostenart", bold), 
				Text("Schlüssel", bold), 
				Text("Gesamtkosten", bold),
				lessees, l => Text(), 
				llv ? Text() : null, 
				llv ? Text() : null, true));
			headgroup.Rows.Add(Row(
				Text(), 
				Text("Mieter", normal), 
				Text(),
				lessees, l => Text(l.Name, normal),
				llv ? Text("Leerstand", normal) : null, 
				llv ? Text("Vermieter", normal) : null));
			headgroup.Rows.Add(Row(
				Text(), 
				Text("Personen", normal), 
				Text(),
				lessees, l => Text(l.Members.ToString(), normal),
				llv ? Text() : null, 
				llv ? Text() : null));
			headgroup.Rows.Add(Row(
				Text(), 
				Text("Mietfläche", normal), 
				Text(),
				lessees, l => Text(result.Lessees[l].AverageFlatSize.ToString("F") + " m²", normal),
				llv ? Text(result.Vacancy.AverageFlatSize.ToString("F") + "~ m²", normal) : null, 
				llv ? Text() : null));
			headgroup.Rows.Add(Row(
				Text(), 
				Text("Mietdauer", normal), 
				Text(),
				lessees, l => Text(result.Lessees[l].Months + " Monate", normal),
				llv ? Text(result.Vacancy.Months + " Monate", normal) : null, 
				llv ? Text() : null, true));
			t.RowGroups.Add(headgroup);

			var costgroup = new TableRowGroup();
			for ( var i = 0; i < project.Costs.Count; ++i ) {
				var cost = project.Costs[i];
				if ( !overview && !result.Lessees[lessees[0]].Costs.ContainsKey(cost) ) continue;
				costgroup.Rows.Add(Row(
									Text(cost.Name, normal), 
									Text(ResultView.CostModeToString(cost.Mode), normal),
					/* Total */		Text(cost.Amount.ToString("F"), normal), lessees,
					/* Lessee */	l => result.Lessees[l].Costs.ContainsKey(cost) ? Text(result.Lessees[l].Costs[cost].ToString("F"), normal) : Text(),
					/* Vacancy */	llv ? (result.Vacancy.Costs.ContainsKey(cost) ? Text(result.Vacancy.Costs[cost].ToString("F"), normal) : Text()) : null,
					/* Landlord */	llv && result.Landlord.Costs.ContainsKey(cost) ? Text(result.Landlord.Costs[cost].ToString("F"), normal) : null,
					/* HasBorder */	i == project.Costs.Count - 1));
			}
			t.RowGroups.Add(costgroup);

			var sumgroup = new TableRowGroup();
			double costssum = project.Costs.Sum(c => c.Amount);
			double paymentsum = result.Lessees.Sum(p => p.Value.AdvancePayment);
			double vacancysum = result.Vacancy.Costs.Sum(v => v.Value);
			double landlordsum = result.Landlord.Costs.Sum(l => l.Value);
			sumgroup.Rows.Add(Row(
				Text("Nebenkosten gesamt", bold), 
				Text("in Euro", normal),
				Text(costssum.ToString("F"), bold), 
				lessees, l => Text(result.Lessees[l].Costs.Sum(p => p.Value).ToString("F"), bold),
				llv ? Text(vacancysum.ToString("F"), bold) : null,
				llv ? Text(landlordsum.ToString("F"), bold) : null));
			sumgroup.Rows.Add(Row(
				Text("Vorrauszahlungen", bold), 
				Text("in Euro", normal),
				Text(paymentsum.ToString("F"), bold), 
				lessees, l => Text(result.Lessees[l].AdvancePayment.ToString("F"), bold),
				llv ? Text() : null, 
				llv ? Text() : null));
			sumgroup.Rows.Add(Row(
				Text("Nachzahlung/Erstattung", bold), 
				Text("in Euro", normal),
				Text((paymentsum - costssum).ToString("F"), bold), 
				lessees, l => {
					var sum = result.Lessees[l].AdvancePayment - result.Lessees[l].Costs.Sum(p => p.Value);
					return Text(sum.ToString("F"), sum < 0 ? red : bold);
				},
				llv ? Text((-vacancysum).ToString("F"), red) : null,
				llv ? Text((-landlordsum).ToString("F"), red) : null));
			t.RowGroups.Add(sumgroup);

			r.Add(t);			
		}

		static Style CreateStyle(double fontsize, FontWeight fontweight, Brush brush) {
			var style = new Style(typeof(Paragraph));
			style.Setters.Add(new Setter(Paragraph.FontSizeProperty, fontsize));
			style.Setters.Add(new Setter(Paragraph.FontWeightProperty, fontweight));
			style.Setters.Add(new Setter(Paragraph.ForegroundProperty, brush));
			style.Setters.Add(new Setter(Paragraph.FontFamilyProperty, new FontFamily("Verdana")));
			return style;
		}
		static Style CreateStyle(double fontsize, FontWeight fontweight) {
			return CreateStyle(fontsize, fontweight, Brushes.Black);
		}
		static Style CreateStyle(double fontsize) {
			return CreateStyle(fontsize, FontWeights.Normal);
		}

		static Paragraph Text(string str, Style style) {
			return new Paragraph(new Run(str)) { Style = style, Margin = new Thickness(2, 4, 2, 4) };
		}
		static Paragraph Text() {
			return new Paragraph(new Run());
		}

		static TableRow Row(Paragraph cost, Paragraph mode, Paragraph total, IEnumerable<Lessee> lessees, 
			Func<Lessee, Paragraph> f, Paragraph vacancy, Paragraph landlord, bool border = false) {
			var b = border ? 1 : 0;
			var row = new TableRow();
			row.Cells.Add(new TableCell(cost) { BorderThickness = new Thickness(0, 0, 1, b), BorderBrush = Brushes.Black });
			row.Cells.Add(new TableCell(mode) { BorderThickness = new Thickness(0, 0, 1, b), BorderBrush = Brushes.Black });
			row.Cells.Add(new TableCell(total) { BorderThickness = new Thickness(0, 0, 1, b), BorderBrush = Brushes.Black });
			foreach ( var l in lessees ) {
				row.Cells.Add(new TableCell(f(l)) { BorderThickness = new Thickness(0, 0, 0, b), BorderBrush = Brushes.Black });
			}
			if ( vacancy != null ) row.Cells.Add(new TableCell(vacancy) { BorderThickness = new Thickness(1, 0, 0, b), BorderBrush = Brushes.Black });
			if ( landlord != null ) row.Cells.Add(new TableCell(landlord) { BorderThickness = new Thickness(0, 0, 0, b), BorderBrush = Brushes.Black });
			return row;
		}

    }
}
