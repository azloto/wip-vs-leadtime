using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estimation
{
    public class Ticket
    {
        public Dictionary<string, int> SizeInBestCase { get; private set; }
        public Dictionary<string, int> SizeInWorstCase { get; private set; }
        private Dictionary<string, int> progress;

        public Ticket()
        {
            SizeInBestCase = new Dictionary<string, int>();
            SizeInWorstCase = new Dictionary<string, int>();
            progress = new Dictionary<string, int>();
        }

        public string Name { get; set; }

        public void Configure(Lane lane, int optimistic, int pessimistic)
        {
            if (!this.SizeInBestCase.ContainsKey(lane.Title))
                this.SizeInBestCase.Add(lane.Title, 0);

            if (!this.SizeInWorstCase.ContainsKey(lane.Title))
                this.SizeInWorstCase.Add(lane.Title, 0);

            this.SizeInBestCase[lane.Title] = optimistic;
            this.SizeInWorstCase[lane.Title] = pessimistic;
        }

        private int InitiateTaskSize(Lane lane)
        {
            if (this.SizeInBestCase.ContainsKey(lane.Title) && this.SizeInWorstCase.ContainsKey(lane.Title))
            {
                var low = this.SizeInBestCase[lane.Title];
                var high = this.SizeInWorstCase[lane.Title];
                var cu = new DiscreteUniform(low, high);
                int[] ret = new int[1];
                cu.Samples(ret);
                return ret[0];
            }
            return 0;
        }

        public int UpdateProgress(Lane lane, int progress)
        {
            if (progress < 0)
                return 0;

            if (!this.progress.ContainsKey(lane.Title))
                this.progress.Add(lane.Title, InitiateTaskSize(lane));

            var initial_task_size = this.progress[lane.Title];
            var new_task_size = Math.Max(0, initial_task_size - progress); // task size can't go below 0.
            this.progress[lane.Title] = new_task_size;
            return progress - (initial_task_size - new_task_size); // return what is left of the effort
        }

        public bool IsCompletedOrReadyOrWhatEver(Lane lane)
        {
            if (!this.progress.ContainsKey(lane.Title))
                return true;

            return this.progress[lane.Title] == 0;
        }


        public int LeadTimeStart { get; set; }
        public int LeadTimeEnd { get; set; }
    }
}
