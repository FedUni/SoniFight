using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace au.edu.federation.PointerChainTester
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

        public static string processName;        // The name of the process we want to attach to
        public static Process[] processArray;    // Used to find the process
        public static Process gameProcess;       // The actual process we found

        // Note: While integers in .NET are always 32-bit, IntPtr can be used to be flexible with the build platform - so when building
        //       as an x86 app an IntPtr is 32-bit but when building as an x64 app an IntPtr is 64-bits.

        public static IntPtr processHandle;      // The handle to the process we found
        public static IntPtr processBaseAddress; // The base address of the process we found
        public static IntPtr featureAddress;     // The feature address after following the pointer trail

        public static bool is64Bit;              // Flag to keep track of whether we're running as a 32-bit or 64-bit process

        // The pointer trail to find our feature address as a list of strings
        public static List<string> pointerList = new List<string>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set our 64-bit flag depending on whether this is the 32-bit or 64-bit build of the pointer chain tester
            if (System.Environment.Is64BitProcess)
            {
                is64Bit = true;
            }
            else
            {
                is64Bit = false;
            }

            // Localisation test code - uncomment to force French localisation etc.
            /*CultureInfo cultureOverride = new CultureInfo("fr");
            Thread.CurrentThread.CurrentUICulture = cultureOverride;
            Thread.CurrentThread.CurrentCulture = cultureOverride;*/

            // Kick off the form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            // Cancel and dispose of the background worker that connects to the requested process
            Form1.processConnectionBW.CancelAsync();
            Form1.processConnectionBW.Dispose();
        }

    } // End of Program class

} // End of namespace
