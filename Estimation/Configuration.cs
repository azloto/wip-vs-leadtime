using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estimation
{
    public class Configuration
    {
        public List<Member> Members { get; private set; }

        public List<Ticket> Tasks { get; private set; }

        public LinkedList<Lane> Lanes { get; private set; }

        public Configuration()
        {
            Tasks = new List<Ticket>();
            Lanes = new LinkedList<Lane>();
            Members = new List<Member>();
        }

        public static Configuration Sample()
        {
            var config = new Configuration();

            //
            // Define members of the team
            //
            var dev1 = new Member() { Name = "Dev1" };
            var dev2 = new Member() { Name = "Dev2" };
            var dev3 = new Member() { Name = "Dev3" };
            var dev4 = new Member() { Name = "Dev4" };

            var test1 = new Member() { Name = "Test1" };
            var test2 = new Member() { Name = "Test2" };

            config.Members.Add(dev1);
            config.Members.Add(dev2);
            config.Members.Add(dev3);
            config.Members.Add(dev4);

            config.Members.Add(test1);
            config.Members.Add(test2);

            //
            // Define all tasks on the Board (in any lane).
            //
            var t1 = new Ticket { Name = "T1" };
            var t2 = new Ticket { Name = "T2" };
            var t3 = new Ticket { Name = "T3" };
            var t4 = new Ticket { Name = "T4" };
            var t5 = new Ticket { Name = "T5" };
            var t6 = new Ticket { Name = "T6" };
            var t7 = new Ticket { Name = "T7" };
            var t8 = new Ticket { Name = "T8" };
            var t9 = new Ticket { Name = "T9" };
            var t10 = new Ticket { Name = "T10"  };

            config.Tasks.Add(t1);
            config.Tasks.Add(t2);
            config.Tasks.Add(t3);
            config.Tasks.Add(t4);
            config.Tasks.Add(t5);
            config.Tasks.Add(t6);
            config.Tasks.Add(t7);
            config.Tasks.Add(t8);
            config.Tasks.Add(t9);
            config.Tasks.Add(t10);

            //
            // (Swim)Lanes
            // 
            var readyForDev = new Lane() { Title = "Backlog", WipLimit = int.MaxValue };
            var dev = new Lane() { Title = "Dev", WipLimit = 1, StartsTheLeadtime = true };
            var test = new Lane() { Title = "Test", WipLimit = 1 };
            var done = new Lane() { Title = "Done", WipLimit = int.MaxValue, EndsTheLeadtime = true };

            config.Lanes.AddLast(readyForDev);
            config.Lanes.AddLast(dev);
            config.Lanes.AddLast(test);
            config.Lanes.AddLast(done);

            //
            // Assign members to lanes
            //
            dev.ActiveMembers.Add(dev1);
            dev.ActiveMembers.Add(dev2);
            dev.ActiveMembers.Add(dev3);
            dev.ActiveMembers.Add(dev4);

            test.ActiveMembers.Add(test1);
            test.ActiveMembers.Add(test2);

            //
            // Add all tasks to the readyForDev lane
            //
            readyForDev.Tasks.Add(t1);
            readyForDev.Tasks.Add(t2);
            readyForDev.Tasks.Add(t3);
            readyForDev.Tasks.Add(t4);
            readyForDev.Tasks.Add(t5);
            readyForDev.Tasks.Add(t6);
            readyForDev.Tasks.Add(t7);
            readyForDev.Tasks.Add(t8);
            readyForDev.Tasks.Add(t9);
            readyForDev.Tasks.Add(t10);

            //
            // Initialize ticket (task) sizes
            //

            foreach (var task in readyForDev.Tasks)
            {
                var random = new DiscreteUniform(1, 20);
                var samples = new int[2];

                random.Samples(samples);
                if (samples[0] > samples[1])
                    task.Configure(dev, samples[1], samples[0]);
                else
                    task.Configure(dev, samples[0], samples[1]);

                random.Samples(samples);
                if (samples[0] > samples[1])
                    task.Configure(dev, samples[1], samples[0]);
                else
                    task.Configure(dev, samples[0], samples[1]);
            }

            //
            // Configure competence of each member in each lane
            // Members can work in any lane, but it might affect performance.
            //
            dev1.Configure(dev, 1, 3);
            dev2.Configure(dev, 1, 2);
            dev3.Configure(dev, 1, 3);
            dev4.Configure(dev, 1, 4);

            test1.Configure(test, 1, 2);
            test2.Configure(test, 2, 4);

            return config;
        }

        public void Save(string path)
        {
            File.WriteAllText(path, ToJson());
        }

        public static Configuration Load(string path)
        {
            string json = File.ReadAllText(path);
            return FromJson(json);
        }

        public static Configuration FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Configuration>(json, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public void Configure(Simulation simulation)
        {
            simulation.Lanes.Clear();
            foreach (var lane in Lanes)
                simulation.Lanes.AddLast(lane.Copy());
        }
    }
}
