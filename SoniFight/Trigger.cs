using System.Xml.Serialization;

using au.edu.federation.SoniFight.Properties;

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

        // The unique ID of this trigger
        [XmlIgnore]
        private int id = -1;
        public int Id
        { 
            get { return id;  }
            set { id = value; }
        }

        // A brief name and description (optional, but useful)
        [XmlIgnore]
        public string name;
        public string Name
        {
            get { return name;  }
            set { name = value; }
        }

        [XmlIgnore]
        private string description;
        public string Description
        {
            get { return description;  }
            set { description = value; }
        }

        // This is the value we activate this trigger on when there is a match with the value read from this triggers watch
        [XmlIgnore]
        private dynamic value;
        public dynamic Value
        {
            get { return value;       }
            set { this.value = value; }
        }

        // The last value read on this trigger. Used so we can activate only on crossing thresholds rather than repeatedly. This is used internally but not saved to XML.
        [XmlIgnore]
        private dynamic previousValue;
        [XmlIgnore]
        public dynamic PreviousValue
        {
            get { return previousValue;  }
            set { previousValue = value; }
        }

        // Is this a normal, continuous or modifier trigger?
        public TriggerType triggerType;

        // Can this trigger be activated in menus, or in game, or both?
        public AllowanceType allowanceType;

        // The type of comparison to make, i.e. EqualTo, LessThan, LessThanOrEqualTo etc.
        public ComparisonType comparisonType;

        // The watch used to read data for this trigger.
        [XmlIgnore]
        private int watchOneId;
        public int WatchOneId
        {
            get { return watchOneId;  }
            set { watchOneId = value; }
        }

        // An optional secondary if for a watch or trigger. Normal triggers use this for dependent triggers, continuous
        // triggers use this for the second watch with which to calculate a percentage, and modifier triggers use this
        // as the continuous trigger to modify.
        [XmlIgnore]
        private int secondaryId;
        public int SecondaryId
        {
            get { return secondaryId;  }
            set { secondaryId = value; }
        }

        // Properties for the sample to play along with its associated default speed and volume (defaults: 1.0 - i.e. full volume, standard playback speed)
        // Note: The sampleFilename field is used as the text to say if we are using tolk for sonification of this trigger.
        [XmlIgnore]
        private string sampleFilename;
        public string SampleFilename
        {
            get { return sampleFilename;  }
            set { sampleFilename = value; }
        }

        [XmlIgnore]
        private float sampleSpeed;
        public float SampleSpeed
        {
            get { return sampleSpeed;  }
            set { sampleSpeed = value; }
        }

        [XmlIgnore]
        private float sampleVolume;
        public float SampleVolume
        {
            get { return sampleVolume;  }
            set { sampleVolume = value; }
        }

        // Whether this trigger uses Tolk to generate the sonification event (true) or if it uses a sample file (false).
        [XmlIgnore]
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
        [XmlIgnore]
        private bool isClock;
        public bool IsClock
        {
            get { return isClock;  }
            set { isClock = value; }
        }

        // Whether we should use this trigger or not
        [XmlIgnore]
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

            WatchOneId  = -1;
            SecondaryId = -1;
            Value       = -1;

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

            Name        = source.Name + Resources.ResourceManager.GetString("cloneString");
            Description = source.Description;

            triggerType    = source.triggerType;
            comparisonType = source.comparisonType;
            allowanceType  = source.allowanceType;

            WatchOneId  = source.WatchOneId;
            SecondaryId = source.SecondaryId;
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