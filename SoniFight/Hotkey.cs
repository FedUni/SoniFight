using System.Collections;
using System.Xml.Serialization;

namespace au.edu.federation.SoniFight
{
    // Class representing a hotkey which can be bound to force a trigger to activate (each trigger has a hotkey, whether it's active or not is held in the Trigger class).
    public class Hotkey
    {
        // The Id of the hotkey will be the same as the Id of the trigger to which it is bound (which is guaranteed to be unique).
        // Note: We do not write the hotkey id to the gameconfig XML because the trigger number might change.
        private int id;
        [XmlIgnore]
        public int Id
        {
            set { this.id = value; }
            get { return this.id;  }
        }

        public char key;
        public int modifierCode; 

        // Default constructor
        public Hotkey() { }

        // Two-parameter constructor
        public Hotkey(int id, char key, int modifierCode)
        {
            this.id           = id;
            this.key          = key;
            this.modifierCode = modifierCode;
        }

        // Does this hotkey use the Alt modifier?
        public bool usesAlt()
        {
            // alt = 1, so turns up in bit 0
            BitArray b = new BitArray(new int[] { modifierCode });
            return b[0] == true ? true : false;
        }

        // Does this hotkey use the Control modifier?
        public bool usesControl()
        {
            // ctrl = 2, so turns up in bit 1
            BitArray b = new BitArray(new int[] { modifierCode });
            return b[1] == true ? true : false;
        }

        // Does this hotkey use the Shift modifier?
        public bool usesShift()
        {
            // shift = 4, so turns up in bit 2
            BitArray b = new BitArray(new int[] { modifierCode });
            return b[2] == true ? true : false;
        }

        // Does this hotkey use the win-key modifier?
        public bool usesWin()
        {
            // win = 8, so turns up in bit 3
            BitArray b = new BitArray(new int[] { modifierCode });
            return b[3] == true ? true : false;
        }

        public int updateModifierCode(bool alt, bool control, bool shift, bool win)
        {
            modifierCode = 0;

            // This is built up like linux permissions, so 1 would mean Alt, 3 would mean Alt+Ctrl, 6 would mean Control+Shift etc.
            if (alt)     modifierCode += 1;
            if (control) modifierCode += 2;
            if (shift)   modifierCode += 4;
            if (win)     modifierCode += 8;

            return modifierCode;
        }

    }
        
}
