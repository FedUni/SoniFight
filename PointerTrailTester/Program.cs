using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointerTrailTester
{
    static class Program
    {
        // Enum of different types of destination values we can read
        public enum ValueType
        {
            IntType,
            ShortType,
            LongType,
            FloatType,
            DoubleType,
            BoolType,
            StringUTF8Type,
            StringUTF16Type,
        }

        // Maximum characters to compare when doing string comparisons
        public static int TEXT_COMPARISON_CHAR_LIMIT = 33;

        public static string processName;
        public static string pointerTrailString;
        public static ValueType valueType;
        public static List<string> pointerList = new List<string>();

        public static bool connectedToProcess = false; // Keep track of whether we're connected to the named process or not
        public static bool validPointerTrail  = false; // Keep track of whether the pointer trail is legal or not

        public static Process[] processArray;
        public static Process gameProcess;
        public static int processHandle;
        public static int processBaseAddress;
        public static int featureAddress;


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
