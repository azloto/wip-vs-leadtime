using MathNet.Numerics.Statistics;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;

namespace Estimation
{
    public partial class MainForm : Form
    {
        Configuration config = null;

        public MainForm()
        {
            InitializeComponent();
            config = Configuration.Sample();
            this.txtConfig.Text = config.ToJson();
        }

        /// <summary>
        /// returns a value where % of the values is smaller or equal to the that value
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="excelPercentile"></param>
        /// <returns></returns>
        private static double Percentile(double[] sequence, double excelPercentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;
            double n = (N - 1) * excelPercentile + 1;
            // Another method: double n = (N + 1) * excelPercentile;
            if (n == 1d) return sequence[0];
            else if (n == N) return sequence[N - 1];
            else
            {
                int k = (int)n;
                double d = n - k;
                return (sequence[k - 1] + d * (sequence[k] - sequence[k - 1]));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int sz = 100000;
            lblHisto.Text = $"Running {sz} iterations";
            lblHisto.Update();

            double[] lead_times = new double[sz];
            double[] sum_of_lead_times = new double[sz];
            double[] last_day = new double[sz];

            //
            // simulation, lane, task_counts_per_day
            //
            List<int[]>[] cfd_raw_data = new List<int[]>[sz];
            int max_day = 0;

            progressBar1.Maximum = sz;
            progressBar1.Visible = true;

            var h_total_time = new Histogram();
            for (int i = 0; i < sz; i++)
            {
                progressBar1.Increment(1);
                progressBar1.Update();

                var s = new Simulation();
                config.Configure(s);
                cfd_raw_data[i] = new List<int[]>();
                s.Simulate();

                if (s.DAY > max_day)
                    max_day = s.DAY;

                lead_times[i] = s.AVERANGE_LEADTIME;
                sum_of_lead_times[i] = s.TOTAL_LEADTIME;
                last_day[i] = s.DAY;

                foreach (var lane in s.Lanes)
                    cfd_raw_data[i].Add(lane.GetTaskCounts());
            }

            progressBar1.Visible = false;

            var myModel = new PlotModel { Title = "CFD" };

            var lane_line = new Dictionary<Lane, LineSeries>();
            foreach (var lane in config.Lanes)
            {
                var line = new AreaSeries();
                line.Title = lane.Title;
                myModel.Series.Add(line);
                lane_line.Add(lane, line);
            }

            var max_tasks = 0.0;
            var current_day = 0;
            while (current_day < max_day)
            {
                int lane_index = config.Lanes.Count -1;
                var _90pct = 0.0;

                foreach (var lane in config.Lanes.Reverse())
                {
                    var line = lane_line[lane];

                    // all task counts in the current_lane for the current_day
                    var data = (from sim_data in cfd_raw_data
                                select current_day > sim_data.ElementAt(lane_index).Length - 1 ? 
                                sim_data.ElementAt(lane_index).Last() : 
                                sim_data.ElementAt(lane_index)[current_day]).ToArray();

                    if (lane == config.Lanes.Last())
                        _90pct = data.Average();
                    else
                        _90pct += data.Average();

                    if (max_tasks < _90pct)
                        max_tasks = _90pct;
                    line.Points.Add(new DataPoint(current_day, _90pct));

                    lane_index--;
                }

                current_day++;
            }

            // Define X-Axis
            var Xaxis = new OxyPlot.Axes.LinearAxis();
            Xaxis.Maximum = max_day;
            Xaxis.MajorStep = 5;
            Xaxis.MinorStep = 1;
            Xaxis.Minimum = 0;
            Xaxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            Xaxis.Title = "Day";
            Xaxis.MajorGridlineStyle = LineStyle.Solid;
            Xaxis.MinorGridlineStyle = LineStyle.None;

            myModel.Axes.Add(Xaxis);

            //Define Y-Axis
            var Yaxis = new OxyPlot.Axes.LinearAxis();
            Yaxis.MajorStep = 5;
            Yaxis.Maximum = max_tasks + 3;
            Yaxis.MaximumPadding = 0;
            Yaxis.Minimum = 0;
            Yaxis.MinimumPadding = 0;
            Yaxis.MinorStep = 1;
            Yaxis.Title = "# features";
            Yaxis.MajorGridlineStyle = LineStyle.Solid;
            Yaxis.MinorGridlineStyle = LineStyle.None;
            myModel.Axes.Add(Yaxis);

            this.cfd.Model = myModel;
            
            var histoModel = new PlotModel { Title = "AVG Leadtime" };
            var h_lead_time = new Histogram(lead_times, 50);
            var avg_lead_time = new RectangleBarSeries();
            for (int bi = 0; bi < h_lead_time.BucketCount; bi++)
            {
                var bucket = h_lead_time[bi];
                avg_lead_time.Items.Add(new RectangleBarItem(bucket.LowerBound, 0, bucket.UpperBound, h_lead_time[bi].Count));
            }
            histoModel.Series.Add(avg_lead_time);
            this.histogram.Model = histoModel;
            
            var pct = .9;
            lblHisto.Text += $"\n{pct * 100}% of all leadtimes in the simulation are : { Percentile(lead_times, pct).ToString("0.00") } or lower";
            lblHisto.Text += $"\n{pct * 100}% of all runs in the simulation where finished using : { Percentile(sum_of_lead_times, pct).ToString("0") } units of time or lower";
            lblHisto.Text += $"\n{pct * 100}% of all runs in the simulation where finished before : { Percentile(last_day, pct).ToString("0") } days";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                config.Save(dlg.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            config = Configuration.FromJson(this.txtConfig.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                config = Configuration.Load(dlg.FileName);
                this.txtConfig.Text = config.ToJson();
            }
        }
    }
}
