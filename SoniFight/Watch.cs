﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

using au.edu.federation.SoniFight.Properties;

namespace au.edu.federation.SoniFight
{
    // A class used to monitor a memory location (as described by a pointer trail) and retrieve data of a given type from that address
    public class Watch
    {
        // NOTE TO SELF: Properties MUST be public for XML Serializer to export them to XML. 

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
            StringUTF16Type
        }

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
                    return (Int16)Utils.getLongFromAddress(MainForm.gameConfig.ProcessHandle, DestinationAddress);

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

                default:
                    MessageBox.Show( Resources.ResourceManager.GetString("unrecognisedValueTypeWarningString") + valueType.ToString() );
                    return null;
            }
        }

        // What type of value this watch will read - default is int
        public ValueType valueType = ValueType.IntType;

        // Watch id is a unique positive integer
        [XmlIgnore]
        private int id;
        public int Id
        {
            get { return id;  }
            set { id = value; }
        }

        // The name of this watch (e.g. "Player 1 X-Location")
        [XmlIgnore]
        public string name;
        public string Name
        {
            get { return name;  }
            set { name = value; }
        }

        // A description of this watch (e.g. "Player 1 X-Location")
        [XmlIgnore]
        private string description;
        public string Description
        {
            get { return description;  }
            set { description = value; }
        }

        // The pointer list contains a list of pointers as strings in hexadecimal format. Note: Do not include any prefixes such as 0x or such.
        // Also: The base offset is the very first pointer in the list. This is typically a larger value than the other offsets which tend to be closer together.
        [XmlIgnore]
        private List<string> pointerList = new List<string>();
        public List<string> PointerList
        {
            get { return pointerList;  }
            set { pointerList = value; }
        }

        // The destination address of the feature this watch is monitoring.
        // IMPORTANT: Triggers don't have destination addresses - WATCHES have a destination address (and one watch may be used by multiple triggers)
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

            // At this point destination address should be correctly set ready for us to return
            return destinationAddress;
        }

    } // End of Watch class

} // End of namespace
