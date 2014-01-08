using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsControlLibrary1;

namespace Inteligenca_rojev
{



    public class PSO
    {

        struct Particle
        {
            public double bestScore;
            public double score;
            public double neighbourhoudScore;
            public Oddajnik[] state;

            public Particle(List<Oddajnik> oddajniki){
                bestScore = 0;
                score = 0;
                neighbourhoudScore = 0;
                state = oddajniki.ToArray();
            }
        }

        int numParticles;
        short[][] topology;
        Stavba stavba;
        Particle[] particles;
        List<double> scores = new List<double>();

        public PSO(int count, Stavba stavba, int NumOddajnikov)
        {
            this.stavba = stavba;
            numParticles = count;


            topology = new short[numParticles][];
            for (var i = 0; i < numParticles; i++)
            {
                topology[i] = Enumerable.Range(0, numParticles).Select(x => (short)1).ToArray();
            }

            particles = Enumerable.Range(0, numParticles).Select(x =>
            {
                return new Particle(generateRandomData(stavba, NumOddajnikov));
            }).ToArray();
        }

        public void Run(int iterations)
        {
            int iteration = 0;
            while (iteration < iterations && !HasConverged())
            {
                particles.ToList().AsParallel().ForAll(x => {
                    x.score = Fitness.CalculateFitness(stavba, x.state);
                    if (x.score > x.bestScore)
                    {
                        x.bestScore = x.score;
                    }
                });

                Enumerable.Range(0, numParticles).AsParallel().ForAll(x =>
                {
                    var neighbours = topology[x];
                    var networkBest = neighbours.Select((i, y) => i == 0 ? 0 : particles[y].score).Max();
                    particles[x].neighbourhoudScore = networkBest;
                });

                iteration++;
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
                return lastN.All(x => x >= last);
            }            
        }

        public List<Oddajnik> generateRandomData(Stavba s, int count, double maxPower = 10)
        {
            HashSet<Tuple<int, int>> hash = new HashSet<Tuple<int, int>>();
            var oddajniki = new List<Oddajnik>();

            var rand = new Random();

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


    public struct Oddajnik
    {
        public double power;
        public double x;
        public double y;

        public int X { get { return Convert.ToInt32(x); } }
        public int Y { get { return Convert.ToInt32(y); } }

        public Oddajnik(int x, int y, double power)
        {
            this.x = x;
            this.y = y;
            this.power = power;
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
                    count += stavba.lokacija[i][y1] == Lokacija.Zid ? 1 : 0;
                }
                return count;
            }

            else if (x1 == x2)
            {
                for (var i = Math.Min(y1, y2); i <= Math.Max(y1, y2); i++)
                {
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
