using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    interface Mutator<T> where T:Chromosome
    {
        T Mutate(T t);
    }

    class SimpleMutator : Mutator<Oddajnik>
    {
        double probabilityToChangeSize = 0.5;
        int MaxPower;
        int MaxSize;

        public SimpleMutator(int maxPower,int maxSize, double probChangeSize = 0.5)
        {
            this.MaxPower = maxPower;
            this.probabilityToChangeSize = probChangeSize;
            this.MaxSize = maxSize;
        }

        public Oddajnik Mutate(Oddajnik o)
        {
            double mode = Utility.Random.Next();

            if (mode < probabilityToChangeSize / 2 && o.Value.Length/3 > 1)
            {
                int pos = Utility.Random.Next(o.Value.Length / 3) * 3;
                var values = o.Value.Take(pos).ToList();
                values.AddRange(o.Value.Skip(pos + 3));
                return new Oddajnik(values.ToArray());
            }
            else if (mode < probabilityToChangeSize && o.Value.Length / 3 < MaxSize+1)
            {
                var values = o.Value.ToList();

                int x = Utility.Random.Next(o.Value.Length);
                int y = Utility.Random.Next(o.Value.Length);
                int power = Utility.Random.Next(MaxPower);
                values.AddRange(new int[] { x, y, power });
                return new Oddajnik(values.ToArray());
            }
            else
            {
                int pos = Utility.Random.Next(o.Value.Length / 3) * 3;

                int x = Utility.Random.Next(o.Value.Length);
                int y = Utility.Random.Next(o.Value.Length);
                int power = Utility.Random.Next(MaxPower);

                int[] arr = (int[])o.Value.Clone();
                arr[pos] = x;
                arr[pos + 1] = y;
                arr[pos + 2] = power;

                return new Oddajnik(arr);
            }
        }
    }
}
