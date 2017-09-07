using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace au.edu.federation.PointerTrailTester
{
    static class Program
    {
        // Enum of different types of destination values we can read.
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

        // Maximum characters to read from string types
        public static int TEXT_COMPARISON_CHAR_LIMIT = 33;

        public static bool connectedToProcess = false; // Keep track of whether we're connected to the named process or not
        public static bool validPointerTrail  = false; // Keep track of whether the pointer trail is legal or not

        public static string processName;      // The name of the process we want to attach to
        public static Process[] processArray;  // Used to find the process
        public static Process gameProcess;     // The actual process we found
        public static int processHandle;       // The handle to the process we found
        public static int processBaseAddress;  // The base address of the process we found
        public static int featureAddress;      // The feature address after following the pointer trail

        // The pointer trail to find our feature address as a list of strings
        public static List<string> pointerList = new List<string>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            // Cancel and dispose of the background worker that connects to the requested process
            Form1.processConnectionBW.CancelAsync();
            Form1.processConnectionBW.Dispose();
        }
    }
}
