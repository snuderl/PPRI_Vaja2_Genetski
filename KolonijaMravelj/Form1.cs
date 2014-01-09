using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsControlLibrary1;

namespace KolonijaMravelj
{
    public partial class Form1 : Form
    {
        Random rand = new Random();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var alfa = 0.8;
            var n = 5;
            var t = 1.2;
            var stavba = userControl11.stavba;
            var feromon = initializeMap(stavba);

            var paths = Enumerable.Range(0, n).Select(x =>
             FindPath(stavba, feromon, alfa, new Tuple<int, int>(4, 4), new Tuple<int, int>(0, 0))
            ).ToList();

            paths.ForEach(path =>
            {
                var score = path.Count / Math.Pow(path.Count, t);
                path.ForEach(index =>
                {

                });
                
            });
            var i = 0;
        }

        public double[][] initializeMap(Stavba s)
        {
            var map = new double[s.Rows][];
            for (var i = 0; i < map.Length; i++)
            {
                map[i] = Enumerable.Range(0, s.Cols).Select(x => rand.NextDouble()).ToArray();
            }
            return map;
        }

        public List<Tuple<int, int>> FindPath(Stavba stavba, double[][] feromon, double alfa, Tuple<int,int> start, Tuple<int, int> goal)
        {
            while(true){
                HashSet<Tuple<int, int>> visited = new HashSet<Tuple<int, int>>();
                List<Tuple<int, int>> path = new List<Tuple<int, int>>();

                var position = start;
                path.Add(position);
                while (true)
                {
                    visited.Add(position);
                    var possibleMoves = GenerateMoves(position, stavba, visited);
                    if (possibleMoves.Count == 0)
                    {
                        //Console.WriteLine("Restarting");
                        break;
                    }
                    var move = PickMove(possibleMoves, feromon, alfa);
                    position = move;
                    path.Add(position);
                    if(position.Item1 == goal.Item1 && position.Item2 == goal.Item2){
                        return path;
                    }
                }
            }
        }

        public Tuple<int, int> PickMove(List<Tuple<int, int>> moves, double[][] feromon, double alfa)
        {
            var pheromones = moves.Select(x => feromon[x.Item1][x.Item2]).ToList();
            var sum = pheromones.Sum();
            double s = 0;
            double n = rand.NextDouble() * sum;

            for (var i = 0; i < moves.Count; i++)
            {
                if (n > s && n < s + pheromones[i])
                {
                    return moves[i];
                }
                s += pheromones[i];
            }
            return moves[pheromones.Count - 1];
        }

        public List<Tuple<int, int>> GenerateMoves(Tuple<int, int> position, Stavba s, HashSet<Tuple<int, int>> visited)
        {
            int x = position.Item1;
            int y = position.Item2;

            var moves = new List<Tuple<int, int>>();
            if (IsValid(x + 1, y, s, visited))
            {
                moves.Add(new Tuple<int, int>(x + 1, y));
            }
            if (IsValid(x - 1, y, s, visited))
            {
                moves.Add(new Tuple<int, int>(x - 1, y));
            } 
            if (IsValid(x, y  + 1, s, visited))
            {
                moves.Add(new Tuple<int, int>(x, y + 1));
            } 
            if (IsValid(x, y - 1, s, visited))
            {
                moves.Add(new Tuple<int, int>(x, y - 1));
            }
            return moves;
        }

        private Boolean IsValid(int x, int y, Stavba s, HashSet<Tuple<int, int>> visited)
        {
            return x >= 0 && y >= 0 && x < s.Rows && y < s.Cols && 
                s.lokacija[x][y] != Lokacija.Zid && 
                !visited.Contains(new Tuple<int, int>(x, y));
                 
        }
    }
}
