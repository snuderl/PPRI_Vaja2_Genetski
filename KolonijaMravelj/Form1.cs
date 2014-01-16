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
            var feromon = Mravlje.initializeFeromoneMap(stavba);

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


    }
}
