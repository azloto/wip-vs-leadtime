using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estimation
{
    public class Member
    {
        public string Name { get; set; }
        public Dictionary<string, int> MaxPerformance { get; private set; }
        public Dictionary<string, int> MinPerformance { get; private set; }

        public Member()
        {
            MaxPerformance = new Dictionary<string, int>();
            MinPerformance = new Dictionary<string, int>();
        }

        public void Configure(Lane lane, int min_performance, int max_performance)
        {
            if (!this.MaxPerformance.ContainsKey(lane.Title))
                this.MaxPerformance.Add(lane.Title, 0);

            if (!this.MinPerformance.ContainsKey(lane.Title))
                this.MinPerformance.Add(lane.Title, 0);

            this.MaxPerformance[lane.Title] = max_performance;
            this.MinPerformance[lane.Title] = min_performance;
        }

        public int WorkDone(Lane lane)
        {
            if (this.MaxPerformance.ContainsKey(lane.Title) && this.MinPerformance.ContainsKey(lane.Title))
            {
                var low = this.MinPerformance[lane.Title];
                var high = this.MaxPerformance[lane.Title];
                var cu = new DiscreteUniform(low, high);
                int[] ret = new int[1];
                cu.Samples(ret);
                return ret[0];
            }
            return 0;
        }
    }
}
