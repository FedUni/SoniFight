using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

using au.edu.federation.PointerChainTester.Properties;

namespace au.edu.federation.PointerChainTester
{
    static class Utils
    {
        // Kernel hook to read process memory. Note: Even on 64-bit systems the kernel is called kernel32.
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        // Find the process in the processName property and set the processBaseAddress property ready for use
        public static IntPtr findProcessBaseAddress(string processName)
        {
            Process[] processArray = Process.GetProcessesByName(processName);

            if (processArray.Length > 0)
            {
                // Sleep before returning process base address (prevents crashing when we only just found the process but the base address hasn't been fully established yet)
                System.Threading.Thread.Sleep(1000);
                return processArray[0].MainModule.BaseAddress;
            }

            return (IntPtr)0;
        }
                
        // Take base address and a list of hex values (as strings) and return the final feature address
        public static IntPtr findFeatureAddress(IntPtr processHandle, IntPtr baseAddress, List<string> hexPointerTrail)
        {
            // Our final address will change as this method runs, but we start at the base address
            IntPtr featureAddress = baseAddress;

            // Follow the pointer trail to find the final address of the feature
            // Note: If we remove the "minus one" part of the below loop we get the ACTUAL value of that feature (assuming it's an int like the clock)
            IntPtr offset = (IntPtr)0;
            for (int loop = 0; loop < hexPointerTrail.Count; ++loop)
            {
                // Get our offset string as an int
                try
                {
                    if (Program.is64Bit)
                    {
                        offset = (IntPtr)Convert.ToInt64(hexPointerTrail.ElementAt(loop), 16);
                    }
                    else
                    {
                        offset = (IntPtr)Convert.ToInt32(hexPointerTrail.ElementAt(loop), 16);
                    }
                }
                catch (FormatException)
                {
                    Program.validPointerTrail = false;
                    return (IntPtr)0;
                }
                catch (OverflowException oe)
                {
                    Program.validPointerTrail = false;
                    MessageBox.Show(Resources.ResourceManager.GetString("overflowExceptionString") + oe.Message);
                    return (IntPtr)0;
                }

                // Apply the offset
                if (Program.is64Bit)
                {
                    long featureAddressLong = featureAddress.ToInt64();
                    long offsetLong = (long)offset; // I genuinely don't know why the cast to long works but converting ToInt64 doesn't - but that's how it is.
                    featureAddress = new IntPtr(featureAddressLong + offsetLong);
                }
                else
                {
                    int featureAddressInt = featureAddress.ToInt32();
                    int offsetInt = (int)offset; // Should cast instead of convert here as well?
                    featureAddress = new IntPtr(featureAddressInt + offsetInt);
                }
                
                // At the last value? Then our feature address has been found and we can exit the loop
                if (loop == (hexPointerTrail.Count - 1))
                {
                    break;
                }

                // Read the address at that offset, grabbing a long if we're running in 64-bit mode and an int if we're in 32-bit mode
                if (Program.is64Bit)
                {
                    featureAddress = (IntPtr)getLongFromAddress(processHandle, featureAddress);
                }
                else
                {
                    featureAddress = (IntPtr)getIntFromAddress(processHandle, featureAddress);
                }

            } // End of loop over pointer hops

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
        public static Int32 getIntFromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt32(buf, 0);
        }

        // Read and return a short
        public static Int16 getShortFromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[2];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt16(buf, 0);
        }

        // Read and return a long
        public static Int64 getLongFromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt64(buf, 0);
        }

        // Read and return a float
        public static float getFloatFromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToSingle(buf, 0);
        }

        // Read and return a double
        public static double getDoubleFromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);

            return BitConverter.ToDouble(buf, 0);
        }

        // Read and return a bool
        public static Boolean getBoolFromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToBoolean(buf, 0);
        }

        // Read and return a UTF-8 formatted string
        public static string getUTF8FromAddress(IntPtr processHandle, IntPtr address)
        {
            int bytesRead = 0;

            // We'll read one UTF-8 character at a time
            byte[] buf = new byte[1];

            // We'll keep a char count to abort after a set number of chars if bad things happen
            int charCount = 0;

            string s = "";
            do
            {
                // Reset how many bytes we've read then read 1 byte of data
                bytesRead = 0;
                ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);

                // We do NOT want the null character on the end of your returned string - so if we find it we bail before adding
                if (buf[0] == 0)
                {
                    break;
                }

                // (Implied else) Add the UTF-8 representation of the byte to our string
                s += System.Text.Encoding.ASCII.GetString(buf);

                // Move along by 1 byte (text being read is UTF-8)
                address += 1;

            } while (!((buf[0] == 0) || (++charCount >= Program.TEXT_COMPARISON_CHAR_LIMIT))); // Quit when we read a null-terminator [00] or hit 33 chars

            // Return a version of the read string with trailing spaces trimmed so the user does not have to add trailing spaces to their match criteria (which would be ugly - espcially for non-sighted users).
            return s.TrimEnd(); // Still trim spaces at end.
        }

        // Read and return a UTF-16 formatted string
        public static string getUTF16FromAddress(IntPtr processHandle, IntPtr address)
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
