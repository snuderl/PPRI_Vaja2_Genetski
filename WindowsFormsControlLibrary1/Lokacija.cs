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
    [Flags]
    public enum Lokacija
    {
        Prosto = 1,
        Zid = 2,
        Izhod = 4
    }
}
