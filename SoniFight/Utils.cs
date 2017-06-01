using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SoniFight
{
    static class Utils
    {
        // Kernel hooks to read and write process memory
        // Note: Even on 64-bit systems the kernel is called kernel32!
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        // Constants are implied static in C# - you cannot mark them as such.
        public const int MAX_STRING_LENGTH = 150;

        

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

            //Console.WriteLine("About to write config - path is: " + MainForm.gameConfig.ConfigDirectory);

            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);

                Console.WriteLine("GameConfig at " + filePath + " saved successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not serialize file to XML.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);
            }
            finally
            {
                if (writer != null)
                {
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
            //var serializer;

            T obj = default(T);
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                obj = (T)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not deserialize object from XML file.");
                Console.WriteLine(e.Message);
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

        // ---------- TreeNode manipulation methods ---------

        // Flatten a TreeView to find a specific node by its Text property
        // Source: http://stackoverflow.com/questions/12388249/is-there-a-method-for-searching-for-treenode-text-field-in-treeview-nodes-collec/12388467#12388467
        public static IEnumerable<TreeNode> FlattenTree(this TreeView tv) { return FlattenTree(tv.Nodes); }

        public static IEnumerable<TreeNode> FlattenTree(this TreeNodeCollection coll)
        {
            return coll.Cast<TreeNode>().Concat(coll.Cast<TreeNode>().SelectMany(x => FlattenTree(x.Nodes)));
        }

        // My code
        public static TreeNode FindNodeWithText(TreeView tv, string text)
        {
            var treeNodes = tv.FlattenTree().Where(n => n.Text == text).ToList();
            return (TreeNode)treeNodes[0];
        }

        // Remove all children node from a given node
        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/689759dd-9ef8-4155-a06b-12f1c0882c8a/remove-all-child-nodes?forum=csharpgeneral
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

        // --------------------------------------------------

        

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
            /*if (!GameConfig.active)
            {
                Console.WriteLine("GameConfig is not active/validated!");
                return 0;
            }*/

            //Console.WriteLine("***Base address is: " + baseAddress);

            // Our final address will change as this method runs, but we start at the base address
            int featureAddress = baseAddress;

            // Follow the pointer trail to find the final address of the feature
            // Note: If we remove the "minus one" part of the below loop we get the ACTUAL value of that feature (assuming it's an int like the clock)
            int offset = 0;
            for (int loop = 0; loop < hexPointerTrail.Count; loop++)
            {
                //Console.WriteLine("***Loop is: " + loop);

                // Get our offset string as an int
                offset = Convert.ToInt32(hexPointerTrail.ElementAt(loop), 16);

                //Console.WriteLine("***Offset " + loop + " in hex is: " + hexPointerTrail.ElementAt(loop) + " which as an int is: " + offset);

                // Apply the offset
                featureAddress += offset;

                //Console.WriteLine("***Adding this offset takes us to address: " + featureAddress);

                if (loop == (hexPointerTrail.Count - 1))
                {
                    break;
                }

                // Read the address at that offset
                featureAddress = getIntFromAddress(processHandle, featureAddress);



                //Console.WriteLine("***Which has value: " + featureAddress);

                //Console.WriteLine("***----------------------------------***");
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


        public static int getIntFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;            
            byte[] buf = new byte[4];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt32(buf, 0);
        }

        public static short getShortFromAddress(int processHandle, int address)
        {   
            int bytesRead = 0;
            byte[] buf = new byte[2];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToInt16(buf, 0);
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

            //double temp = BitConverter.ToDouble(buf, 0);
            //Console.WriteLine("Read double as: " + temp);

            return BitConverter.ToDouble(buf, 0);
        }

        public static bool getBoolFromAddress(int processHandle, int address)
        {
            int bytesRead = 0;
            byte[] buf = new byte[1];
            ReadProcessMemory(processHandle, address, buf, buf.Length, ref bytesRead);
            return BitConverter.ToBoolean(buf, 0);
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

            //Console.WriteLine("Characters read: " + charCount);

            //Console.WriteLine("About to return: \"" + s.TrimEnd() + "\"");

            // Return a version of the read string with trailing spaces trimmed so the user does not have to add trailing spaces to their match criteria (which would be ugly - espcially for non-sighted users).
            return s.TrimEnd(); // Still trim spaces at end.
        }

        // By default you cannot remove an arbitrary row from a table (only the last row), but
        // this method allows us to do so via a bunch of copy and pastes. Taken from stack overflow.
        // Source: http://stackoverflow.com/questions/15535214/removing-a-specific-row-in-tablelayoutpanel
        public static void removeRow(TableLayoutPanel panel, int rowIndex)
        {
            /*if (row_index_to_remove >= panel.RowCount)
            {
                Console.WriteLine("Index greater than rowcount =/");
                return;
            }*/

            //this.SuspendLayout();
            //panel.Parent.Parent.SuspendLayout();
            //panel.Parent.SuspendLayout();
            //panel.SuspendLayout();

            // delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, rowIndex);
                panel.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
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

            // remove last row
            //panel.RowStyles.RemoveAt(panel.RowCount - 1);
            if (panel.RowCount > 0) panel.RowCount--;

            //panel.ResumeLayout();
            //panel.Parent.ResumeLayout();
            //panel.Parent.Parent.ResumeLayout();
            //this.ResumeLayout();
        }

        public static void moveRowsDownByOne(TableLayoutPanel panel, int startingRowIndex)
        {
            Console.WriteLine("Running move rows down by one - starting at row index: " + startingRowIndex);

            panel.SuspendLayout();

            // Add a new row to the table
            panel.RowCount++;

            //this.SuspendLayout();
            //panel.Parent.Parent.SuspendLayout();
            //panel.Parent.SuspendLayout();
            //panel.SuspendLayout();

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
            //panel.Parent.ResumeLayout();
            //panel.Parent.Parent.ResumeLayout();
            //this.ResumeLayout();

            Console.WriteLine("*****************Row count is: " + panel.RowCount);
        }

        public static List<string> CommaSeparatedStringToStringList(string s)
        {
            /*
            offset = Convert.ToInt32(hexPointerTrail.ElementAt(loop), 16);
            myList.Split(',').Select(s => Convert.ToInt32(s)).ToList();

            List<int> pointerList = new List<int>();
            pointerList = s.Split(',').Select(r => Convert.ToInt32)
            */

            //return s.Split(',').ToList().Tr;

            // Return a list of string items split on commas with whitespaces removed and no 'blank' entries (i.e. ",   ,")
            return s.Split(',')
                   .Select(x => x.Trim())
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .ToList();

        }

        public static Watch.ValueType GetValueTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Watch.ValueType.IntType;
                case 1:
                    return Watch.ValueType.ShortType;
                case 2:
                    return Watch.ValueType.FloatType;
                case 3:
                    return Watch.ValueType.DoubleType;
                case 4:
                    return Watch.ValueType.BoolType;
                case 5:
                    return Watch.ValueType.StringType;
                default:
                    return Watch.ValueType.IntType;
            }
        }

        public static int GetIntFromValueType(Watch.ValueType vt)
        {
            switch (vt)
            {
                case Watch.ValueType.IntType:
                    return 0;
                case Watch.ValueType.ShortType:
                    return 1;
                case Watch.ValueType.FloatType:
                    return 2;
                case Watch.ValueType.DoubleType:
                    return 3;
                case Watch.ValueType.BoolType:
                    return 4;
                case Watch.ValueType.StringType:
                    return 5;
                default:
                    return 0;
            }
        }

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
                    return Trigger.ComparisonType.DistanceBetween;
                default:
                    return Trigger.ComparisonType.EqualTo;
            }
        }

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
                case Trigger.ComparisonType.DistanceBetween:
                    return 7;
                default:
                    return 0;
            }

        } // End of GetIntFromComparisonType method

        public static Trigger.TriggerType GetTriggerTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Trigger.TriggerType.Once;
                case 1:
                    return Trigger.TriggerType.Recurring;
                case 2:
                    return Trigger.TriggerType.Continuous;
                default:
                    return Trigger.TriggerType.Once;
            }
        }

        public static int GetIntFromTriggerType(Trigger.TriggerType tt)
        {
            switch (tt)
            {
                case Trigger.TriggerType.Once:
                    return 0;
                case Trigger.TriggerType.Recurring:
                    return 1;
                case Trigger.TriggerType.Continuous:
                    return 2;
                default:
                    return 0;
            }
        }


        // Method to return the ControlType based on the selected index (int) of a dropdown menu
        public static Trigger.ControlType GetControlTypeFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return Trigger.ControlType.Normal;
                case 1:
                    return Trigger.ControlType.Reset;
                case 2:
                    return Trigger.ControlType.Mute;
                default:
                    return Trigger.ControlType.Normal;
            }
        }

        // Method to return an int based on the ControlType of a trigger
        public static int GetIntFromControlType(Trigger.ControlType ct)
        {
            switch (ct)
            {
                case Trigger.ControlType.Normal:
                    return 0;
                case Trigger.ControlType.Reset:
                    return 1;
                case Trigger.ControlType.Mute:
                    return 2;
                default:
                    return 0;
            }
        }

        // Method to return the ControlType based on the selected index (int) of a dropdown menu
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

        // Method to return an int based on the ControlType of a trigger
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
            for (int loop = 0; loop < MainForm.gameConfig.triggerList.Count; ++loop)
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

} // End of FairFight namespace
