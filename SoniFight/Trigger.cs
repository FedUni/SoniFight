using System.Xml.Serialization;

using au.edu.federation.SoniFight.Properties;
using System.Collections.Generic;

namespace au.edu.federation.SoniFight
{
    public class Trigger
    {
        // Enum of different types of comparisons we can make        
        public enum ComparisonType
        {
            EqualTo,
            LessThan,
            LessThanOrEqualTo,
            GreaterThan,
            GreaterThanOrEqualTo,
            NotEqualTo,
            Changed,
            DistanceVolumeDescending, // Used in continuous triggers only
            DistanceVolumeAscending,  // Used in continuous triggers only
            DistancePitchDescending,  // Used in continuous triggers only
            DistancePitchAscending    // Used in continuous triggers only
        }

        // Enum of different types of triggers
        public enum TriggerType
        {
            Normal,     // Activates a sonification event when criteria is met. Normal triggers may play multiple times.
            Dependent,  // Does not play a sound, but must be met to allow a normal trigger to activate if that normal trigger depends on the condition of one or more dependent triggers being met
            Continuous, // Activates a looped sonification event (e.g. distance between players). 
            Modifier    // Modifies a continuous trigger.
        }

        // Enum of when a trigger may be allowed to activate
        public enum AllowanceType
        {
            Any,    // Trigger may activate in any state.
            InGame, // Trigger may only activate when program state is InGame (i.e. live - we're playing a round).
            InMenu  // Trigger may only activate when program state is InMenu (i.e. the clock hasn't changed for Program.GameTickIntervalMS milliseconds).
        }

        // ---------- Properties ----------       

        // NOTE: Properties marked as private are not saved to XML, so we do not need the [XmlIgnore] tag on the line before they're declared.

        // The unique ID of this trigger
        private int id = -1;
        public int Id
        { 
            get { return id;  }
            set { id = value; }
        }

        // The name of this trigger
        private string name;
        public string Name
        {
            get { return name;  }
            set { name = value; }
        }

        // A brief description of this trigger
        private string description;
        public string Description
        {
            get { return description;  }
            set { description = value; }
        }

        // This is the value we activate this trigger on when there is a match with the value read from this triggers watch
        private dynamic value;
        public dynamic Value
        {
            get { return value;       }
            set { this.value = value; }
        }

        // The last value read on this trigger
        // Note: This is a list because a trigger may have multiple watches so it can be re-used, and each watch will have its own previous value
        private dynamic previousValueList;
        [XmlIgnore]
        public List<dynamic> PreviousValueList
        {
            get { return previousValueList;  }
            set { previousValueList = value; }
        }

        // Is this a normal, continuous or modifier trigger?
        public TriggerType triggerType;

        // Can this trigger be activated in menus, or in game, or both?
        public AllowanceType allowanceType;

        // The type of comparison to make, i.e. EqualTo, LessThan, LessThanOrEqualTo etc.
        public ComparisonType comparisonType;

        // The watch used to read data for this trigger.
        private List<int> watchIdList;
        public List<int> WatchIdList
        {
            get { return watchIdList;  }
            set { watchIdList = value; }
        }

        // An optional list of secondary IDs for a watch or trigger. Normal triggers use this for dependent triggers, continuous
        // triggers use this for the second watch with which to calculate a percentage, and modifier triggers use this
        // as the continuous trigger to modify.
        private List<int> secondaryIdList;
        public List<int> SecondaryIdList
        {
            get { return secondaryIdList;  }
            set { secondaryIdList = value; }
        }

        // The filename of the sample.
        // Note: The sampleFilename field is used as the text to say if we are using tolk for sonification of this trigger.
        private string sampleFilename;
        public string SampleFilename
        {
            get { return sampleFilename;  }
            set { sampleFilename = value; }
        }

        // The speed to play the sample. Range: 0.0f to 1.0f.
        private float sampleSpeed;
        public float SampleSpeed
        {
            get { return sampleSpeed;  }
            set { sampleSpeed = value; }
        }

        // The volume to play the sample. Range: 0.0f to 1.0f.
        private float sampleVolume;
        public float SampleVolume
        {
            get { return sampleVolume;  }
            set { sampleVolume = value; }
        }

        // Whether this trigger uses Tolk to generate the sonification event (true) or if it uses a sample file (false).
        private bool useTolk;
        public bool UseTolk
        {
            get { return useTolk;  }
            set { useTolk = value; }
        }

        // A current sample volume for continuous triggers whose volume may change with distance. This is used internally but not saved to XML.
        [XmlIgnore]
        private float currentSampleVolume;
        [XmlIgnore]
        public float CurrentSampleVolume
        {
            get { return currentSampleVolume;  }
            set { currentSampleVolume = value; }
        }

        // A current sample speed for continuous triggers whose speed may change with distance. This is used internally but not saved to XML.
        [XmlIgnore]
        private float currentSampleSpeed;
        [XmlIgnore]
        public float CurrentSampleSpeed
        {
            get { return currentSampleSpeed;  }
            set { currentSampleSpeed = value; }
        }

        // Is this trigger the clock which we use to determine whether we're InGame or InMenu?
        private bool isClock;
        public bool IsClock
        {
            get { return isClock;  }
            set { isClock = value; }
        }

        // Whether this trigger is active so will be used (true) or not (false).
        private bool active;
        public bool Active
        {
            get { return active;  }
            set { active = value; }
        }
        
        // The sampleKey is the relative path to the sample, such as ".\Configs\SomeGame\beep.mp3". This is used internally but not saved to XML.
        [XmlIgnore]
        private string sampleKey = "";  // Just so it's not null and doesn't trip the pause-if-continuous-trigger-in-menus
        [XmlIgnore]
        public string SampleKey
        {
            get { return sampleKey;  }
            set { sampleKey = value; }
        }

        // If this is a modifier trigger, this flag stores whether this modification is active or not. This is needed so we can reset the continuous
        // trigger's state when no longer matching the modification criteria. This is used internally but not saved to XML.
        [XmlIgnore]
        private bool modificationActive = false;
        [XmlIgnore]
        public bool ModificationActive
        {
            get { return modificationActive;  }
            set { modificationActive = value; }
        }

        // ---------- Methods ----------

        // Default constructor required for XML serialization
        public Trigger()
        {
            Name        = Resources.ResourceManager.GetString("changeMeString");
            Description = Resources.ResourceManager.GetString("changeMeString");

            triggerType    = TriggerType.Normal;
            comparisonType = Trigger.ComparisonType.EqualTo;
            allowanceType  = Trigger.AllowanceType.Any;

            WatchIdList = new List<int>();
            SecondaryIdList = new List<int>();
            SecondaryIdList.Add(-1);
            Value       = -1;

            PreviousValueList = new List<dynamic>();

            SampleFilename = Resources.ResourceManager.GetString("noneString");
            SampleVolume   = 1.0f;
            SampleSpeed    = 1.0f;

            Active             = true;
            IsClock            = false;
            ModificationActive = false;
            UseTolk            = false;
        }

        // Copy constructor which creates a deep-copy of an existing trigger
        public Trigger(Trigger source)
        {
            Id = source.Id;

            /* TODO: Make this so it becomes CLONE(2), CLONE(3) and so on, not CLONE-CLONE, CLONE-CLONE-CLONE etc.
            string fullSuffix = source.Name.Substring( source.Name.LastIndexOf('-') + 1);
            if (fullSuffix.Equals(Resources.ResourceManager.GetString("cloneString"))
            {
                Name = source.Name + "(2)";
            }
            else
            {
                Name = source.Name + Resources.ResourceManager.GetString("cloneString");
            }*/

            Name = source.Name + Resources.ResourceManager.GetString("cloneString");
            Description = source.Description;

            triggerType    = source.triggerType;
            comparisonType = source.comparisonType;
            allowanceType  = source.allowanceType;

            // Deep-copy the watch ID list so we don't end up accidentally modifiying the source Trigger
            WatchIdList = new List<int>();
            for (int loop = 0; loop < source.WatchIdList.Count; ++loop)
            {
                WatchIdList.Add(source.WatchIdList[loop]);
            }

            SecondaryIdList = new List<int>();
            for (int loop = 0; loop < source.SecondaryIdList.Count; ++loop)
            {
                SecondaryIdList.Add(source.SecondaryIdList[loop]);
            }

            PreviousValueList = new List<dynamic>();
            
            Value       = source.Value;

            SampleFilename = source.SampleFilename;
            SampleVolume   = source.SampleVolume;
            SampleSpeed    = source.SampleSpeed;

            Active             = source.Active;
            IsClock            = source.IsClock;
            ModificationActive = source.ModificationActive;
            UseTolk            = source.UseTolk;
        }

    } // End of Trigger class

} // End of namespace