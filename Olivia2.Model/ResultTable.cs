using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;

namespace Acly.Olivia2.Model {

    public class Interval {
        public Interval(int start, int end) {
            Start = start;
            End = end;
        }
        public int Start { get; set; }
        public int End { get; set; }
        public int Duration { get { return End - Start + 1; } }
        public Interval Union(Interval other) {
            return new Interval(Math.Min(Start, other.Start), End = Math.Max(End, other.End));
        }
    }

	public class ResultInfo {
		public Lessee Lessee;
        public List<Interval> Durations = new List<Interval>();
		public double FlatSize = 0;
		public double Members = 0;
		public DateTime StartDate = DateTime.MaxValue;
		public DateTime EndDate = DateTime.MinValue;
		public double AdvancePayment = 0;
		public Dictionary<Cost, double> Costs = new Dictionary<Cost,double>();

        public double Duration { get {
            if (Lessee == null) return 24;
            return Durations.Aggregate((left, right) => left.Union(right)).Duration; 
        } }
		public double AverageFlatSize { get { return Duration > 0 ? FlatSize / Duration : 0; } }
		public double AverageMembers { get { return Duration > 0 ? Members / Duration : 0; } }
		public double Months { get { return Duration / 2.0; } }
	}

	public class ResultTable : Base {

		public ResultTable(Project project) {
			Project = project;
		}

		public Dictionary<Lessee, ResultInfo> Lessees { get; protected set; }
		public ResultInfo Vacancy { get; protected set; }
		public ResultInfo Landlord { get; protected set; }
		public ResultInfo Error { get; protected set; }
		public IEnumerable<Cost> Costs { get { return Project.Costs; } }

		public void Calculate() {
			Lessees = new Dictionary<Lessee, ResultInfo>();
			Vacancy = new ResultInfo();
			Landlord = new ResultInfo();
			if ( Project == Project.Empty ) return;

			foreach ( var fac in Project.Assignments ) {
				foreach ( var fa in fac ) {
					if ( !Lessees.ContainsKey(fa.Lessee) ) Lessees[fa.Lessee] = new ResultInfo();
					Lessees[fa.Lessee].Lessee = fa.Lessee;
					if ( fa.Start < Lessees[fa.Lessee].StartDate ) Lessees[fa.Lessee].StartDate = fa.Start;
					if ( fa.End > Lessees[fa.Lessee].EndDate ) Lessees[fa.Lessee].EndDate = fa.End;
					Lessees[fa.Lessee].Durations.Add(new Interval(fa.StartIntervalIndex, fa.EndIntervalIndex));
					Lessees[fa.Lessee].Members = fa.Lessee.Members;
					Lessees[fa.Lessee].FlatSize += fac.Flat.Size * fa.Duration;
				}
				Vacancy.Members += fac.VacantDuration;
				Vacancy.FlatSize += fac.Flat.Size * fac.VacantDuration;
			}

			foreach ( var result in Lessees.Values ) {
				result.AdvancePayment = TimeInterval.Within(result.StartDate, result.EndDate).Sum(
					i => result.Lessee.Payments.Get(i.Start) / 2.0);
                result.Members *= result.Duration;
			}

			foreach ( var cost in Project.Costs ) {
				if ( !Landlord.Costs.ContainsKey(cost) ) Landlord.Costs[cost] = 0;
				var devisor = CalculateDevisor(cost, Lessees, Vacancy);
				foreach ( var lessee in Lessees.Keys ) {
					var option = cost.Options.FirstOrDefault(o => o.Lessee == lessee); 
					if ( option == null || (!option.Affected && !option.Exempt) ) continue;
					if ( option.Exempt ) Landlord.Costs[cost] += CalculateAmount(cost, Lessees[lessee], devisor);
					else Lessees[lessee].Costs[cost] = CalculateAmount(cost, Lessees[lessee], devisor);
				}
				if ( cost.AffectsVacancy ) Vacancy.Costs[cost] = CalculateAmount(cost, Vacancy, devisor);
			}

			Error = new ResultInfo();
			foreach ( var cost in Project.Costs ) {
				Error.Costs[cost] = Math.Abs(Lessees.Sum(i => i.Value.Costs.ContainsKey(cost) ? i.Value.Costs[cost] : 0)
					+ (Vacancy.Costs.ContainsKey(cost) ? Vacancy.Costs[cost] : 0)
					+ (Landlord.Costs.ContainsKey(cost) ? Landlord.Costs[cost] : 0 )
					- cost.Amount);
			}
			
			NotifyPropertyChanged("Result");
		}

		public double CalculateDevisor(Cost cost, Dictionary<Lessee, ResultInfo> result, ResultInfo vacancy) {
			switch ( cost.Mode ) {
				case CostMode.Lessee: 
					return cost.Options.Sum(o => o.Affected || o.Exempt ? result[o.Lessee].Duration : 0) 
						+ (cost.AffectsVacancy ? vacancy.Members : 0);
				case CostMode.Member:
					return cost.Options.Sum(o => o.Affected || o.Exempt ? result[o.Lessee].Members : 0) 
						+ (cost.AffectsVacancy ?  vacancy.Members : 0);
				case CostMode.Flat:
					return cost.Options.Sum(o => o.Affected || o.Exempt ? result[o.Lessee].FlatSize : 0) 
						+ (cost.AffectsVacancy ? vacancy.FlatSize : 0);
			}
			return 1;
		}

		public double CalculateAmount(Cost cost, ResultInfo info, double devisor) {
			switch ( cost.Mode ) {
				case CostMode.Lessee: return cost.Amount * info.Duration / devisor;
				case CostMode.Member: return cost.Amount * info.Members / devisor;
				case CostMode.Flat: return cost.Amount * info.FlatSize / devisor;
				case CostMode.External: 
                    if (info.Lessee != null)
					    return cost.Options.First(o => o.Lessee == info.Lessee).Amount;
                    else if (info == Vacancy)
                        return cost.VacancyAmount;
                    else return 0;
			}
			return 0;
		}

		protected Project Project { get; set; }

		public override string this[string p] { 
			get { return null; } }
	}

}
