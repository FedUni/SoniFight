using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using au.edu.federation.SoniFight.Properties;

namespace au.edu.federation.SoniFight
{
    static class Utils
    {
        // Kernel hook to read process memory
        // Note: Even on 64-bit systems the kernel is called kernel32!
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        // Constants are implied static in C# - you cannot mark them as such.
        public const int MAX_STRING_LENGTH = 150;

        /* Note: Writing and reading objects to XML code taken from: http://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/ */

        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);

                Console.WriteLine( Resources.ResourceManager.GetString("fileSavedSuccessfullyString") + filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ResourceManager.GetString("fileSaveFailedString") + e.Message);
                Console.WriteLine(e.InnerException);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;

            T obj = default(T);
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                obj = (T)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {                
                Console.WriteLine(Resources.ResourceManager.GetString("deserialiseFailedString") + e.Message);
                Console.WriteLine(e.InnerException);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return obj;

        } // End of ReadFromXmlFile method

        // NOTE: This method is not used at the current time, but may come in useful in the future so leaving alone.
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

        // Method to generate a range of numbers in an array similar to how pages to print are specified, e.g. 1, 3-5 would print pages 1, 3, 4 and 5.
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

        // ---------- TreeNode manipulation methods ---------

        // Flatten a TreeView to find a specific node by its Text property
        // Source: http://stackoverflow.com/questions/12388249/is-there-a-method-for-searching-for-treenode-text-field-in-treeview-nodes-collec/12388467#12388467
        public static IEnumerable<TreeNode> FlattenTree(this TreeView tv)
        {
            return FlattenTree(tv.Nodes);
        }

        // Method to flatter a TreeView to an enumerable of TreeNodes
        public static IEnumerable<TreeNode> FlattenTree(this TreeNodeCollection coll)
        {
            return coll.Cast<TreeNode>().Concat(coll.Cast<TreeNode>().SelectMany(x => FlattenTree(x.Nodes)));
        }

        // Method to find and return a TreeNode inside a TreeView which has specific text on it
        public static TreeNode FindNodeWithText(TreeView tv, string text)
        {
            var treeNodes = tv.FlattenTree().Where(n => n.Text == text).ToList();
            return (TreeNode)treeNodes[0];
        }

        // Remove all children node from a given node
        // Source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/689759dd-9ef8-4155-a06b-12f1c0882c8a/remove-all-child-nodes?forum=csharpgeneral
        public static void RemoveChildNodes(TreeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                for (int i = node.Nodes.Count - 1; i >= 0; i--)
                {
                    node.Nodes[i].Remove();
                }
            }
        }

        // ------------ Process and Memory methods ----------

        // Method to find the process in the processName property and set the processBaseAddress property ready for use
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

        // Method to return a feature address given a process handle, base adress, and pointer trail as a list of hexadecimal strings
        public static int findFeatureAddress(int processHandle, int baseAddress, List<string> hexPointerTrail)
        {
            //Console.WriteLine("***Base address is: " + baseAddress);

            // Our feature address will change as this method runs, but we always start at the base address
            int featureAddress = baseAddress;

            // Follow the pointer trail to find the final address of the feature
            // Note: If we remove the "minus one" part of the below loop we get the ACTUAL value of that feature (assuming it's an int like the clock)
            int offset = 0;
            for (int loop = 0; loop < hexPointerTrail.Count; loop++)
            {
                // Get our offset string as an int
                offset = Convert.ToInt32(hexPointerTrail.ElementAt(loop), 16);

                // Apply the offset
                featureAddress += offset;

                // If this was the final value of the pointer trail we've followed the entire trails so break out of the loop here...
                if (loop == (hexPointerTrail.Count - 1))
                {
                    break;
                }

                // ...otherwise, if this wasn't the final hop we read the address at that offset and go around the loop again.
                featureAddress = getIntFromAddress(processHandle, featureAddress);
            }

            return featureAddress;
        }

        // Method to add a hex value, specified as a string, to the pointer trail
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

        // Method to read and return an int (4 bytes)
        public static int getIntFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt32(buf, 0);
        }

        // Method to read and return a short (2 bytes)
        public static short getShortFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[2];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt16(buf, 0);
        }

        // Method to read and return a long (8 bytes)
        public static Int64 getLongFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt64(buf, 0);
        }

        // Method to read and return a float (4 bytes)
        public static float getFloatFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToSingle(buf, 0);
        }

        // Method to read and return a double (8 bytes)
        public static double getDoubleFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[8];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToDouble(buf, 0);
        }

        // Method to read and return a boolean (1 byte)
        public static bool getBoolFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToBoolean(buf, 0);
        }

        // Method to read and return a UTF-8 formatted string (1 byte per char - max of 33 chars returned)
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

        // Method to read and return a UTF-16 formatted string (2 bytes per char - max of 33 chars returned)
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

        // --------- UI Manipulation / Helper methods -------

        // Method to remove an aribtrary row from a TableLayoutPanel.
        // By default you cannot remove an arbitrary row from a table (only the last row), but
        // this method allows us to do so via a bunch of copy and pastes. Taken from stack overflow.
        // Source: http://stackoverflow.com/questions/15535214/removing-a-specific-row-in-tablelayoutpanel
        public static void removeRow(TableLayoutPanel panel, int rowIndex)
        {
            // Delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, rowIndex);
                panel.Controls.Remove(control);
            }

            // Move up row controls that comes after row we want to remove
            for (int i = rowIndex + 1; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - 1);
                    }
                }
            }

            // Remove last row
            if (panel.RowCount > 0) panel.RowCount--;
        }

        // Method to move a row in a TableLayoutPanel down by one, making room to insert a row at its previous location.
        // Note: This method is never used at present, but may come in useful later on so leaving in.
        public static void moveRowsDownByOne(TableLayoutPanel panel, int startingRowIndex)
        {
            //Console.WriteLine("Running move rows down by one - starting at row index: " + startingRowIndex);

            panel.SuspendLayout();

            // Add a new row to the table
            panel.RowCount++;

            for (int rowLoop = panel.RowCount - 1; rowLoop >= startingRowIndex; rowLoop--)
            {
                for (int columnLoop = 0; columnLoop < panel.ColumnCount; columnLoop++)
                {
                    var control = panel.GetControlFromPosition(columnLoop, rowLoop);
                    if (control != null)
                    {
                        Console.WriteLine("Moving control from row: " + rowLoop + " column " + columnLoop + " to row " + (rowLoop + 1) + " column " + columnLoop);
                        panel.SetRow(control, rowLoop + 1);
                        panel.SetColumn(control, columnLoop);
                    }
                }
            }

            panel.ResumeLayout();
        }

        // Method to return a list of string items split on commas with whitespaces removed and no 'blank' entries
        public static List<string> CommaSeparatedStringToStringList(string s)
        {
            return s.Split(',')
                   .Select(x => x.Trim())
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .ToList();
        }

        // Method to return a watch value type by its enum position / dropdown index
        public static Watch.ValueType GetValueTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Watch.ValueType.IntType;
                case 1:
                    return Watch.ValueType.ShortType;
                case 2:
                    return Watch.ValueType.LongType;
                case 3:
                    return Watch.ValueType.FloatType;
                case 4:
                    return Watch.ValueType.DoubleType;
                case 5:
                    return Watch.ValueType.BoolType;
                case 6:
                    return Watch.ValueType.StringUTF8Type;
                case 7:
                    return Watch.ValueType.StringUTF16Type;
                default:
                    return Watch.ValueType.IntType;
            }
        }

        // Method to return an int based on the value type of a watch
        public static int GetIntFromValueType(Watch.ValueType vt)
        {
            switch (vt)
            {
                case Watch.ValueType.IntType:
                    return 0;
                case Watch.ValueType.ShortType:
                    return 1;
                case Watch.ValueType.LongType:
                    return 2;
                case Watch.ValueType.FloatType:
                    return 3;
                case Watch.ValueType.DoubleType:
                    return 4;
                case Watch.ValueType.BoolType:
                    return 5;
                case Watch.ValueType.StringUTF8Type:
                    return 6;
                case Watch.ValueType.StringUTF16Type:
                    return 7;
                default:
                    return 0;
            }
        }

        // Method to return a trigger comparison type by its enum position / dropdown index
        public static Trigger.ComparisonType GetComparisonTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Trigger.ComparisonType.EqualTo;
                case 1:
                    return Trigger.ComparisonType.LessThan;
                case 2:
                    return Trigger.ComparisonType.LessThanOrEqualTo;
                case 3:
                    return Trigger.ComparisonType.GreaterThan;
                case 4:
                    return Trigger.ComparisonType.GreaterThanOrEqualTo;
                case 5:
                    return Trigger.ComparisonType.NotEqualTo;
                case 6:
                    return Trigger.ComparisonType.Changed;
                case 7:
                    return Trigger.ComparisonType.DistanceVolumeDescending;
                case 8:
                    return Trigger.ComparisonType.DistanceVolumeAscending;
                case 9:
                    return Trigger.ComparisonType.DistancePitchDescending;
                case 10:
                    return Trigger.ComparisonType.DistancePitchAscending;
                default:
                    return Trigger.ComparisonType.EqualTo;
            }
        }

        // Method to return an int based on the comparison type of a trigger
        public static int GetIntFromComparisonType(Trigger.ComparisonType ct)
        {
            switch (ct)
            {
                case Trigger.ComparisonType.EqualTo:
                    return 0;
                case Trigger.ComparisonType.LessThan:
                    return 1;
                case Trigger.ComparisonType.LessThanOrEqualTo:
                    return 2;
                case Trigger.ComparisonType.GreaterThan:
                    return 3;
                case Trigger.ComparisonType.GreaterThanOrEqualTo:
                    return 4;
                case Trigger.ComparisonType.NotEqualTo:
                    return 5;
                case Trigger.ComparisonType.Changed:
                    return 6;
                case Trigger.ComparisonType.DistanceVolumeDescending:
                    return 7;
                case Trigger.ComparisonType.DistanceVolumeAscending:
                    return 8;
                case Trigger.ComparisonType.DistancePitchDescending:
                    return 9;
                case Trigger.ComparisonType.DistancePitchAscending:
                    return 10;
                default:
                    return 0;
            }
        }

        // Method to return a trigger type by its enum position / dropdown index
        public static Trigger.TriggerType GetTriggerTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Trigger.TriggerType.Normal;
                case 1:
                    return Trigger.TriggerType.Continuous;
                case 2:
                    return Trigger.TriggerType.Modifier;
                default:
                    return Trigger.TriggerType.Normal;
            }
        }

        // Method to return an int based on the type of a trigger
        public static int GetIntFromTriggerType(Trigger.TriggerType tt)
        {
            switch (tt)
            {
                case Trigger.TriggerType.Normal:
                    return 0;
                case Trigger.TriggerType.Continuous:
                    return 1;
                case Trigger.TriggerType.Modifier:
                    return 2;
                default:
                    return 0;
            }
        }

        // Method to return a trigger allowance type by its enum position / dropdown index
        public static Trigger.AllowanceType GetAllowanceTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Trigger.AllowanceType.Any;
                case 1:
                    return Trigger.AllowanceType.InGame;
                case 2:
                    return Trigger.AllowanceType.InMenu;
                default:
                    return Trigger.AllowanceType.Any;
            }
        }

        // Method to return an int based on the alowance type of a trigger.
        public static int GetIntFromAllowanceType(Trigger.AllowanceType at)
        {
            switch (at)
            {
                case Trigger.AllowanceType.Any:
                    return 0;
                case Trigger.AllowanceType.InGame:
                    return 1;
                case Trigger.AllowanceType.InMenu:
                    return 2;
                default:
                    return 0;
            }
        }

        // Method to return the specific trigger with a given id value
        public static Trigger getTriggerWithId(int id)
        {
            Trigger t = null;
            for (int loop = 0; loop < MainForm.gameConfig.triggerList.Count; ++loop)
            {
                t = MainForm.gameConfig.triggerList[loop];
                if (t.id == id)
                    break;
            }
            return t;
        }

        // Method to return the specific watch with a given id value
        public static Watch getWatchWithId(int id)
        {
            Watch w = null;
            for (int loop = 0; loop < MainForm.gameConfig.watchList.Count; ++loop)
            {
                w = MainForm.gameConfig.watchList[loop];
                if (w.Id == id)
                    break;
            }
            return w;
        }

        // Method to return the next highest value to use as the next new watch id
        public static int getNextWatchIndex(List<Watch> list)
        {
            int highest = 0;
            for (int loop = 0; loop < list.Count; ++loop)
            {
                if (list[loop].Id > highest)
                {
                    highest = list[loop].Id;
                }
            }
            return highest + 1;
        }

        // Method to return the next highest value to use as the next new trigger id
        public static int getNextTriggerIndex(List<Trigger> list)
        {
            int highest = 0;
            for (int loop = 0; loop < list.Count; ++loop)
            {
                if (list[loop].id > highest)
                {
                    highest = list[loop].id;
                }
            }
            return highest + 1;
        }

    } // End of Utils class

} // End of namespace
