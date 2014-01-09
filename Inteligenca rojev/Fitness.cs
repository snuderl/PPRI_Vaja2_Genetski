using System;
using System.Collections.Generic;
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

        public void Update(double c1, double c2, double[] r1, double[] r2)
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
                state[i] += velocity[i];
            }

            this.state = Oddajnik.FromVector(state);
        }
    }

    public class PSO
    {


        

        int numParticles;
        short[][] topology;
        Stavba stavba;
        public Particle[] particles;
        public List<Tuple<double, double, double>> scores = new List<Tuple<double, double, double>>();
        public double c1 = 0.5, c2 = 0.7;
        public double MaxPower = 15;
        public double[] r1, r2;
        public Random rand = new Random();

        public PSO(int count, Stavba stavba, int NumOddajnikov)
        {
            this.stavba = stavba;
            numParticles = count;


            topology = new short[numParticles][];
            particles = new Particle[numParticles];
            for (var i = 0; i < numParticles; i++)
            {
                topology[i] = Enumerable.Range(0, numParticles).Select(x => (short)1).ToArray();
                particles[i] = new Particle(generateRandomData(stavba, NumOddajnikov));
            }

            r1 = new double[NumOddajnikov * 3];
            r2 = new double[NumOddajnikov * 3];
            for(var i = 0; i < r1.Length; i++){
                r1[i] = rand.NextDouble();
                r2[i] = rand.NextDouble();
            }
        }

        public static double Clamp(double val, double min, double max)
        {
            return Math.Max(min, Math.Min(max, val)); 
        }

        public void Run(int iterations)
        {
            int iteration = 0;
            while (iteration < iterations)
            {
                
                for (var i = 0; i < particles.Length; i++)
                {
                    var particle = particles[i];
                    var score = Fitness.CalculateFitness(stavba, particle.state);
                    particle.score = score;
                    if (score > particle.bestScore)
                    {
                        particle.bestScore = score;
                        particle.best = particle.state.ToList().ToArray();
                    }
                }

                for (var i = 0; i < particles.Length; i++)
                {
                    var neighboursInd = topology[i];
                    var particle = particles[i];
                    var neighbours = neighboursInd.Select((x, y) => x == 0 ? null : particles[y]).Where(x => x != null).ToList();
                    var networkBest = neighbours.Max(x => x.score);
                    particle.neighbourhoudScore = networkBest;
                    particle.neighbourhoudBest = neighbours.Where(x => x.score == networkBest).FirstOrDefault().state.ToList().ToArray();
                    particle.Update(c1, c2, r1, r2);
                    for (var z = 0; z < particle.state.Length; z++)
                    {
                        var p = particle.state[z];
                        p.power = Clamp(p.power, 0, MaxPower);
                        p.x = Clamp(p.x, 0, stavba.Rows - 1);
                        p.y = Clamp(p.y, 0, stavba.Cols - 1);
                        
                    }

                }

                var s = particles.Select(x => x.score).ToList();
                var best = s.Max();
                var average = s.Sum() / s.Count;
                var min = s.Min();
                scores.Add(new Tuple<double, double, double>(best, average, min));

                iteration++;
                if (HasConverged())
                {
                    Console.WriteLine("Converged");
                    //break;
                }
            }
        }

        public bool HasConverged()
        {
            if (scores.Count < 10)
            {
                return false;
            }
            else
            {
                int N = 10;
                var lastN = scores.Skip(scores.Count - (N + 1));
                var last = lastN.Last();
                return lastN.All(x => x.Item1 >= last.Item1);
            }            
        }

        public List<Oddajnik> generateRandomData(Stavba s, int count, double maxPower = 10)
        {
            HashSet<Tuple<int, int>> hash = new HashSet<Tuple<int, int>>();
            var oddajniki = new List<Oddajnik>();


            while (hash.Count < count)
            {
                int x = rand.Next(s.Rows);
                int y = rand.Next(s.Cols);
                var tuple = new Tuple<int, int>(x, y);
                if (s.lokacija[x][y] == Lokacija.Prosto && !hash.Contains(tuple))
                {
                    double power = rand.NextDouble() * maxPower + 1;
                    oddajniki.Add(new Oddajnik(x, y, power));
                    hash.Add(tuple);
                }
            }
            return oddajniki;
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
                var o = new Oddajnik(vector[i / 3], vector[i / 3 + 1], vector[i / 3 + 2]);
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
            CalculatePower(stavba, oddajniki, powerMap);

            var fitness = powerMap.SelectMany(x => x).Sum() - strosek * oddajniki.Length;
            return fitness;
        }

        public static void CalculatePower(Stavba stavba, Oddajnik[] oddajniki, double[][] powerMap)
        {
            List<double>[][] listPowerMap = new List<double>[stavba.Rows][];
            for (var i = 0; i < stavba.Rows; i++)
            {
                var row = Enumerable.Range(0, stavba.Cols).Select(x => new List<double>()).ToArray();
                listPowerMap[i] = row;
            }

            foreach (var p in oddajniki)
            {
                if (stavba.lokacija[p.X][p.Y] == Lokacija.Prosto)
                {
                    double range = C * Math.Sqrt(p.power);

                    var d = (int)(range + 0.5);

                    for (var x = Math.Max(0, p.X - d); x < Math.Min(stavba.Rows, p.X + d); x++)
                    {
                        for (var y = Math.Max(0, p.Y - d); y < Math.Min(stavba.Cols, p.Y + d); y++)
                        {
                            double distance = Distance(x + 0.5, y + 0.5, p.x + 0.5, p.y + 0.5);
                            if (distance < range)
                            {
                                var count = CalculateWallCount(stavba, p, x, y);
                                var neoslabljen = p.power * (1 - distance / (C * Math.Sqrt(p.power)));
                                var oslabljen = Math.Pow(q, count) * neoslabljen;
                                listPowerMap[x][y].Add(oslabljen);
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
                    if(x1 >= 0 && x1 < stavba.Rows && y1 >= 0 && y1 < stavba.Cols)
                        count += stavba.lokacija[x1][y1] == Lokacija.Zid ? 1 : 0;
                    //Console.WriteLine(x1 + "," + y1);

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
