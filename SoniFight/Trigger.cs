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

        /*public enum ControlType
        {
            Normal, // Triggers that operate as normal to provide sonification
            Reset,  // Triggers that reset the 'Triggered' flag on all triggers with a TriggerType of Once
            Mute    // Triggers that mute all sonification of Normal triggers
        }*/

        public enum AllowanceType
        {
            Any,    // Trigger may activate in any state
            InGame, // Trigger may only activate when program state is InGame (i.e. live - we're playing a round)
            InMenu  // Trigger may only activate when program state is InMenu (i.e. the clock hasn't changed for Program.GameTickIntervalMS milliseconds)
        }

        // ---------- Properties ----------       

        public int id;

        public string name;
        public string description;

        //public bool isReset;

        // This is the value we trigger on
        public dynamic value;

        [XmlIgnore]
        public dynamic previousValue;

        public TriggerType triggerType;

        //public ControlType controlType;

        public AllowanceType allowanceType;

        public int watchOneId;
        public int watchTwoId;
        
        public ComparisonType comparisonType;
                
        public string sampleFilename;
        public float sampleSpeed;
        public float sampleVolume;

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
        }

        public Trigger(int id) : base()
        {            
            this.id = id;
        }

    }
}
