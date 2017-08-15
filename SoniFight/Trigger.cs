using System.Xml.Serialization;

namespace SoniFight
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
            DistanceVolumeDescending,
            DistanceVolumeAscending,
            DistancePitchDescending,
            DistancePitchAscending            
        }
        
        public enum TriggerType
        {            
            Normal,      // Triggers when criteria is met (i.e. passes a threshold etc). Normal triggers may play multiple times.
            Continuous,  // Triggers continuously (i.e. distance between players). We may only have a single trigger of type continuous in any GameConfig.
            Modifier     // Modifies a continuous trigger
        }
        
        public enum AllowanceType
        {
            Any,    // Trigger may activate in any state
            InGame, // Trigger may only activate when program state is InGame (i.e. live - we're playing a round)
            InMenu  // Trigger may only activate when program state is InMenu (i.e. the clock hasn't changed for Program.GameTickIntervalMS milliseconds)
        }

        // ---------- Properties ----------       

        // The unique ID of this trigger
        public int id;

        // A brief name and description (optional, but useful)
        public string name;
        public string description;

        // This is the value we activate this trigger on when there is a match with the value read from this triggers watch
        public dynamic value;

        // The last value read on this trigger. Used so we can activate only on crossing thresholds rather than repeatedly.
        [XmlIgnore]
        public dynamic previousValue;

        // Is this a normal, continuous or modifier trigger?
        public TriggerType triggerType;

        // Can this trigger be activated in menus, or in game, or both?
        public AllowanceType allowanceType;

        // The watch used to read data for this trigger.
        public int watchOneId;

        // An optional second watch used with continuous triggers.
        // Note: This property is used to hold the continuous trigger to modify if this trigger is a modifier trigger.
        public int watchTwoId;
        
        // The type of comparison to make, i.e. EqualTo, LessThan, LessThanOrEqualTo etc.
        public ComparisonType comparisonType;
                
        // Properties for the sample to play along with its associated default speed and volume (defaults: 1.0 - i.e. full volume, standard playback speed)
        public string sampleFilename;
        public float sampleSpeed;
        public float sampleVolume;

        // A current sample volume for continuous triggers whose volume may change with distance
        [XmlIgnore]
        public float currentSampleVolume;

        // A current sample speed for continuous triggers whose speed may change with distance
        [XmlIgnore]
        public float currentSampleSpeed;

        // Can this trigger be muted? If we mute when p1 or p2 health hits zero then character select triggers are muted until the reset hits (i.e. timer is 99 or such).
        //public bool mutable;

        // Is this trigger the clock which we use to determine whether we're InGame or InMenu?
        public bool isClock;
        
        // Whether we should use this trigger or not
        public bool active;

        [XmlIgnore]
        public bool spitDebug = true;

        // The sampleKey is the relative path to the sample, such as ".\Configs\SomeGame\beep.mp3"
        [XmlIgnore]
        public string sampleKey = "";  // Just so it's not null and doesn't trip the pause-if-continuous-trigger-in-menus

        [XmlIgnore]
        public bool modificationActive = false;

        // ---------- Methods ----------

        // Default constructor required for XML serialization
        public Trigger()
        {
            name = "CHANGE_ME";
            description = "CHANGE_ME";

            triggerType = TriggerType.Normal;

            watchOneId = -1;
            watchTwoId = -1;
            comparisonType = Trigger.ComparisonType.EqualTo;
            allowanceType = Trigger.AllowanceType.Any;

            value = -1;

            sampleFilename = "CHANGE_ME";
            sampleVolume = 1.0f;
            sampleSpeed = 1.0f;

            active = true;
            isClock = false;
            modificationActive = false;
        }

        // Constructor which creates a deep-copy of an existing trigger
        public Trigger(Trigger source)
        {
            id = source.id;

            name = source.name + "-CLONE";
            description = source.description;

            triggerType = source.triggerType;
            comparisonType = source.comparisonType;
            allowanceType = source.allowanceType;

            watchOneId = source.watchOneId;
            watchTwoId = source.watchTwoId;
            value = source.value;

            sampleFilename = source.sampleFilename;
            sampleVolume = source.sampleVolume;
            sampleSpeed = source.sampleSpeed;

            active = source.active;
            isClock = source.isClock;
            modificationActive = source.modificationActive;
        }

        public Trigger(int id) : base()
        {            
            this.id = id;
        }

    }
}
