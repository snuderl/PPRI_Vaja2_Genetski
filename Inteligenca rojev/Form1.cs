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

            PSO pso = new PSO(100, userControl11.stavba, 10);
            pso.Run(50);


            Console.WriteLine("Done");

        }




    }
}
