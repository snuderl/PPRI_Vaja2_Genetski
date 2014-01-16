using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsControlLibrary1;

namespace Inteligenca_rojev
{
    public class Particle
    {
        public double bestScore;
        public double score;
        public double neighbourhoudScore;
        public Oddajnik[] neighbourhoudBest;
        public Oddajnik[] best;
        public Oddajnik[] state;
        public double[] velocity;

        public Particle(List<Oddajnik> oddajniki)
        {
            bestScore = 0;
            score = 0;
            neighbourhoudScore = 0;
            state = oddajniki.ToArray();
            velocity = new double[oddajniki.Count * 3];
            best = state;
            neighbourhoudBest = state;
        }

        public void Update(double c1, double c2, double[] r1, double[] r2, Stavba s)
        {
            double[] s1 = new double[r1.Length];
            double[] s2 = new double[r1.Length];

            var best = Oddajnik.ToVector(this.best);
            var neigh = Oddajnik.ToVector(this.neighbourhoudBest);
            var state = Oddajnik.ToVector(this.state);
            for (var i = 0; i < r1.Length; i++)
            {
                var f1 = c1 * r1[i] * (best[i] - state[i]);
                var f2 = c2 * r2[i] * (neigh[i] - state[i]);
                velocity[i] += f1 + f2;
            }
            for (var i = 0; i < r1.Length; i += 3)
            {
                var x = Convert.ToInt32(state[i] + velocity[i]);
                var y = Convert.ToInt32(state[i + 1] + velocity[i + 1]);
                state[i + 2] += velocity[i + 2];
                if (x >= 0 && y >= 0 && x < s.Rows && y < s.Cols && s.lokacija[x][y] == Lokacija.Prosto)
                {
                    state[i] += velocity[i];
                    state[i + 1] += velocity[i + 1];
                }
            }

            this.state = Oddajnik.FromVector(state);
        }
    }




    public class Oddajnik
    {
        public double power;
        public double x;
        public double y;

        public int X { get { return Convert.ToInt32(x); } }
        public int Y { get { return Convert.ToInt32(y); } }

        public Oddajnik(double x, double y, double power)
        {
            this.x = x;
            this.y = y;
            this.power = power;
        }

        public static double[] ToVector(Oddajnik[] oddajniki)
        {
            var s = new double[oddajniki.Length * 3];
            for (var i = 0; i < oddajniki.Length; i++)
            {
                s[3 * i] = oddajniki[i].x;
                s[3 * i + 1] = oddajniki[i].y;
                s[3 * i + 2] = oddajniki[i].power;
            }
            return s;
        }

        public static Oddajnik[] FromVector(double[] vector)
        {
            var s = new Oddajnik[vector.Length / 3];
            for (var i = 0; i < vector.Length; i+=3)
            {
                var o = new Oddajnik(vector[i], vector[i + 1], vector[i + 2]);
                s[i / 3] = o;
            }
            return s;
        }
    }

    public class Fitness
    {
        public static double C = 1;
        public static double q = 0.9;
        public static double strosek = 5;

        public static double[][] generatePowerMap(Stavba s)
        {
            var map = new double[s.Rows][];
            for (var i = 0; i < s.Rows; i++)
            {
                var row = Enumerable.Range(0, s.Cols).Select(x => 0.0).ToArray();
                map[i] = row;
            }
            return map;
        }

        public static double CalculateFitness(Stavba stavba, Oddajnik[] oddajniki)
        {
            var powerMap = generatePowerMap(stavba);
            var outside = CalculatePower(stavba, oddajniki, powerMap);

            var fitness = powerMap.SelectMany(x => x).Sum() - strosek * oddajniki.Where(x => x.power > 0.00001).Count() - outside;
            return fitness;
        }

        public static double CalculatePower(Stavba stavba, Oddajnik[] oddajniki, double[][] powerMap)
        {
            Dictionary<Point, List<double>> outside = new Dictionary<Point, List<double>>();

            List<double>[][] listPowerMap = new List<double>[stavba.Rows][];
            for (var i = 0; i < stavba.Rows; i++)
            {
                var row = new List<double>[stavba.Cols];
                for (var y = 0; y < stavba.Cols; y++)
                {
                    row[y] = new List<double>();
                }
                listPowerMap[i] = row;

            }

            foreach (var p in oddajniki)
            {
                if (p.power > 0 && stavba.lokacija[p.X][p.Y] == Lokacija.Prosto)
                {
                    var sqrtPower = Math.Sqrt(p.power);
                    double range = C * sqrtPower;

                    var d = (int)(range + 0.5);


                    for (var x = p.X - d; x < p.X + d; x++)
                    {
                        for (var y = p.Y - d; y < p.Y + d; y++)
                        {
                            if(x < 0 || y < 0 || x >= stavba.Rows || y >= stavba.Cols){
                                double distance = Math.Sqrt(Math.Pow(x - p.x, 2) + Math.Pow(y - p.y, 2));
                                if (distance < range)
                                {
                                    var count = CalculateWallCount(stavba, p, x, y);
                                    var neoslabljen = p.power * (1 - distance / (C * sqrtPower));
                                    var oslabljen = Math.Pow(q, count) * neoslabljen;
                                    var t = new Point(x, y);
                                    if (!outside.ContainsKey(t))
                                    {
                                        outside[t] = new List<double> { oslabljen };
                                    }
                                    else
                                    {
                                        outside[t].Add(oslabljen);
                                    }
                                }
                            }
                            else if (stavba.lokacija[x][y] == Lokacija.Prosto)
                            {
                                double distance = Distance(x + 0.5, y + 0.5, p.x + 0.5, p.y + 0.5);
                                if (distance < range)
                                {
                                    var count = CalculateWallCount(stavba, p, x, y);
                                    var neoslabljen = p.power * (1 - distance / range);
                                    var oslabljen = Math.Pow(q, count) * neoslabljen;
                                    listPowerMap[x][y].Add(oslabljen);
                                }
                            }
                        }
                    }
                }
            }

            for (var i = 0; i < stavba.Rows; i++)
            {
                for (var y = 0; y < stavba.Cols; y++)
                {
                    var l = listPowerMap[i][y];
                    if (l.Count == 1)
                    {
                        powerMap[i][y] = l[0];
                    }
                    else if (l.Count > 1)
                    {
                        powerMap[i][y] = Math.Max(0, 2 * l.Max() - l.Sum());
                    }
                }
            }

            double outsidePower = 0;
            foreach(var kv in outside){
                if (kv.Value.Count == 1)
                {
                    outsidePower += kv.Value[0];
                }
                else if (kv.Value.Count > 1)
                {
                    outsidePower += Math.Max(0, 2 * kv.Value.Max() - kv.Value.Sum());
                }
            }
            return outsidePower;
        }

        public static bool isInBoundary(Oddajnik p, Stavba s)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < s.Rows && p.Y < s.Cols;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static double CalculateWallCount(Stavba stavba, Oddajnik p, int x, int y)
        {
            return KvadratnaMetodaDDA(x, y, p.X, p.Y, stavba);
        }

        public static int KvadratnaMetodaDDA(int x1, int y1, int x2, int y2, Stavba stavba)
        {
            int count = 0;


            if (y1 == y2)
            {
                for (var i = Math.Min(x1, x2); i <= Math.Max(x1, x2); i++)
                {
                    if (i >= 0 && i < stavba.Rows && y1 >= 0 && y1 < stavba.Cols)
                        count += stavba.lokacija[i][y1] == Lokacija.Zid ? 1 : 0;
                }
                return count;
            }

            else if (x1 == x2)
            {
                for (var i = Math.Min(y1, y2); i <= Math.Max(y1, y2); i++)
                {
                    if (x1 >= 0 && x1 < stavba.Rows && i >= 0 && i < stavba.Cols)
                        count += stavba.lokacija[x1][i] == Lokacija.Zid ? 1 : 0;
                }
                return count;
            }

            else
            {
                ///Modified Bresenham algorithm
                int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
                int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
                int err = dx + dy, e2;

                for (; ; )
                {
                    if (x1 >= 0 && x1 < stavba.Rows && y1 >= 0 && y1 < stavba.Cols && stavba.lokacija[x1][y1] == Lokacija.Zid)
                        count ++;

                    if (x1 == x2 && y1 == y2) break;

                    e2 = 2 * err;

                    // EITHER horizontal OR vertical step (but not both!)
                    if (e2 > dy)
                    {
                        err += dy;
                        x1 += sx;
                    }
                    else if (e2 < dx)
                    { // <--- this "else" makes the difference
                        err += dx;
                        y1 += sy;
                    }
                }
                return count;
            }
        }
    }
}
