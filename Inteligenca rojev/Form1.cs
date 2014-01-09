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

namespace Inteligenca_rojev
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(Fitness.KvadratnaMetodaDDA(1, 5, 2, 10, userControl11.stavba));

            PSO pso = new PSO(40, userControl11.stavba, 2);
            pso.Run(200);

            Particle best = pso.particles.Where(x => x.score == pso.particles.Max(z => z.score)).FirstOrDefault();
            userControl11.SetExtra(best.state.Select(x => new Tuple<int, int, double>(x.X, x.Y, x.power)).ToList());

            if (best.score > 50)
            {
                Console.WriteLine("Done " + best.score + " ," + best.state.Max(x => x.power));
                Console.WriteLine(pso.scores.Max(x => x.Item1));

            }
        }




    }
}
