using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SoniFight
{
    public class Trigger
    {
        // Every trigger must have a destination value of a type,
        /*
        [XmlIgnore]
        public struct Value
        {
            public int    intValue;
            public short  shortValue;
            public float  floatValue;
            public double doubleValue;
            public bool   boolValue;
            public string stringValue;
        }*/

        

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
            DistanceBetween
        }
        
        public enum TriggerType
        {
            Once,       // Triggers once only when criteria is met (i.e. once when clock timer hits 50)
            Recurring,  // Triggers when criteria is met, but may trigger again (i.e. when EX bar hits a given level, gets used, and builds up to that level again)
            Continuous  // Triggers continuously (i.e. distance between players)
        }

        public enum ControlType
        {
            Normal, // Triggers that operate as normal to provide sonification
            Reset,  // Triggers that reset the 'Triggered' flag on all triggers with a TriggerType of Once
            Mute    // Triggers that mute all sonification of Normal triggers
        }

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

        public ControlType controlType;

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

        //public List<int> triggerStopList;  // Stops all triggers in the list from playing when this trigger is triggered (list is of id's)
        //public List<int> triggerResetList; // Resets all triggers in the list to have their triggered flag as false (list is of id's)


        
             
        

        // Whether we should use this trigger or not
        public bool active;

        [XmlIgnore]
        public bool spitDebug = true;

        // ---------- Methods ----------

        // Default constructor required for XML serialization
        public Trigger()
        {
            name = "CHANGE_ME";
            description = "CHANGE_ME";

            controlType = ControlType.Normal;

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
            name = source.name + "-CLONE";
            description = source.description;

            triggerType = source.triggerType;
            controlType = source.controlType;
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
