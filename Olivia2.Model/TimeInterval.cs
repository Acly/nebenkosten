using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acly.Olivia2.Model {

	public class TimeInterval {

		public TimeInterval(DateTime start) {
			Start = start;
		}

		public DateTime Start { get; set; }
		public DateTime End { get { return EndDate(Start); } }

		public override string ToString() {
			return Start.ToString("dd. MMMM yyyy");
		}

		public string ToEndString() {
			return End.ToString("dd. MMMM yyyy");
		}

		public static DateTime EndDate(DateTime start) {
			if ( start.Day == 1 ) return start.AddDays(13);
			else return new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month));
		}

		public static DateTime NextDate(DateTime date) {
			if ( date.Day == 1 ) return date.AddDays(14);
			else return new DateTime(date.AddMonths(1).Year, date.AddMonths(1).Month, 1);
		}

		public static IEnumerable<TimeInterval> Within(DateTime start, DateTime end) {
			for ( DateTime date = start; date < end; date = NextDate(date) ) {
				yield return new TimeInterval(date);
			}
		}

		public static int Find(IList<TimeInterval> intervals, Func<TimeInterval, bool> selector, int def = 0) {
			var i = intervals.FirstOrDefault(selector);
			if ( i != null ) return intervals.IndexOf(i);
			return def;
		}

		public static DateTime StartFromIndex(IList<TimeInterval> intervals, int[] indices) {
			for ( int i = 0; i < indices.Count(); ++i ) {
				if ( indices[i] > 0 ) return intervals[i].Start;
			}
			return intervals[intervals.Count - 1].Start;
		}

		public static DateTime EndFromIndex(IList<TimeInterval> intervals, int[] indices) {
			bool hasstarted = false;
			for ( int i = 0; i < indices.Count(); ++i ) {
				if ( indices[i] > 0 ) hasstarted = true;
				if ( indices[i] == 0 && hasstarted ) return intervals[i-1].End;
			}
			return intervals[intervals.Count - 1].End;
		}
	}


}
