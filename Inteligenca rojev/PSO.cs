using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsControlLibrary1;

namespace Inteligenca_rojev
{
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
        public static Random rand = new Random();
        public int ConvergenceIterations = 10;

        public short[][] CreateTopology(String topologija){

            topology = new short[numParticles][];
            if (topologija == "Polna")
            {
                for (var i = 0; i < numParticles; i++)
                {
                    topology[i] = Enumerable.Range(0, numParticles).Select(x => (short)1).ToArray();
                }
            }
            else if (topologija == "Krog")
            {
                for (var i = 0; i < numParticles; i++)
                {
                    topology[i] = Enumerable.Range(0, numParticles).Select(x => (short)0).ToArray();
                    if (i == numParticles - 1)
                    {
                        topology[i][0] = 1;
                    }
                    else
                    {
                        topology[i][i + 1] = 1;
                    }
                }
            }

            return topology;
        }

        public PSO(int count, Stavba stavba, int NumOddajnikov, double maxPower, String topologija)
        {
            this.stavba = stavba;
            numParticles = count;
            this.MaxPower = maxPower;



            this.topology = CreateTopology(topologija);
            particles = new Particle[numParticles];            
            for (var i = 0; i < numParticles; i++)
            {
                particles[i] = new Particle(generateRandomData(stavba, NumOddajnikov));
            }

            r1 = new double[NumOddajnikov * 3];
            r2 = new double[NumOddajnikov * 3];
            for (var i = 0; i < r1.Length; i++)
            {
                r1[i] = rand.NextDouble();
                r2[i] = rand.NextDouble();
            }
        }

        public static double Clamp(double val, double min, double max)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        public void Run(int iterations, Action<IEnumerable<Tuple<double,double,double>>> onUpdate, Action onFinish, UserControl1 uc)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() =>
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
                        particle.Update(c1, c2, r1, r2, stavba);
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
                    this.scores.Add(new Tuple<double, double, double>(best, average, min));
                    if (iteration % 10 == 0)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            onUpdate(scores);
                            Particle b = particles.Where(x => x.score == particles.Max(z => z.score)).FirstOrDefault();
                            uc.SetExtra(b.state.Where(x => x.power > 0).Select(x => new Tuple<int, int, double>(x.X, x.Y, x.power)).ToList());

                            var m = Fitness.generatePowerMap(uc.stavba);
                            Fitness.CalculatePower(stavba, b.state, m);
                            uc.SetPowerMap(m);


                        }, CancellationToken.None, TaskCreationOptions.AttachedToParent, context);
                    }


                    iteration++;
                    Console.WriteLine("Iteration " + iteration + ", Best: " + best);
                    if (HasConverged())
                    {
                        Console.WriteLine("Converged");
                        break;
                    }
                }

                Task.Factory.StartNew(() =>
                onFinish()
                , CancellationToken.None, TaskCreationOptions.AttachedToParent, context);
            });
        }

        public bool HasConverged()
        {
            int N = ConvergenceIterations;
            if (scores.Count < N + 1)
            {
                return false;
            }
            else
            {
                var lastN = scores.Skip(scores.Count + 1 - (N));
                var last = lastN.Last();
                return lastN.Take(N).All(x => last.Item1 <= x.Item1);
            }
        }

        public static List<Oddajnik> generateRandomData(Stavba s, int count, double maxPower = 10)
        {
            HashSet<Point> hash = new HashSet<Point>();
            var oddajniki = new List<Oddajnik>();


            while (hash.Count < count)
            {
                int x = rand.Next(s.Rows);
                int y = rand.Next(s.Cols);
                var tuple = new Point(x, y);
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
}
