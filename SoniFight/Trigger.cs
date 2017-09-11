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
            DistanceVolumeDescending, // Used in continuous triggers only
            DistanceVolumeAscending,  // Used in continuous triggers only
            DistancePitchDescending,  // Used in continuous triggers only
            DistancePitchAscending    // Used in continuous triggers only
        }
        
        // Enum of different types of triggers
        public enum TriggerType
        {            
            Normal,      // Activates a sonification event when criteria is met. Normal triggers may play multiple times.
            Continuous,  // Activates a looped sonification event (e.g. distance between players). 
            Modifier     // Modifies a continuous trigger.
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

        // An optional secondary if for a watch or trigger. Normal triggers use this for dependent triggers, continuous
        // triggers use this for the second watch with which to calculate a percentage, and modifier triggers use this
        // as the continuous trigger to modify.
        public int secondaryId;
        
        // The type of comparison to make, i.e. EqualTo, LessThan, LessThanOrEqualTo etc.
        public ComparisonType comparisonType;
                
        // Properties for the sample to play along with its associated default speed and volume (defaults: 1.0 - i.e. full volume, standard playback speed)
        // Note: The sampleFilename field is used as the text to say if we are using tolk for sonification of this trigger.
        public string sampleFilename;
        public float sampleSpeed;
        public float sampleVolume;

        // Whether this trigger uses Tolk to generate the sonification event (true) or if it uses a sample file (false).
        public bool useTolk;

        // A current sample volume for continuous triggers whose volume may change with distance
        [XmlIgnore]
        public float currentSampleVolume;

        // A current sample speed for continuous triggers whose speed may change with distance
        [XmlIgnore]
        public float currentSampleSpeed;
        
        // Is this trigger the clock which we use to determine whether we're InGame or InMenu?
        public bool isClock;
        
        // Whether we should use this trigger or not
        public bool active;

        [XmlIgnore]
        public bool spitDebug = true;

        // The sampleKey is the relative path to the sample, such as ".\Configs\SomeGame\beep.mp3"
        [XmlIgnore]
        public string sampleKey = "";  // Just so it's not null and doesn't trip the pause-if-continuous-trigger-in-menus

        // If this is a modifier trigger, whether this modification is active or not (needed so we can reset the continuous trigger's state when no longer matching the modification criteria)
        [XmlIgnore]
        public bool modificationActive = false;

        // ---------- Methods ----------

        // Default constructor required for XML serialization
        public Trigger()
        {
            name        = "CHANGE_ME";
            description = "CHANGE_ME";

            triggerType    = TriggerType.Normal;
            comparisonType = Trigger.ComparisonType.EqualTo;
            allowanceType  = Trigger.AllowanceType.Any;

            watchOneId  = -1;
            secondaryId = -1;
            value       = -1;

            sampleFilename = "NONE";
            sampleVolume   = 1.0f;
            sampleSpeed    = 1.0f;

            active             = true;
            isClock            = false;
            modificationActive = false;
            useTolk            = false;
        }

        // Copy constructor which creates a deep-copy of an existing trigger
        public Trigger(Trigger source)
        {
            id = source.id;

            name        = source.name + "-CLONE";
            description = source.description;

            triggerType    = source.triggerType;
            comparisonType = source.comparisonType;
            allowanceType  = source.allowanceType;

            watchOneId  = source.watchOneId;
            secondaryId = source.secondaryId;
            value       = source.value;

            sampleFilename = source.sampleFilename;
            sampleVolume   = source.sampleVolume;
            sampleSpeed    = source.sampleSpeed;

            active             = source.active;
            isClock            = source.isClock;
            modificationActive = source.modificationActive;
            useTolk            = source.useTolk;
        }

    } // End of Trigger class

} // End of namespace
