using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    class Generation<T> where T : Chromosome
    {
        public T[] members;
        public int Number;

        public double AverageFitness { get; protected set; }
        public double MaxFitness { get; protected set; }
        public double WorstFitness { get; protected set; }
        public double Sum { get; protected set; }

        public Generation(T[] l, int num)
        {
            this.members = l;
            this.Number = num;
            this.Sum = l.Sum(x => x.Fitness);

            AverageFitness =  Sum /  members.Length;
            MaxFitness = l.Max(x => x.Fitness);
            WorstFitness = l.Min(x => x.Fitness);
        }

        public override string ToString()
        {
            return String.Format("Generation {0}, Max: {1}, Average: {2}, Worst: {3}", Number, MaxFitness, AverageFitness, WorstFitness);
        }


    }
}
