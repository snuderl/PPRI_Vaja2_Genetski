using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PPRI_Vaja2_Genetski
{
    public partial class Form1 : Form
    {

        Boolean[][] stavba;
        Oddajnik selected = null;
        Pokritje trainer;
        GeneticAlgorithm<Oddajnik> GA = null;

        public static Boolean[][] GenerateRandomMap(int size, double percentBlocked = 0.2)
        {
            var rnd = new Random();
            Boolean[][] map = new Boolean[size][];
            for (var i = 0; i < size; i++)
            {
                map[i] = new Boolean[size];
            }

            var total = Math.Floor(size * size * percentBlocked);
            var c = 0;
            while (c < total)
            {
                var row = rnd.Next(size);
                var col = rnd.Next(size);

                if (!map[row][col])
                {
                    map[row][col] = true;
                    c++;
                }

            }
            return map;
        }

        public Form1()
        {
            InitializeComponent();


            textBox1.Text = "10";
            textBox2.Text = "0.2";
            stavba = GenerateRandomMap(10);

            chart1.Series[0].ChartType = SeriesChartType.Line;
        }


        private void OnProgress(IEnumerable<Generation<Oddajnik>> updated)
        {
            updated.ToList().ForEach(x => {
                chart1.Series[0].Points.Add(new DataPoint(x.Number, x.MaxFitness));
                chart1.Series[1].Points.Add(new DataPoint(x.Number, x.AverageFitness));
                chart1.Series[2].Points.Add(new DataPoint(x.Number, x.WorstFitness));
            });

            DrawGenerations(updated);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int size;
            if (int.TryParse(textBox1.Text, out size))
            {

                double percentBlocked = 0.2;
                double.TryParse(textBox2.Text, out percentBlocked);
                percentBlocked = Math.Abs(percentBlocked);
                percentBlocked = Math.Min(percentBlocked, 0.8);


                this.stavba = GenerateRandomMap(size, percentBlocked);
                pictureBox1.Invalidate();
            }


        }





        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            using (Pen blackpen = new Pen(Color.Black, 3))
            {
                Brush brush = Brushes.Red;
                Brush fontBrush = Brushes.Blue;
                Font font = new Font("Default", 16);

                Graphics g = e.Graphics;
                g.Clear(Color.White);
                var width = pictureBox1.Width;
                var height = pictureBox1.Height;

                var rows = stavba.Length;
                var cols = stavba[0].Length;
                float colStep = height / (float)rows;
                float rowStep = width / (float)cols;

                for (var i = 0; i < rows + 1; i++)
                {
                    g.DrawLine(blackpen, 0, i * colStep, width, i * colStep);

                }
                for (var y = 0; y < cols + 1; y++)
                {
                    g.DrawLine(blackpen, y * rowStep, 0, y * rowStep, height);
                }

                for (var i = 0; i < rows; i++)
                {
                    for (var y = 0; y < cols; y++)
                    {
                        if (stavba[i][y])
                        {
                            g.FillRectangle(brush, new RectangleF(y * rowStep, i * colStep, rowStep, colStep));
                        }
                    }
                }

                if (selected != null)
                {
                    Brush pokrito = new SolidBrush(Color.Yellow);
                    HashSet<Tuple<int, int>> pokriti = new HashSet<Tuple<int, int>>();
                    trainer.EvaluateEager(selected, pokriti);
                    foreach(var tuple in pokriti)
                    {                        
                        g.FillRectangle(pokrito, new RectangleF(tuple.Item2 * rowStep, tuple.Item1 * colStep, rowStep, colStep));
                    }

                    foreach (var tuple in selected.GetTuples())
                    {
                        if (stavba.Length >= 20)
                        {
                            g.DrawString(tuple.Item3.ToString(), font, fontBrush, new PointF(tuple.Item2 * rowStep, tuple.Item1 * colStep));
                        }
                        else
                        {
                            g.DrawString(tuple.Item3.ToString(), font, fontBrush, new PointF(tuple.Item2 * rowStep + rowStep / 2, tuple.Item1 * colStep + colStep / 2));
                        }
                    }
                }
            }
        }

        private double Distance(int x1, int x2)
        {
            return Math.Sqrt(x1 * x1 + x2 * x2);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var width = pictureBox1.Width;
            var height = pictureBox1.Height;
            var rows = stavba.Length;
            var cols = stavba[0].Length;

            float colStep = height / (float)rows;
            float rowStep = width / (float)cols;

            var col = (int)Math.Floor(e.X / rowStep);
            var row = (int)Math.Floor(e.Y / colStep);

            Console.WriteLine(row + " " + col);


            stavba[row][col] = !stavba[row][col];
            pictureBox1.Invalidate();
        }

        private void Clear()
        {
            treeView1.SelectedNode = null;
            treeView1.Nodes.Clear();
            selected = null;
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            pictureBox1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Clear();
            int popSize;
            int maxPower = 5;
            if (int.TryParse(textBox3.Text, out popSize))
            {
                int chromosomeSize;
                int elite;
                if (!int.TryParse(textBox4.Text, out chromosomeSize))
                {
                    chromosomeSize = 5;
                }
                if (!int.TryParse(textBox7.Text, out elite))
                {
                    elite = 1;
                }
                double cross;
                double mut;
                if (!double.TryParse(textBox5.Text, out cross))
                {
                    cross = 0.2;
                }
                if (!double.TryParse(textBox6.Text, out mut))
                {
                    mut = 0.1;
                }
                int.TryParse(textBox8.Text, out maxPower);
                double C;
                double.TryParse(textBox9.Text, out C);
                int MaxTotalPower = 15;
                int.TryParse(textBox10.Text, out MaxTotalPower);

                trainer = new Pokritje(stavba, 1, chromosomeSize, maxPower, C, MaxTotalPower);
                GA = new GeneticAlgorithm<Oddajnik>(trainer, popSize, mut, cross, elite, comboBox1.Text);

                OnProgress(GA.generations);
            }
        }

        private void DrawGenerations(IEnumerable<Generation<Oddajnik>> list)
        {
            TreeNode last = null;
            list.ToList().ForEach(generation =>
            {
                List<TreeItem> items = new List<TreeItem>();
                var children = generation.members.Take(100).Select(x => new TreeNode(x.ToString())).ToArray();
                var tn = new TreeNode(generation.ToString(), children);
                last = children.First();
                treeView1.Nodes.Add(tn);
            });

            treeView1.SelectedNode = last;
            treeView1_AfterSelect_1(null, null);
        }


        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            var sel = treeView1.SelectedNode;
            if (sel != null)
            {
                if (sel.Level == 1)
                {
                    int generation = sel.Parent.Index;
                    int member = sel.Index;

                    this.selected = GA.generations[generation].members[member];

                    label13.Text = this.selected.Pokrtitih.ToString();
                    label14.Text = this.selected.Power.ToString();

                }
                else
                {
                    selected = null;
                    label13.Text = "";
                    label14.Text = "";
                }
                pictureBox1.Invalidate();
            }

        }

        private void onCompleted()
        {
            this.runing = false;
            this.button2.Enabled = true;
            button3.Text = "Run";
        }

        private void button3_Click(object sender, EventArgs e)
        {



            if (runing == false)
            {
                GA.Run(this.OnProgress, onCompleted, 100);
                button3.Text = "Stop";
                runing = true;
                this.button2.Enabled = false;
            }
            else
            {
                GA.Stop();
                button3.Text = "Run";
                runing = false;
                this.button2.Enabled = true;
            }
        }

        private bool runing = false;

        private void button4_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            if (DialogResult.OK == fd.ShowDialog())
            {
                var f = File.ReadAllLines(fd.FileName);

                var lines = f.Length;
                var rows = f[0].Length;

                Boolean[][] map = new Boolean[lines][];
                for (var i = 0; i < lines; i++)
                {
                    var line = f[i];
                    map[i] = new Boolean[rows];
                    for (var y = 0; y < line.Length; y++)
                    {
                        map[i][y] = line[y] == '1' ? true : false;
                    }
                }

                this.stavba = map;
                this.Clear();
            }

        }

    }

    public class TreeItem
    {
        public string Name;
        public int Level;
        public TreeItem(string name, int level)
        {
            Name = name; Level = level;
        }
    }
}
