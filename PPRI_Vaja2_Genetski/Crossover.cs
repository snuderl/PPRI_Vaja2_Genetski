using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    interface Crossover<T> where T : Chromosome
    {
        List<T> Crossover(T p1, T p2);
    }

    class SimpleCrossover : Crossover<Oddajnik>
    {
        public List<Oddajnik> Crossover(Oddajnik p1, Oddajnik p2)
        {
            List<Oddajnik> result = new List<Oddajnik>();
            int[] parent1 = (int[])p1.Value.Clone();
            int[] parent2 = (int[])p2.Value.Clone();

            ShuffleIndexes(parent1);
            ShuffleIndexes(parent2);

            var krajsi = Math.Min(parent1.Length, parent2.Length);

            int n = Utility.Random.Next(krajsi/3)*3;

            for (var i = n; i < krajsi; i++)
            {
                var tmp = parent1[i];
                parent1[i] = parent2[i];
                parent2[i] = tmp;
            }

            result.Add(new Oddajnik(parent1));
            result.Add(new Oddajnik(parent2));

            return result;
        }

        private void ShuffleIndexes(int[] arr)
        {
            for (int i = 0; i < arr.Length; i+=3)
            {
                int r = Utility.Random.Next(i/3, arr.Length/3)*3;
                int tmp = arr[r];
                int tmp1 = arr[r + 1];
                int tmp3 = arr[r+2];
                arr[r] = arr[i];
                arr[i] = tmp;
                arr[r+1] = arr[i+1];
                arr[i+1] = tmp1;
                arr[r+2] = arr[i+2];
                arr[i+2] = tmp3;
            }
        }
    }
}
