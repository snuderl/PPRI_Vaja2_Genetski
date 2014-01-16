using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WindowsFormsControlLibrary1;

namespace Inteligenca_rojev
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(Fitness.KvadratnaMetodaDDA(1, 5, 2, 10, userControl11.stavba));

            double maxPower;
            if (!double.TryParse(textBox1.Text, out maxPower))
            {
                maxPower = 10;
            }
            int maxOddajnikov;
            if (!Int32.TryParse(textBox3.Text, out maxOddajnikov))
            {
                maxOddajnikov = 5;
            }
            
            double cost;
            if (!double.TryParse(textBox2.Text, out cost))
            {
                cost = 5;
            }

            double damping;
            if (!double.TryParse(textBox4.Text, out damping))
            {
                damping = 0.8;
            }

            double powerMultD = 1;
            if (double.TryParse(powerMult.Text, out powerMultD))
            {
                Fitness.C = powerMultD;
            }

            int n;
            if (!Int32.TryParse(textBox7.Text, out n))
            {
                n = 20;
            }

            String topologija = comboBox1.Text;

            Fitness.strosek = cost;
            Fitness.q = damping;
            PSO pso = new PSO(50, userControl11.stavba, maxOddajnikov, maxPower, topologija);
            pso.ConvergenceIterations = n;
            int iteracij = 100;
            if (!int.TryParse(iteracij1.Text, out iteracij))
            {
                iteracij = 100;
            }
            pso.Run(iteracij, OnProgress, () =>
            {
                Particle best = pso.particles.Where(x => x.score == pso.particles.Max(z => z.score)).FirstOrDefault();
                best.state = best.state.Where(x => x.power > 0).ToArray();
                userControl11.SetExtra(best.state.Select(x => new Tuple<int, int, double>(x.X, x.Y, x.power)).ToList());

                Console.WriteLine("Done " + best.score + " ," + best.state.Max(x => x.power));
                Console.WriteLine(pso.scores.Max(x => x.Item1));

                var m = Fitness.generatePowerMap(userControl11.stavba);
                Fitness.CalculatePower(userControl11.stavba, best.state, m);
                userControl11.SetPowerMap(m);


                listView1.Items.Clear();
                best.state.ToList().ForEach(x =>
                {
                    listView1.Items.Add(new ListViewItem(new String[] { x.X.ToString(), x.Y.ToString(), x.power.ToString() }));
                });

                label5.Text = "Oddajnikov " + best.state.Length;
            }, userControl11);




        }

        private void OnProgress(IEnumerable<Tuple<double,double,double>> updated)
        {
            var i = 1;
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[1].ChartType = SeriesChartType.Line;
            chart1.Series[2].ChartType = SeriesChartType.Line;
            updated.ToList().ForEach((x) =>
            {
                if (i % 10 == 1)
                {
                    chart1.Series[0].Points.Add(new DataPoint(i, x.Item1));
                    chart1.Series[1].Points.Add(new DataPoint(i, x.Item2));
                    chart1.Series[2].Points.Add(new DataPoint(i, x.Item3));
                }
                i++; 
            });
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double p = 0.1;
            if (double.TryParse(textBox1.Text, out p))
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Starting");
            var alfa = 0.8;
            var n = 5;
            double t = 1 + trackBar1.Value / 1000.0;
            var stavba = userControl11.stavba;
            var powerMap = userControl11.powerMap;
            var feromon = Mravlje.initializeFeromoneMap(stavba);

            var start = stavba.exits[0];
            var exit = stavba.exits[1];

            int iteracij = 10;
            if (!int.TryParse(iteracij2.Text, out iteracij))
            {
                iteracij = 10;
            }
            int mravelj;
            if (!int.TryParse(stMravelj.Text, out mravelj))
            {
                mravelj = 5;
            }
            double p;
            if (!double.TryParse(pText.Text, out p))
            {
                p = 0.5;
            }
            Mravlje.GetBestPath(stavba, feromon, powerMap, t, alfa, start, exit, userControl11, OnProgress, mravelj, iteracij, p);
            //userControl11.SetPath(best);
        }

        Imunski imunski;
        private void button3_Click(object sender, EventArgs e)
        {
            int vzorcev = 10;
            if (!int.TryParse(stVzorcev.Text, out vzorcev))
            {
                vzorcev = 10;
            }
            int dolzina = 200;
            if (!int.TryParse(Lmax.Text, out dolzina))
            {
                dolzina = 200;
            }

            if (userControl11.powerMap != null && userControl11.extra != null)
            {
                userControl11.SetPath(null);
                imunski = new Imunski(userControl11.stavba, userControl11.powerMap, userControl11.extra, dolzina, vzorcev);
                listView2.Items.Clear();

                for (var i = 0; i < imunski.samples.Count; i++)
                {
                    var path = imunski.samples[i];
                    var vector = imunski.vectors[i];

                    var vectorString = String.Join(", ", vector);
                    listView2.Items.Add(new ListViewItem(
                        new String[]{
                            i.ToString(), " ", vectorString
                        }));
                }
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count == 1)
            {
                int index = listView2.SelectedIndices[0];
                if (index < imunski.samples.Count)
                {

                    userControl11.SetPath(imunski.samples[index].ToList());
                }
                else
                {
                    index -= imunski.samples.Count;
                    userControl11.SetPath(imunski.antigens[index].ToList());
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            double threshold = 10;
            if (!double.TryParse(textBox5.Text, out threshold))
            {
                threshold = 10;
            }
            imunski.TvoriAntigen(threshold);

            listView2.Items.Clear();
            for (var i = 0; i < imunski.samples.Count; i++)
            {
                var path = imunski.samples[i];
                var vector = imunski.vectors[i];

                var vectorString = String.Join(", ", vector);
                listView2.Items.Add(new ListViewItem(
                    new String[]{
                            i.ToString(), " ", vectorString
                        }));
            }


            for (var i = 0; i < imunski.antigens.Count;i++ )
            {
                var path = imunski.antigens[i];
                var vector = imunski.antiVectors[i];
                var scores = imunski.afiniteta[i]; 
                //scores.Select(x => string.Format("{0:N2}%", x));

                var vectorString = String.Join(", ", vector);
                var scoresString = String.Join(",", scores.Where(x => scores.Max() == x).Select(x => string.Format("{0:N2}", x)));
                var s = (scores.Max() > threshold ? "Vsiljivec" : "");
                scoresString = s + scoresString;
                listView2.Items.Add(new ListViewItem(
                    new String[]{
                            i.ToString(), scoresString, vectorString
                        }));
            }
            
        }




    }
}
