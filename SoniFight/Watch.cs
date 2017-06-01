using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SoniFight
{
    public class Watch
    {
        // NOTE: Properties MUST be public for XML Serializer to export them to XML file.... 

        // Enum of different types of destination values we can read
        public enum ValueType
        {
            IntType,
            ShortType,
            FloatType,
            DoubleType,
            BoolType,
            StringType
        }

        private dynamic value;


        // Default constructor
        public Watch() { }

        // Copy constructor
        // Constructor which creates a deep-copy of an existing trigger
        public Watch(Watch source)
        {
            name = source.name + "-CLONE";
            description = source.description;
            valueType = source.valueType;
            active = source.active;

            // Deep copy the pointer list
            pointerList = new List<String>();
            foreach (string s in source.pointerList)
            {
                pointerList.Add(s);
            }            
        }

        public dynamic getDynamicValueFromType()
        {
            //dynamic value;

            switch (valueType)
            {
                case Watch.ValueType.IntType:
                    return (Int32)Utils.getIntFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);
                case Watch.ValueType.ShortType:
                    return (Int16)Utils.getShortFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);
                case Watch.ValueType.FloatType:
                    return (float)Utils.getFloatFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);
                case Watch.ValueType.DoubleType:
                    return (double)Utils.getDoubleFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);
                case Watch.ValueType.BoolType:
                    return (bool)Utils.getBoolFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);
                case Watch.ValueType.StringType:
                    return (string)Utils.getUTF16FromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);
                default:
                    MessageBox.Show("Value type in getDynamicValueFromType not recognised. Value type we got was: " + valueType.ToString());
                    return null;
            }
        }

        public ValueType valueType = ValueType.IntType;
        public void setValueType(ValueType vt) { this.valueType = vt; }

        [XmlIgnore]
        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        // Name of this watch (e.g. "Player 1 X-Location")
        [XmlIgnore]
        public string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        // Description of this watch (e.g. "Player 1 X-Location")
        [XmlIgnore]
        private string description;
        public string Description
        {
            get { return description;  }
            set { description = value; }
        }

        // The pointer list contains a list of pointers as strings in hexadecimal format
        // Note: Do not include any prefixes.
        // Also: The base offset is the very first pointer in the list. This is typically a larger value than the other offsets which tend to be closer together.
        [XmlIgnore]
        private List<string> pointerList = new List<string>();
        public List<string> PointerList
        {
            get { return pointerList; }
            set { pointerList = value; }
        }

        // A watch is associated with one or more triggers (id values are stored - the actual dictionary of triggers is in the GameConfig object)
        /*private List<int> triggerList = new List<int>();
        public List<int> TriggerList
        {
            get { return triggerList;  }
            set { triggerList = value; }
        }*/

        // Note: This form of getter/setter does not make the field publically available, and we don't want it
        // to be public because we don't want it in our serialized XML.
        // IMPORTANT: Triggers don't have destination addresses - WATCHES have a destination address (and one watch may have multiple triggers)
        [XmlIgnore]
        private int destinationAddress;
        [XmlIgnore]
        public int DestinationAddress
        {
            get { return destinationAddress;  }
            set { destinationAddress = value; }
        }

        // Whether or not the watch is active.
        // Note: We want this public so we can store it, because we might have watches in our GameConfig which we
        // might want to temporarily turn off.
        [XmlIgnore]
        private bool active = true;
        public bool Active
        {
            get { return active;  }
            set { active = value; }
        }


        // Method to update the destination address of this watch. This is called once per poll on all watches.
        public int updateDestinationAddress(int processHandle, int baseAddress)
        {
            // Our destination address will change as this method runs, but we start at the base address
            destinationAddress = baseAddress;

            // Follow the pointer trail to find the final address of the feature
            // Note: If we remove the "minus one" part of the below loop we get the ACTUAL value of that feature (assuming it's an int like the clock)
            int offset = 0;
            for (int hopLoop = 0; hopLoop < pointerList.Count; ++hopLoop)
            {
                offset = Convert.ToInt32(pointerList[hopLoop], 16);

                // Apply the offset
                destinationAddress += offset;
                
                // Final hop? Then that's where we'll find our value so break out of the loop
                if (hopLoop == (pointerList.Count - 1))
                {
                    break;
                }

                // Not final hop? Then read the address at that offset and keep going.
                destinationAddress = Utils.getIntFromAddress(processHandle, destinationAddress);
            }

            // At this point destination address should be correctly set ready for us to read from
            // Note: The specific type of value we'll read at this address will be based on the value type.

            return destinationAddress;
        }

        // This method MUST be implemented by all concrete Watch types (ValueWatch, PairWatch and ToggleWatch)
        // Note: A 'abstract' method MUST be overridden in any inheriting class, a 'virtual' method -may- be overriden, and if not
        //       then an existing version will be used, which may or may not work.
        /*public void checkTriggers()
        {

        }*/

    } // End of namespace
}
