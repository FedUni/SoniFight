using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace au.edu.federation.SoniFight
{
    // Class representing a hotkey which can be bound to force a trigger to activate (each trigger has a hotkey, whether it's active or not is held in the Trigger class).
    public class Hotkey
    {
        // We need user32.dll to register GLOBAL hotkeys (e.g. hotkeys that will work regardless of whether our application currently has focus).
        // RegisterHotkey(windowHandle, ID of hotkey, hotkey modifier code, activation key (if we have something like Key foo = Keys.A; Use: "foo.GetHashCode()" to obtain)
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // NOTE: ALT does NOT work on its own, maybe because it's looking to tie into menus? Alt WITH SOMETHING / ANYTHING ELSE is fine though.
        // See: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
        private int MOD_ALT = 0x0001;
        private int MOD_CONTROL = 0x0002;
        private int MOD_SHIFT = 0x0004;
        private int MOD_NOREPEAT = 0x4000; // Add in this flag so you can't hold down hotkeys for them to keep triggering

        // Whether this hotkey executes a watch or a trigger.
        public enum TargetType { ExecutesWatch, ExecutesTrigger }

        // The id of this hotkey which we construct from the watchOrTriggerID (below) and its HotkeyType (above)
        // Note: This is NOT the ID of the watch or trigger we will be using for sonification.
        // Also: This hotkey ID is only for THIS application, not global or anything.    

        public int Id;                  // This is the unique ID of the hotkey in the GameConfig, and is also used as the registration code when registering / unregistering the hotkey
        public string name = "";        // Format: Id-[Watch|Trigger]-watchOrTriggerId-The name of the hotkey, used to provide a brief discription
        public TargetType targetType;   // Whether this hotkey activates a watch or a trigger?
        public int watchOrTriggerID;    // The ID of the Watch or Trigger this hotkey activates
        public int activationKey;       // This is the actual key (NOT including modifiers) that will cause this hotkey to activate
        public int modifierCode;        // This is the bitflag indicating the combination of hotkeys being used with this hotkey's activation key
        public bool enabled;            // Is this hotkey enabled?

        // Default constructor. Note: This is required for XML serialisation.
        public Hotkey() {  }

        /*
        public void Enable()  { enabled = true; }
        public void Disable() { enabled = false; }
        public int GetHotkeyID() { return hotkeyID; }
        public int GetModifierCode() { return (int)modifierCode; }
        public int GetActivationCode() { return (int)activationKey; }
        */

        public bool IsValid()
        {
            if (Id == 0) { return false; }

            // TODO: Check watch or trigger ID exists in config

            if (activationKey == 0 /* e.g. Keys.None */) { return false; }

            return true;
        }

        // Method to set the modifier and activation keys as Keys
        public void SetKeyCombination(Keys theModifierKeys, Keys theActivationKey)
        {
            modifierCode = (int)theModifierKeys;
            activationKey = (int)theActivationKey;
        }

        // Method to set the modifier and activation keys as ints
        public void SetKeyCombination(int theModifierKeys, int theActivationKey)
        {
            modifierCode  = theModifierKeys;
            activationKey = theActivationKey;
        }

        // Method to register a global hotkey, will return true on success or false otherwise
        public bool Register()
        {
            

            // Check if hotkey is valid. If so then attempt to register, otherwise inform user of issue.
            if (IsValid())
            {
                bool succeeded = RegisterHotKey(MainForm.formHandlePtr, Id, modifierCode, activationKey);

                if (succeeded)
                {
                    Console.WriteLine("Successfully registered hotkey: " + this.ToString());
                }
                else
                {
                    Console.WriteLine("WARNING: Failed to registered hotkey: " + this.ToString());
                    MessageBox.Show("Warning: Failed to register hotkey: " + this.ToString(), "Hotkey Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                return succeeded;
            }
            else
            {
                // Try to assist user with the exact issue based on what could possibly go wrong...
                string exactIssue = "Unknown issue";
                if (activationKey == 0 /* e.g. Keys.None */) { exactIssue = "Activation key cannot be none."; }

                // Display messagebox and return false to indicate failure
                MessageBox.Show("Error: Hotkey is invalid. Issue: " + exactIssue + " \n\nFull hotkey details: " + this.ToString(), "Hotkey Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Method to unregister a global hotkey, will return true on success or false otherwise
        public bool Unregister()
        {

            bool result;
            result = UnregisterHotKey(MainForm.formHandlePtr, Id);
            if (result)
            {
                Console.WriteLine("Successfully unregistered hotkey: " + this.ToString());
            }
            else
            {
                Console.WriteLine("Failed to unregistered hotkey: " + this.ToString());
                MessageBox.Show("Warning: Failed to unregister hotkey: " + this.ToString(), "Hotkey Unregistration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return result;
        }

        // Method to generate a modifier code from a series of flags. Note: This does NOT set the modifierCode member variable - to do that use the below SetModifierCodeFromFlags method!
        public int GenerateModifierCodeFromFlags(bool hasAlt, bool hasControl, bool hasShift)
        {
            int code = MOD_NOREPEAT;
            if (hasAlt    ) { code |= MOD_ALT;     };
            if (hasControl) { code |= MOD_CONTROL; };
            if (hasShift  ) { code |= MOD_SHIFT;   };
            return code;
        }

        // Sets modifier code from some boolean flags - uses above GenerateModifierCodeFromFlags method
        public void SetModifierCodeFromFlags(bool hasAlt, bool hasControl, bool hasShift)
        {
            modifierCode = GenerateModifierCodeFromFlags(hasAlt, hasControl, hasShift);
        }

        // Does this hotkey use the Alt modifier?
        public bool usesAlt()
        {
            int hasAlt = modifierCode & ~MOD_NOREPEAT & ~MOD_CONTROL & ~MOD_SHIFT;
            return (hasAlt == 0) ? false : true;
        }

        // Does this hotkey use the Control modifier?
        public bool usesControl()
        {
            int hasControl = modifierCode & ~MOD_NOREPEAT & ~MOD_ALT & ~MOD_SHIFT;
            return (hasControl == 0) ? false : true;
        }

        // Does this hotkey use the Shift modifier?
        public bool usesShift()
        {
            int hasShift = modifierCode & ~MOD_NOREPEAT & ~MOD_ALT & ~MOD_CONTROL;
            return (hasShift == 0) ? false : true;
        }

        // Get a string representation of the activation sequence, i.e. Control+Shift+R
        public string getActivationSequenceString()
        {
            // Add each modifier and a space then convert the spaces into pluses then tack on the activation key
            string tmp = "";
            if (usesControl()) { tmp += "Control "; }
            if (usesAlt()    ) { tmp += "Alt ";     }
            if (usesShift()  ) { tmp += "Shift ";   }
            tmp = tmp.Replace(' ', '+');
            tmp += ((Keys)activationKey).ToString();
            return tmp;
        }

        public string getTargetTypeString() { return (targetType == TargetType.ExecutesWatch) ? "Watch" : "Trigger"; }


        public string generateAndSetHotkeyName()
        {
            name = Id.ToString() + "-" + getTargetTypeString() + "-" + watchOrTriggerID.ToString() + "-" + getActivationSequenceString();
            return name;
        }

        // Get a string that will match with the Hotkey Activation Target dropdown. Format: [Watch|Trigger]-Id_number-description_of_watch_or_trigger
        public string getHotkeyTargetString()
        {
            string tmp = getTargetTypeString() + "-" + watchOrTriggerID.ToString() + "-";

            if (targetType == TargetType.ExecutesWatch)
            {
                tmp += Utils.getWatchWithId(watchOrTriggerID).Name;
            }
            else
            {
                tmp += Utils.getTriggerWithId(watchOrTriggerID).Name;
            }

            return tmp;
        }

        // Method to return a string description of a hotkey
        public override string ToString()
        {
            // Hotkey ID
            string desc = "Hotkey ID: " + Id;
            if (enabled) { desc += " (Enabled)"; } else { desc += " (Disabled)"; }

            // Whether this activates a watch or trigger and the ID of that watch or trigger
            if (targetType == TargetType.ExecutesWatch) { desc += " - Activates Watch: "; } else { desc += " - Activates Trigger: "; }
            desc += watchOrTriggerID;

            // Add each modifier and a space then convert the spaces into pluses then tack on the activation key
            string modifierKeys = "";
            if (usesControl()) { modifierKeys += "Control "; }
            if (usesAlt()    ) { modifierKeys += "Alt ";     }
            if (usesShift()  ) { modifierKeys += "Shift ";   }

            desc += " - Activation Sequence: ";
            desc += getActivationSequenceString();

            /*
            if (modifierKeys.Length > 0)
            {
                modifierKeys = modifierKeys.Replace(' ', '+');
                desc += " - Activation Sequence: ";
                desc += modifierKeys;
            }

            if (modifierKeys.Length == 0)
            {
                desc += " - Activation sequence: ";
            }
            desc += activationKey.ToString();
            */

            return desc;
        }
    }
}
