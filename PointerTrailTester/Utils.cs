using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace au.edu.federation.PointerTrailTester
{
    static class Utils
    {
        // Kernel hook to read process memory. Note: Even on 64-bit systems the kernel is called kernel32.
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        
        // Find the process in the processName property and set the processBaseAddress property ready for use
        public static int findProcessBaseAddress(string processName)
        {
            Process[] processArray = Process.GetProcessesByName(processName);
            
            if (processArray.Length > 0)
            {
                // Sleep before returning process base address (prevents crashing when we only just found the process but the base address hasn't been fully established yet)
                System.Threading.Thread.Sleep(1000);
                return processArray[0].MainModule.BaseAddress.ToInt32();
            }

            return 0;
        }

        // Take base address and a list of hex values (as strings) and return the final feature address
        //protected int findFeatureAddress(int baseAddress, List<string> hexPointerTrail)
        // Take base address and a list of hex values (as strings) and return the final feature address
        public static int findFeatureAddress(int processHandle, int baseAddress, List<string> hexPointerTrail)
        {
            // Our final address will change as this method runs, but we start at the base address
            int featureAddress = baseAddress;

            // Follow the pointer trail to find the final address of the feature
            // Note: If we remove the "minus one" part of the below loop we get the ACTUAL value of that feature (assuming it's an int like the clock)
            int offset = 0;
            for (int loop = 0; loop < hexPointerTrail.Count; loop++)
            {
                // Get our offset string as an int
                try
                {
                    offset = Convert.ToInt32(hexPointerTrail.ElementAt(loop), 16);
                }
                catch (FormatException)
                {
                    Program.validPointerTrail = false;
                    return 0;
                }

                // Apply the offset
                featureAddress += offset;

                if (loop == (hexPointerTrail.Count - 1))
                {
                    break;
                }

                // Read the address at that offset
                featureAddress = getIntFromAddress(processHandle, featureAddress);
            }

            // Set the validPointerTrail flag to false if it was empty, or true if it made its way through the above without hitting the FormatException
            if (hexPointerTrail.Count == 0)
            {
                Program.validPointerTrail = false;
            }
            else
            {
                Program.validPointerTrail = true;
            }

            return featureAddress;
        }

        // Add a hex value, specified as a string, to the pointer trail
        public static bool addHexValueToPointerTrail(List<string> pointerList, string hexValue)
        {
            int i = 0;
            if (int.TryParse(hexValue, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out i))
            {
                pointerList.Add(hexValue);
                return true;
            }

            // Not a valid string representation of a hexadecimal number? Fail!
            return false;
        }

        // Read and return an int
        public static Int32 getIntFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;            
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt32(buf, 0);
        }

        // Read and return a short
        public static Int16 getShortFromAddress(int processHandle, int address)
        {   
            int bytesRead = 0;
            byte[] buf = new byte[2];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt16(buf, 0);
        }

        // Read and return a long
        public static Int64 getLongFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt64(buf, 0);
        }

        // Read and return a float
        public static float getFloatFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToSingle(buf, 0);
        }

        // Read and return a double
        public static double getDoubleFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);

            return BitConverter.ToDouble(buf, 0);
        }

        // Read and return a bool
        public static Boolean getBoolFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToBoolean(buf, 0);
        }

        // Read and return a UTF-8 formatted string
        public static string getUTF8FromAddress(int processHandle, int address)
        {
            int bytesRead = 0;

            // We'll read one UTF-16 character at a time
            byte[] buf = new byte[1];

            // We'll keep a char count to abort after a set number of chars if bad things happen
            int charCount = 0;

            string s = "";
            do
            {
                // Reset how many bytes we've read then read 2 bytes of data
                bytesRead = 0;
                ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);

                // We do NOT want the null character on the end of your returned string - so if we find it we bail before adding
                if (buf[0] == 0)
                {
                    break;
                }

                // (Implied else) Add the UTF-16 representation of the 2-bytes to our string
                s += System.Text.Encoding.Unicode.GetString(buf);

                // Move along by 2 bytes (text being read is UTF16)
                address += 1;

            } while (!((buf[0] == 0) || (++charCount >= Program.TEXT_COMPARISON_CHAR_LIMIT))); // Quit when we read a null-terminator [00] or hit 33 chars

            // Return a version of the read string with trailing spaces trimmed so the user does not have to add trailing spaces to their match criteria (which would be ugly - espcially for non-sighted users).
            return s.TrimEnd(); // Still trim spaces at end.
        }

        // Read and return a UTF-16 formatted string
        public static string getUTF16FromAddress(int processHandle, int address)
        {
            int bytesRead = 0;

            // We'll read one UTF-16 character at a time
            byte[] buf = new byte[2];

            // We'll keep a char count to abort after a set number of chars if bad things happen
            int charCount = 0;

            string s = "";
            do
            {
                // Reset how many bytes we've read then read 2 bytes of data
                bytesRead = 0;
                ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);

                // We do NOT want the null character on the end of your returned string - so if we find it we bail before adding
                if (buf[0] == 0 && buf[1] == 0)
                {
                    break;
                }

                // (Implied else) Add the UTF-16 representation of the 2-bytes to our string
                s += System.Text.Encoding.Unicode.GetString(buf);

                // Move along by 2 bytes (text being read is UTF16)
                address += 2;

            } while (!((buf[0] == 0 && buf[1] == 0) || (++charCount >= Program.TEXT_COMPARISON_CHAR_LIMIT))); // Quit when we read a null-terminator [00] or hit 33 chars

            // Return a version of the read string with trailing spaces trimmed so the user does not have to add trailing spaces to their match criteria (which would be ugly - espcially for non-sighted users).
            return s.TrimEnd(); // Still trim spaces at end.
        }

        // Return a value of a given type from the feature address
        public static dynamic getDynamicValueFromType(Program.ValueType valueType)
        {
            switch (valueType)
            {
                case Program.ValueType.IntType:
                    return (Int32)Utils.getIntFromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.ShortType:
                    return (Int16)Utils.getShortFromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.LongType:
                    return (Int64)Utils.getShortFromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.FloatType:
                    return (float)Utils.getFloatFromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.DoubleType:
                    return (double)Utils.getDoubleFromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.BoolType:
                    return (bool)Utils.getBoolFromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.StringUTF8Type:
                    return (string)Utils.getUTF8FromAddress(Program.processHandle, Program.featureAddress);

                case Program.ValueType.StringUTF16Type:
                    return (string)Utils.getUTF16FromAddress(Program.processHandle, Program.featureAddress);

                default:
                    MessageBox.Show("Value type in getDynamicValueFromType not recognised. Value type we got was: " + valueType.ToString());
                    return null;
            }
        }

        // Convert a comma separated list of pointer trails into a list
        public static List<string> CommaSeparatedStringToStringList(string s)
        {
            // Return a list of string items split on commas with whitespaces removed and no 'blank' entries (i.e. ",   ,")
            return s.Split(',')
                   .Select(x => x.Trim())
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .ToList();
        }
        
    } // End of Utils class

} // End of namespace
