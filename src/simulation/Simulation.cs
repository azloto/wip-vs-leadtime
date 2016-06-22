using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estimation
{
    public class Simulation
    {
        public int DAY { get; set; }
        public int PREDICTED_DAY { get; set; }
        public double AVERANGE_LEADTIME { get; set; }
        public double TOTAL_LEADTIME { get; set; }
        public double[] LaneTaskCountsPerDay { get; set; }

        public LinkedList<Lane> Lanes { get; private set; }

        public Simulation()
        {
            Lanes = new LinkedList<Lane>();
        }

        public void Clear()
        {
            Lanes.Clear();
            DAY = 0;
            PREDICTED_DAY = 0;
            AVERANGE_LEADTIME = 0;
            TOTAL_LEADTIME = 0;
        }

        public int Simulate()
        {
            DAY = 0;
            var total_todo = this.Lanes.First().Tasks.Count;
            while (this.Lanes.Last().Tasks.Count != total_todo)
            {
                NewDay();
                DAY++;

                foreach (var x in this.Lanes)
                    x.CountTasks();

                var ttpd = (double)this.Lanes.Last().Tasks.Count / (double)DAY;
                var tt = this.Lanes.Sum(x => x.Tasks.Count);

                PREDICTED_DAY = (int)(tt / ttpd);
            }

            TOTAL_LEADTIME = this.Lanes.Last().Tasks.Sum(x => x.LeadTimeEnd - x.LeadTimeStart);
            AVERANGE_LEADTIME = TOTAL_LEADTIME / this.Lanes.Last().Tasks.Count;

            return DAY;
        }

        private void NewDay()
        {
            Work();
            Pull();
        }

        private void Work()
        {
            var linkedListItem_currentLane = Lanes.Last;

            while (linkedListItem_currentLane != null)
            {
                var currentLane = linkedListItem_currentLane.Value;
                var workDone = currentLane.ActiveMembers.Sum(x => x.WorkDone(currentLane));

                foreach (var task in currentLane.Tasks)
                    workDone = task.UpdateProgress(currentLane, workDone);

                linkedListItem_currentLane = linkedListItem_currentLane.Previous;
            }
        }

        private void Pull()
        {
            var linkedListItem_currentLane = Lanes.Last;

            while (linkedListItem_currentLane != null)
            {
                if (linkedListItem_currentLane.Next != null)
                {
                    var currentLane = linkedListItem_currentLane.Value;
                    var nextLane = linkedListItem_currentLane.Next.Value;
                    var finishedTasks = (from x in currentLane.Tasks
                                         where x.IsCompletedOrReadyOrWhatEver(currentLane)
                                         select x).ToList();

                    foreach (var task in finishedTasks)
                    {
                        if (nextLane.Tasks.Count < nextLane.WipLimit)
                        {
                            nextLane.Tasks.Add(task);
                            currentLane.Tasks.Remove(task);

                            if (nextLane.StartsTheLeadtime)
                            {
                                task.LeadTimeStart = DAY;
                            }

                            if (nextLane.EndsTheLeadtime)
                            {
                                task.LeadTimeEnd = DAY;
                            }
                        }
                    }
                }

                linkedListItem_currentLane = linkedListItem_currentLane.Previous;
            }
        }
    }
}
