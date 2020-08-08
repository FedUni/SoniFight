using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

using au.edu.federation.SoniFight.Properties;

namespace au.edu.federation.SoniFight
{
    // A class used to monitor a memory location (as described by a pointer chain) and retrieve data of a given type from that address
    public class Watch
    {
        // NOTE TO SELF: Properties MUST be public for XML Serializer to export them to XML. 

        // Enum of different types of destination values we can read
        public enum ValueType
        {
            IntType,
            ShortType,
            LongType,
            UnsignedIntType,
            FloatType,
            DoubleType,
            BoolType,
            StringUTF8Type,
            StringUTF16Type,
            ByteType
        }

        // ---------- Properties ----------        

        // What type of value this watch will read - default is int
        public ValueType valueType = ValueType.IntType;

        // Watch ID as a unique positive integer
        private int id;
        public int Id
        {
            get { return id;  }
            set { id = value; }
        }

        // The name of this watch (e.g. "Player 1 X-Location")
        private string name;
        public string Name
        {
            get { return name;  }
            set { name = value; }
        }

        // A description of this watch (e.g. "Tracks player 1 location, range is plus or minus 7.5f")
        private string description;
        public string Description
        {
            get { return description;  }
            set { description = value; }
        }

        // The pointer list contains a list of pointers as strings in hexadecimal format. Note: Do not include any prefixes such as 0x or such.
        // Also: The base offset is the very first pointer in the list. This is typically a larger value than the other offsets which tend to be closer together.
        private List<string> pointerList = new List<string>();
        public List<string> PointerList
        {
            get { return pointerList;  }
            set { pointerList = value; }
        }

        // The destination address of the feature this watch is monitoring.
        // IMPORTANT: Triggers don't have destination addresses - WATCHES have a destination address (and one watch may be used by multiple triggers)
        private IntPtr destinationAddress;
        [XmlIgnore]
        public IntPtr DestinationAddress
        {
            get { return destinationAddress;  }
            set { destinationAddress = value; }
        }

        // A flag to indicate Whether or not this watch is active.
        private bool active = true;
        public bool Active
        {
            get { return active;  }
            set { active = value; }
        }

        // ---------- Constructors ----------

        // Default constructor req'd for XML serialization
        public Watch() { }

        // Copy constructor which creates a deep-copy of an existing watch
        public Watch(Watch source)
        {
            name = source.name + Resources.ResourceManager.GetString("cloneString");
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

        // ---------- Methods ----------

        // Method to read a value of a given type from memory
        public dynamic getDynamicValueFromType()
        {
            switch (valueType)
            {
                case Watch.ValueType.IntType:
                    return (Int32)Utils.getIntFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.ShortType:
                    return (Int16)Utils.getShortFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.LongType:
                    return (Int64)Utils.getLongFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.UnsignedIntType:
                    return (UInt32)Utils.getUnsignedIntFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.FloatType:
                    return (float)Utils.getFloatFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.DoubleType:
                    return (double)Utils.getDoubleFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.BoolType:
                    return (bool)Utils.getBoolFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.StringUTF8Type:
                    return (string)Utils.getUTF8FromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.StringUTF16Type:
                    return (string)Utils.getUTF16FromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                case Watch.ValueType.ByteType:
                    return (Byte)Utils.getByteFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

                default:
                    MessageBox.Show(Resources.ResourceManager.GetString("unrecognisedValueTypeWarningString") + valueType.ToString());
                    return null;

            } // End of switch

        } // End of getDynamicValueFromType method

        public void evaluateAndUpdateDestinationAddress(IntPtr processHandle, IntPtr baseAddress)
        {
            destinationAddress = Utils.findFeatureAddress(processHandle, baseAddress, this.PointerList);
        }

        // Method to return a string description of a Watch
        public override string ToString()
        {   
            string desc = "Watch: " + Id;
            if (active) { desc += " (Enabled) "; } else { desc += " (Disabled) "; }
            desc += description;
            desc += " Destination address: ";
            desc += destinationAddress;
            desc += " Type: ";
            desc += valueType;
            desc += " Value: ";
            desc += getDynamicValueFromType();

            return desc;
        }

    } // End of Watch class

} // End of namespace
