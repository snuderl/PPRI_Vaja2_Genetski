using MB.Algodat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    interface Selection<T> where T : Chromosome
    {
        int Select(Generation<T> pop);
    }

    class TournamenetSelection<T> : Selection<T> where T : Chromosome
    {
        int k = 2;
        public TournamenetSelection(int k)
        {
            this.k = k;
        }
        public int Select(Generation<T> gen)
        {
            double totalFitnes = gen.Sum;
            double n = Utility.Random.NextDouble() * totalFitnes;
            double s = 0;

            for (var i = 0; i < gen.members.Length; i++)
            {
                if (n > s && n < s + gen.members[i].Fitness)
                {
                    return i;
                }
                s += gen.members[i].Fitness;
            }
            return gen.members.Length - 1;
        }
    }

    class RouletteSelection<T> : Selection<T> where T: Chromosome
    {

        public int Select(Generation<T> gen)
        {
            double totalFitnes = gen.Sum;
            double n = Utility.Random.NextDouble() * totalFitnes;
            double s = 0;

            for (var i = 0; i < gen.members.Length; i++)
            {
                s += gen.members[i].Fitness;
                if (s > n)
                {
                    return i;
                }
            }
            return gen.members.Length - 1;
        }
    }
}
