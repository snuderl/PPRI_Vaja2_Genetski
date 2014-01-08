using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPRI_Vaja2_Genetski
{
    abstract public class Chromosome
    {
        public double Fitness;
        public Boolean Evaluated { get; set; }

        public Chromosome()
        {
            this.Evaluated = false;
            this.Fitness = 0;
        }

        public void SetFitness(double v)
        {
            if (!Evaluated)
            {
                this.Fitness = v;
                this.Evaluated = true;
            }
            else
            {
                throw new Exception("Fitnes already exists");
            }
        }
    }

    public class Oddajnik : Chromosome
    {

        public int[] Value { get; protected set; }
        public int Power { get; protected set; }
        public int Pokrtitih;
        


        public Oddajnik(int[] val) : base()
        {
            this.Value = val;
            
            for (var i = 2; i < val.Length; i += 3)
            {
                Power += val[i];
            }
        }

        public IEnumerable<Tuple<int, int, int>> GetTuples()
        {
            int i = 0;
            while (i < Value.Length)
            {
                yield return new Tuple<int,int,int>(Value[i++], Value[i++], Value[i++]);
            }
        }

        public String GetValueString()
        {
            return String.Join(", ", Value);
        }

        public String[] GetStrings()
        {
            return new String[]{ GetValueString(), Fitness.ToString()};
        }


        public override string ToString()
        {
            return String.Format("Oddajnik:  [{0}], Fitness: {1}", 
                GetValueString(), Fitness);
        }


    }
}
