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
            string Agss = "";
            for(int i = 0; i < Ags.Length; i++)
            {
                Agss += Ags[i] + "|";
            }
            Application.Run(new Form1(Agss));
        }
    }
}
