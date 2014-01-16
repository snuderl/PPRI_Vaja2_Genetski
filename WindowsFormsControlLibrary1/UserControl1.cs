using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsControlLibrary1
{
    public partial class UserControl1: UserControl
    {
        public Stavba stavba;
        public int Rows { get; set; }
        public List<Tuple<int, int, double>> extra = null;
        public double[][] powerMap = null;
        public List<Point> path = null;

        public void SetPath(List<Point> path)
        {
            this.path = path;
            pictureBox1.Invalidate();
        }

        public void SetPowerMap(double[][] m)
        {
            this.powerMap = m;
            pictureBox1.Invalidate();
        }

        public UserControl1()
        {
            InitializeComponent();
            stavba = GenerateRandomMap(10);
        }

        public static Stavba GenerateRandomMap(int size, double percentBlocked = 0)
        {
            var rnd = new Random();
            var stavba = new Stavba(size, size);

            var total = Math.Floor(size * size * percentBlocked);
            var c = 0;
            while (c < total)
            {
                var row = rnd.Next(size);
                var col = rnd.Next(size);

                if (stavba.lokacija[row][col] != Lokacija.Zid)
                {
                    stavba.lokacija[row][col] = Lokacija.Zid;
                    c++;
                }

            }
            return stavba;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int size;
            if (int.TryParse(textBox1.Text, out size))
            {

                double percentBlocked = 0;
                double.TryParse(textBox2.Text, out percentBlocked);
                percentBlocked = Math.Abs(percentBlocked);
                percentBlocked = Math.Min(percentBlocked, 0.8);


                this.stavba = GenerateRandomMap(size, percentBlocked);
                pictureBox1.Invalidate();
            }


        }

        public void SetExtra(List<Tuple<int, int, double>> x)
        {
            extra = x;
            pictureBox1.Invalidate();
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

                var rows = stavba.Rows;
                var cols = stavba.Cols;
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
                        if (stavba.lokacija[i][y] != Lokacija.Prosto)
                        {
                            var color = Brushes.Yellow;
                            if (stavba.lokacija[i][y] == Lokacija.Zid)
                            {
                                color = brush;
                            }
                            g.FillRectangle(color, new RectangleF(y * rowStep, i * colStep, rowStep, colStep));
                        }
                    }
                }



                if (powerMap != null && powerMap.Length == stavba.Rows && powerMap[0].Length == stavba.Cols)
                {
                    var offset = 2;
                    var max = Math.Sqrt(powerMap.SelectMany(x => x).Max());
                    for (var i = 0; i < rows; i++)
                    {
                        for (var y = 0; y < cols; y++)
                        {
                            var val = powerMap[i][y];
                            if (val > 0.00001)
                            {
                                //Console.WriteLine(val);
                                var b = new SolidBrush(Color.FromArgb(255, (int)(255 - Math.Sqrt(val) / max * 200), 255));
                                g.FillRectangle(b, new RectangleF(y * rowStep + offset, i * colStep + offset, rowStep - 2 * offset, colStep - 2 * offset));
                            }

                        }
                    }
                }

                if (extra != null)
                {
                    Brush pokrito = new SolidBrush(Color.Black);

                    foreach (var tuple in extra)
                    {
                        var offset = 4;
                        g.FillRectangle(new SolidBrush(Color.Beige), new RectangleF(tuple.Item2 * rowStep + offset, tuple.Item1 * colStep + offset , rowStep - 2 * offset, colStep - 2 * offset));
                        //if (rows >= 20 || cols >= 20)
                        //{
                        //    g.DrawString(tuple.Item3.ToString(), font, fontBrush, new PointF(tuple.Item2 * rowStep, tuple.Item1 * colStep));
                        //}
                        //else
                        //{
                        //    g.DrawString(tuple.Item3.ToString(), font, fontBrush, new PointF(tuple.Item2 * rowStep + rowStep / 2, tuple.Item1 * colStep + colStep / 2));
                        //}
                    }
                }

                if (path != null)
                {
                    var offset = 3;
                    foreach (var tuple in path)
                    {
                        g.FillRectangle(new SolidBrush(Color.Black), new RectangleF(tuple.Y * rowStep + offset, tuple.X * colStep + offset, rowStep - 2 * offset, colStep - 2 * offset));

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
            var rows = stavba.Rows;
            var cols = stavba.Cols;

            float colStep = height / (float)rows;
            float rowStep = width / (float)cols;

            var col = (int)Math.Floor(e.X / rowStep);
            var row = (int)Math.Floor(e.Y / colStep);


            if(stavba.lokacija[row][col] != (Lokacija.Izhod)){
                stavba.lokacija[row][col] = stavba.lokacija[row][col] == Lokacija.Zid ? Lokacija.Prosto : Lokacija.Zid;
            }
            pictureBox1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            if (DialogResult.OK == fd.ShowDialog())
            {
                var f = File.ReadAllLines(fd.FileName);

                var lines = f.Length;
                var cols = f[0].Length;

                var map = new Stavba(lines, cols);
                for (var i = 0; i < lines; i++)
                {
                    var line = f[i];
                    for (var y = 0; y < cols; y++)
                    {
                        var val = Lokacija.Prosto;
                        if (line[y] == '1')
                        {
                            val = Lokacija.Zid;
                        }
                        else if (line[y] == 'E')
                        {
                            val = Lokacija.Izhod;
                            map.exits.Add(new Point(i, y));
                        }
                        map.lokacija[i][y] = val;
                    }
                }

                this.stavba = map;
                this.pictureBox1.Invalidate();
            }

        }




    }
}
