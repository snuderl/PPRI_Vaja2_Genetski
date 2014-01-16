using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsControlLibrary1;

namespace Inteligenca_rojev
{
    class Mravlje
    {
        static Random rand = new Random();
        public static double[][] initializeFeromoneMap(Stavba s)
        {
            var map = new double[s.Rows][];
            for (var i = 0; i < map.Length; i++)
            {
                map[i] = Enumerable.Range(0, s.Cols).Select(x => rand.NextDouble()).ToArray();
            }
            return map;
        }

        public static void GetBestPath(Stavba stavba, double[][] feromon, double[][] powerMap, double t, double alfa, Point start, Point goal, UserControl1 uc, Action<List<Tuple<double,double,double>>> onProgress, int ants = 5, int iterations = 10, double p = 0.7)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() =>
            {
                var scoresList = new List<Tuple<double,double, double>>();
                var watch = new Stopwatch();
                watch.Start();
                List<Tuple<double, List<Point>>> scores = null;
                for (var i = 0; i < iterations; i++)
                {
                    var paths = Enumerable.Range(0, ants).Select(x =>
                         Mravlje.FindPath(stavba, feromon, alfa, start, goal)
                        ).ToList();


                    scores = paths.Select(path =>
                    {
                        var power = path.Select(x => powerMap[x.X][x.Y]).Sum();
                        var score = power / Math.Pow(path.Count, t);
                        return new Tuple<double, List<Point>>(score, path);

                    }).ToList();

                    for (var x = 0; x < stavba.Rows; x++)
                    {
                        for (var y = 0; y < stavba.Cols; y++)
                        {
                            feromon[x][y] *= (1 - p);
                        }
                    }

                    foreach (var tuple in scores)
                    {
                        foreach (var location in tuple.Item2)
                        {
                            feromon[location.X][location.Y] = tuple.Item1;
                        }
                    }

                        var best = scores.Max(x => x.Item1);
                        var avg = scores.Sum(x => x.Item1) / scores.Count;
                        var worst = scores.Min(x => x.Item1);

                        scoresList.Add(new Tuple<double,double,double>(best, avg, worst));




                    Console.WriteLine("Done iteration " + i + ", Best: " + scores.Max(x => x.Item1));
                    Console.WriteLine(paths.Average(x => x.Count));
                    Console.WriteLine(watch.ElapsedMilliseconds);


                    var bestItem = scores.Where(x => x.Item1 == scores.Max(y => y.Item1)).First().Item2;
                    Task.Factory.StartNew(() =>
                    {
                        uc.SetPath(bestItem);
                        onProgress(scoresList);
                    }, CancellationToken.None, TaskCreationOptions.AttachedToParent, context);
                }


                return scores.Where(x => x.Item1 == scores.Max(y => y.Item1)).First().Item2;
            });
        }

        public static List<Point> FindPath(Stavba stavba, double[][] feromon, double alfa, Point start, Point goal)
        {
            double[] pheromones = new double[4];
            Point[] moves = new Point[4];
            while (true)
            {
                HashSet<Point> visited = new HashSet<Point>();
                List<Point> path = new List<Point>();

                var position = start;
                path.Add(position);
                while (true)
                {
                    visited.Add(position);
                    int n;
                    var possibleMoves = GenerateMoves(position, moves, stavba, visited, out n);
                    if (n == 0)
                    {
                        //Console.WriteLine("Restarting");
                        break;
                    }
                    var move = PickMove(possibleMoves, pheromones, n, feromon, alfa);
                    position = move;
                    path.Add(position);
                    if (position.X == goal.X && position.Y == goal.Y)
                    {
                        return path;
                    }
                }
            }
        }

        public static Point PickMove(Point[] moves, double[] pheromones, int N, double[][] feromon, double alfa)
        {
            double sum = 0;
            for (var i = 0; i < N; i++)
            {
                var tuple = moves[i];
                var f = feromon[tuple.X][tuple.Y];
                pheromones[i] = f;
                sum += f;
            }
            double s = 0;
            double n = rand.NextDouble() * sum;

            for (var i = 0; i < N; i++)
            {
                if (n > s && n < s + pheromones[i])
                {
                    return moves[i];
                }
                s += pheromones[i];
            }
            return moves[N - 1];
        }

        public static Point[] GenerateMoves(Point position, Point[] moves, Stavba s, HashSet<Point> visited, out int n)
        {
            int x = position.X;
            int y = position.Y;

            n = 0;
            if (IsValid(x + 1, y, s, visited))
            {
                moves[n++] = new Point(x + 1, y);
            }
            if (IsValid(x - 1, y, s, visited))
            {
                moves[n++] = (new Point(x - 1, y));
            }
            if (IsValid(x, y + 1, s, visited))
            {
                moves[n++] = (new Point(x, y + 1));
            }
            if (IsValid(x, y - 1, s, visited))
            {
                moves[n++] =(new Point(x, y - 1));
            }
            return moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsValid(int x, int y, Stavba s, HashSet<Point> visited)
        {
            return x >= 0 && y >= 0 && x < s.Rows && y < s.Cols &&
                s.lokacija[x][y] != Lokacija.Zid &&
                !visited.Contains(new Point(x, y));

        }
    }
}
