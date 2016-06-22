using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estimation
{
    public class Lane
    {
        private List<int> taskCounts;

        public string Title { get; set; }

        public int WipLimit { get; set; }

        public List<Ticket> Tasks { get; private set; }

        public List<Member> ActiveMembers { get; private set; }

        public Lane()
        {
            this.taskCounts = new List<int>();
            this.Tasks = new List<Ticket>();
            this.ActiveMembers = new List<Member>();
        }

        public void CountTasks()
        {
            taskCounts.Add(this.Tasks.Count);
        }

        public int[] GetTaskCounts()
        {
            return taskCounts.ToArray();
        }

        public override string ToString()
        {
            return Title;
        }

        public bool StartsTheLeadtime { get; set; }

        public bool EndsTheLeadtime { get; set; }

        public Lane Copy()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Lane>(json);
        }
    }
}
