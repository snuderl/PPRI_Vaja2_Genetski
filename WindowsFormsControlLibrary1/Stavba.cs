using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsControlLibrary1
{
    public struct Stavba
    {
        public int Rows;
        public int Cols;
        public Lokacija[][] lokacija;
        public List<Tuple<int, int>> exits;
        public Stavba(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            this.lokacija = new Lokacija[rows][];
            for (var i = 0; i < rows; i++)
            {
                this.lokacija[i] = new Lokacija[cols];
                for (var y = 0; y < cols; y++)
                {
                    this.lokacija[i][y] = Lokacija.Prosto;
                }
            } 
            exits = new List<Tuple<int, int>>();
        }
    }
}
