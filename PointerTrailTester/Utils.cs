using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PointerTrailTester
{
    static class Utils
    {
        // Kernel hooks to read and write process memory
        // Note: Even on 64-bit systems the kernel is called kernel32!
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        // Constants are implied static in C# - you cannot mark them as such.
        public const int MAX_STRING_LENGTH = 150;        

        // Range parsing code based on Chris Fazzio's code on StackOverflow. Dupe removal added by me. Source:
        // http://stackoverflow.com/questions/40161/does-c-sharp-have-built-in-support-for-parsing-page-number-strings
        public static int[] ParseIntRange(string ranges)
        {
            // Split on commas
            string[] groups = ranges.Split(',');

            // List may contain duplicates
            int[] naiveList = groups.SelectMany(t => GetIntRangeNumbers(t)).ToArray();

            // Use a LINQ query to return a list without dupes, sorted in descending order (high to low)
            return naiveList.Distinct().OrderByDescending(c => c).ToArray();
        }

        private static int[] GetIntRangeNumbers(string range)
        {
            int[] RangeNums = range
                .Split('-')
                .Select(t => new String(t.Where(Char.IsDigit).ToArray())) // Digits Only
                .Where(t => !string.IsNullOrWhiteSpace(t)) // Only if has a value
                .Select(t => int.Parse(t)) // digit to int
                .ToArray();
            return RangeNums.Length.Equals(2) ? Enumerable.Range(RangeNums.Min(), (RangeNums.Max() + 1) - RangeNums.Min()).ToArray() : RangeNums;
        }

        public static short[] ParseShortRange(string ranges)
        {
            int[] intRange = ParseIntRange(ranges);

            int elementCount = intRange.Length;

            short[] shortArray = new short[elementCount];

            for (int loop = 0; loop < elementCount; loop++)
            {
                shortArray[loop] = (short)(intRange[loop]);
                MessageBox.Show(shortArray[loop].ToString());
            }

            return shortArray;
        }

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
                catch (FormatException fe)
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


        public static Int32 getIntFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;            
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt32(buf, 0);
        }

        public static Int16 getShortFromAddress(int processHandle, int address)
        {   
            int bytesRead = 0;
            byte[] buf = new byte[2];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt16(buf, 0);
        }

        public static Int64 getLongFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt64(buf, 0);
        }

        public static float getFloatFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToSingle(buf, 0);
        }

        public static double getDoubleFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);

            return BitConverter.ToDouble(buf, 0);
        }

        public static Boolean getBoolFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToBoolean(buf, 0);
        }

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

        public static List<string> CommaSeparatedStringToStringList(string s)
        {
            // Return a list of string items split on commas with whitespaces removed and no 'blank' entries (i.e. ",   ,")
            return s.Split(',')
                   .Select(x => x.Trim())
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .ToList();
        }

        public static Program.ValueType GetValueTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Program.ValueType.IntType;
                case 1:
                    return Program.ValueType.ShortType;
                case 2:
                    return Program.ValueType.LongType;
                case 3:
                    return Program.ValueType.FloatType;
                case 4:
                    return Program.ValueType.DoubleType;
                case 5:
                    return Program.ValueType.BoolType;
                case 6:
                    return Program.ValueType.StringUTF8Type;
                case 7:
                    return Program.ValueType.StringUTF16Type;
                default:
                    return Program.ValueType.IntType;
            }

        } // End of GetValueTypeFromInt method

        public static int GetIntFromValueType(Program.ValueType vt)
        {
            switch (vt)
            {
                case Program.ValueType.IntType:
                    return 0;
                case Program.ValueType.ShortType:
                    return 1;
                case Program.ValueType.LongType:
                    return 2;
                case Program.ValueType.FloatType:
                    return 3;
                case Program.ValueType.DoubleType:
                    return 4;
                case Program.ValueType.BoolType:
                    return 5;
                case Program.ValueType.StringUTF8Type:
                    return 6;
                case Program.ValueType.StringUTF16Type:
                    return 7;
                default:
                    return 0;
            }

        } // End of GetIntFromValueType method

    } // End of Utils class

} // End of namespace
