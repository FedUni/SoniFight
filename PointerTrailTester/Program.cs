using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointerTrailTester
{
    static class Program
    {
        // Maximum characters to compare when doing string comparisons
        public static int TEXT_COMPARISON_CHAR_LIMIT = 33;

        public static string processName;
        public static string pointerTrail;
        public static ValueType valueType;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
