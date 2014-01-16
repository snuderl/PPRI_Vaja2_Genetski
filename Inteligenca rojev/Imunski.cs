using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsControlLibrary1;

namespace Inteligenca_rojev
{
    class Imunski
    {
        int dolzina;
        int stVzorcev;
        Stavba stavba;
        double[][] powerMap;
        Random rand = new Random();
        public List<Point[]> samples = new List<Point[]>();
        public List<int[]> vectors = new List<int[]>();
        public List<Point[]> antigens = new List<Point[]>();
        public List<int[]> antiVectors = new List<int[]>();
        public List<double[]> afiniteta = new List<double[]>();
        public double Threshold = 0.5;
        List<Tuple<int, int, double>> oddajniki;

        public Imunski(Stavba s, double[][] powerMap, List<Tuple<int, int, double>> oddajniki, int dolzina, int stVzorcev)
        {
            this.stavba = s;
            this.powerMap = powerMap;
            this.dolzina = dolzina;
            this.stVzorcev = stVzorcev;
            this.oddajniki = oddajniki;

            for (var i = 0; i < stVzorcev; i++)
            {
                var path = RandomPath(dolzina, s.exits[0], s.exits[1]);
                Console.WriteLine(path.Length);
                var vector = pathToVector(oddajniki, path);
                Console.WriteLine(vector.Length);

                samples.Add(path);
                vectors.Add(vector);

            }
        }

        public void TvoriAntigen(double treshold)
        {
            this.Threshold = treshold;
            var path = RandomPath(dolzina * 2, stavba.exits[0], stavba.exits[1]);
            var vector = pathToVector(oddajniki, path);
            var aff = Enumerable.Range(0, vectors.Count).Select(x => Affinity(vector, vectors[x])).ToArray();
            antigens.Add(path);
            antiVectors.Add(vector);
            afiniteta.Add(aff);
        }

        public double Affinity(int[] vector, int[] vector2)
        {
            ///Evklid
            var sum = 0.0;
            for (var i = 0; i < vector2.Length; i++)
            {
                sum += Math.Pow(vector[i] - vector2[i], 2);
            }
            return Math.Sqrt(sum);
        }

        public int[] pathToVector(List<Tuple<int, int, double>> oddajniki, Point[] path)
        {
            int[] vector = new int[oddajniki.Count];

            short[][] map = new short[stavba.Rows][];
            for (var i = 0; i < stavba.Rows; i++)
            {
                map[i] = new short[stavba.Cols];
            }

            for (var i = 0; i < path.Length; i++)
            {
                map[path[i].X][path[i].Y] = 1;
            }

            for (var i = 0; i < oddajniki.Count; i++)
            {
                var oddajnik = oddajniki[i];
                var range = Fitness.C * Math.Sqrt(oddajnik.Item3);

                var d = Convert.ToInt32(range + 0.5);
                for (var x = Math.Max(0, oddajnik.Item1 - d); x < Math.Min(oddajnik.Item1 + d, stavba.Rows); x++)
                {
                    for (var y = Math.Max(0, oddajnik.Item2 - d); y < Math.Min(oddajnik.Item2 + d, stavba.Cols); y++)
                    {
                        double distance = Math.Sqrt(Math.Pow(x - oddajnik.Item1, 2) + Math.Pow(y - oddajnik.Item2, 2));
                        if (distance < range && map[x][y] == 1)
                        {
                            vector[i]++;
                        }
                    }
                }
            }

            return vector;
        }


        public Point[] RandomPath(int maxSize, Point start, Point end)
        {
            Point[] path = new Point[maxSize + 1];
            path[0] = start;
            while (true)
            {
                HashSet<Point> visited = new HashSet<Point>();
                Point[] moves = new Point[4];

                var position = start;
                var ind = 1;
                while (ind < maxSize)
                {
                    visited.Add(position);
                    int n;
                    var possibleMoves = GenerateMoves(position, moves, stavba, visited, out n);
                    if (n == 0)
                    {
                        break;
                    }

                    position = possibleMoves[rand.Next(n)];
                    path[ind++]= position;

                    if (position.X == end.X && position.Y == end.Y)
                    {
                        return path.Take(ind).ToArray();
                    }
                }
            }
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
                moves[n++] = (new Point(x, y - 1));
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
