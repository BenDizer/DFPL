using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DFPl
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] Ags)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(Ags));
        }
    }
}
