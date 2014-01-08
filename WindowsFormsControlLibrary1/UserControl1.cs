﻿using System;
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
        public UserControl1()
        {
            InitializeComponent();
            stavba = GenerateRandomMap(50);
        }

        public static Stavba GenerateRandomMap(int size, double percentBlocked = 0.2)
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


            if(stavba.lokacija[row][col] != (Lokacija.Izhod | Lokacija.Vhod)){
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