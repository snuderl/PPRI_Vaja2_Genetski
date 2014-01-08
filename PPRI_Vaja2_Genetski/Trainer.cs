using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    abstract class Trainer<T> where T : Chromosome
    {
        public Selection<T> SelectionAlgorithm;
        public Crossover<T> CrossoverAlgoirthm;
        public Mutator<T> MutatorAlgorithm;
        abstract public IEnumerable<T> Random();
        abstract public bool IsValid(T val);
        abstract public double Evaluate(T val);
    }

    class Pokritje : Trainer<Oddajnik>
    {
        public int Velikost { get; protected set; }
        public int MaxPower = 10;
        public int ChromosomeMinSize;
        public int ChromosomeMaxSize;
        public double C;
        public int MaxTotalPower;

        public Boolean[][] stavba;

        private Random rnd = new Random();

        public Pokritje(Boolean[][] stavba, int chromosomeMin, int chromosomeMax, int maxPower, double c, int maxTotalPower)
        {
            this.ChromosomeMaxSize = chromosomeMax;
            this.ChromosomeMinSize = chromosomeMin;
            this.Velikost = stavba.Length;
            this.stavba = stavba;
            this.MaxPower = maxPower;
            this.C = c;
            this.MaxTotalPower = maxTotalPower;


            this.SelectionAlgorithm = new RouletteSelection<Oddajnik>();
            this.CrossoverAlgoirthm = new SimpleCrossover();
            this.MutatorAlgorithm = new SimpleMutator(maxPower, ChromosomeMaxSize);
        }





        override public IEnumerable<Oddajnik> Random()
        {
            while (true)
            {
                Oddajnik od;
                do
                {
                    var size = Utility.Random.Next(ChromosomeMinSize, ChromosomeMaxSize + 1);
                    var l = new int[size * 3];
                    for (var i = 0; i < l.Length; i += 3)
                    {
                        var row = rnd.Next(0, Velikost);
                        var col = rnd.Next(0, Velikost);

                        var power = rnd.Next(1, MaxPower);
                        l[i] = row;
                        l[i + 1] = col;
                        l[i + 2] = power;
                    }
                    od = new Oddajnik(l);
                } while (!IsValid(od));
                yield return od;
            }
        }

        override public double Evaluate(Oddajnik t)
        {
            if (!t.Evaluated)
            {
                t.SetFitness(EvaluateEager(t));
            }
            return t.Fitness;
        }

        public double EvaluateEager(Oddajnik t, HashSet<Tuple<int,int>> set = null)
        {
            if (set == null)
            {
                set = new HashSet<Tuple<int, int>>();
            }
            var power = 0;
            for(var z = 0; z < t.Value.Length; z+=3)
            {
                var row = t.Value[z];
                var col= t.Value[z+1];
                var pow= t.Value[z+2];
                power += pow;

                var range = Math.Sqrt(pow);
                var up = Math.Ceiling(range) + 1;
                var down = Math.Ceiling(range);

                var colMin = (int)Math.Max(0, col - down);
                var colMax = Math.Min(col + up, stavba.Length);

                for (var i = (int)Math.Max(0, row - down); i < Math.Min(row + up, stavba.Length); i++)
                {
                    for (var y = colMin; y < colMax; y++)
                    {
                        if (!stavba[i][y] && DistanceSquared(i - row, y - col) <= pow)
                        {
                            set.Add(new Tuple<int, int>(i, y));
                        }
                    }
                }

            }
            var fitness = Math.Pow(set.Count,C) / power;
            t.Pokrtitih = set.Count;
            return fitness;
        }

        private double DistanceSquared(int x1, int x2)
        {
            return (x1 * x1 + x2 * x2);
        }

        override public bool IsValid(Oddajnik t)
        {
            if (t.Power > MaxTotalPower)
            {
                return false;
            }

            if (t.Value.Length / 3 < ChromosomeMinSize || t.Value.Length / 3 > ChromosomeMaxSize)
            {
                return false;
            }

            HashSet<Tuple<int, int>> locations = new HashSet<Tuple<int, int>>();
            for(var i = 0; i < t.Value.Length; i+=3)
            {
                var Item1 = t.Value[i];
                var Item2 = t.Value[i+1];
                var Item3 = t.Value[i+2];
                var loc = new Tuple<int, int>(Item1, Item2);
                if (locations.Contains(loc))
                    return false;
                locations.Add(loc);


                if (!(Item1 >= 0 && Item1 < Velikost))
                {
                    return false;
                }
                if (!(Item2 >= 0 && Item2 < Velikost))
                {
                    return false;
                }

                if (!(Item3 > 0))
                {
                    return false;
                }

                if (stavba[Item1][Item2])
                {
                    return false;
                }
            }



            return true;
        }



    }
}
