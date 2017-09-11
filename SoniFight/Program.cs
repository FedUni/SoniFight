﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

using DavyKager; // Required for TOLK screenreader integration.

namespace SoniFight
{
    static class Program
    {
        // Please note: The program will only handle one GameConfig at any given time and this is
        // stored as a public static object in the Form.cs file.

        // We need to know whether the game is currently playing a live round or is in a menu.
        // We'll modify this enum based on whether the clock has changed in the last second or not. Live is so, InMenu otherwise.
        // Then, we'll use the gameState enum we'll keep to determine whether we should play triggers based on their GameStateRequirement setting.
        public enum GameState
        {
            InGame,
            InMenu
        };

        // When starting we assume the game is at the menu
        public static GameState gameState = GameState.InMenu;

        // We'll also keep track of the previous (last tick) game state and only play sonification events when the previous and current game states match
        // Note: The reason for this is because between rounds the click gets reset, and without this check we then think we're InGame when we're actually
        //       just between rounds, so it'll trigger InGame sonification between rounds, which we don't particularly want.
        public static GameState previousGameState = GameState.InMenu;

        // DateTime objects to use to determine if one second has passed (at which point we check if the clock has changed)
        static DateTime startTime, endTime;

        // The time that we played our last sonification event
        static DateTime lastMenuSonificationTime = DateTime.Now;
        
        // Maximum characters to compare when doing string comparisons
        public static int TEXT_COMPARISON_CHAR_LIMIT = 33;

        // Background worker for sonification
        public static BackgroundWorker sonificationBGW = new BackgroundWorker();
        public static AutoResetEvent resetEvent = new AutoResetEvent(false); // We use this to reset the worker

        // Our IrrKlang SoundPlayer instance
        static SoundPlayer soundplayer;

        public static bool connectedToProcess = false;

        // We don't want to play menu triggers at the same time (i.e. samples playing over the top of each other), so if we're playing a menu
        // trigger then we add the next menu trigger that wants to get played to this list. We also cap this list to a single item only and
        // overwrite the trigger in the queue if the SoundPlayer channel is playing. This stops us from 'buffering' menu samples if we quickly
        // navigate through menus and only plays something if we're not already playing, then plays the final menu option sonification event.
        static Queue<Trigger> menuTriggerQueue = new Queue<Trigger>(2);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Prepare sonficiation background worker...
            sonificationBGW.DoWork += performSonification;      // Specify the work method - this runs when RunWorkerAsync is called
            sonificationBGW.WorkerReportsProgress = false;      // We do not want progress reports
            sonificationBGW.WorkerSupportsCancellation = true;  // We do want to be able to cancel the background worker

            // Initialise our irrKlang SoundPlayer class ready to play audio
            Program.soundplayer = new SoundPlayer();
            
            // Main loop - we STAY on this line until the application terminates
            Application.Run(new MainForm());
            
            // IrrKlang cleanup (unload and samples and dispose of player)
            SoundPlayer.ShutDown();
        }

        // Method to determine if a trigger dependency has been met or not
        private static bool dependenceCheck(Trigger t, int recursiveDepth)
        {
            //Console.WriteLine("Trigger " + t.id + " matched equal with perform comparison on depth of: " + recursiveDepth);

            // No dependent triggers (even if we ARE a dependent trigger at a recursive depth > 0)? Then we've already made a match so return true.
            // Also, if this is a modifier trigger and we've made a value match we'll return true (because modifier triggers are focussed on matching
            // a condition, not only when we pass a threshold!).
            if (t.secondaryId == -1 || t.triggerType == Trigger.TriggerType.Modifier)
            {
                return true;
            }

            // At this point our trigger is a normal trigger. We know this because we return true if the trigger was a modifier, and continuous triggers
            // do not call the performComparison method.    

            // This trigger has a dependent trigger - so we grab it.
            Trigger dependentT = Utils.getTriggerWithId(t.secondaryId);

            // If the dependent trigger is active, then our return type from THIS method is the return from checking the comparison
            // with the dependent trigger within this one (which has already matched or we wouldn't be here). This will recurse as
            // deep as the trigger dependencies are linked - fails after 5 linked dependencies to prevent cyclic dependency crash.
            if (dependentT.active)
            {
                Watch dependentWatch = Utils.getWatchWithId(dependentT.watchOneId);

                // Watch of dependent trigger was not active? Then obviously we must fail as we're not updating the watch details.
                if (!dependentWatch.Active)
                {
                    return false;
                }

                // Does the dependent trigger match its target condition?
                bool dependentResult = performComparison(dependentT, dependentWatch.getDynamicValueFromType(), recursiveDepth + 1);

                // No? Then provide feedback that we'll be supressing this trigger because its dependent trigger failed.
                if (!dependentResult)
                {
                    Console.WriteLine("Trigger " + t.id + " supressed as dependent trigger " + dependentT.id + " failed (depth: " + recursiveDepth + ").");
                }
                return dependentResult;
            }
            else // Dependent trigger was not active so dependency fails and we record no-match as the end result.
            {
                return false;
            }
        }

        // This method checks for successful comparisons between a trigger and the value read from that triggers watch        
        public static bool performComparison(Trigger t, dynamic readValue, int recursiveDepth)
        {
            // Note: Continuous triggers do NOT call this method because their job is not to compare to a specific value, it's to compare
            //       two values and give a percentage (e.g. player 1 x-location and player 2 x-location).

            // Don't recurse more than 5 levels (so 6 in total, also stops cyclic loop stack overflow)
            if (recursiveDepth >= 5)
            {
                return false;
            }

            // Note: The 'opposite' comparison checks using the previous value below stop multiple retriggers of a sample as the sample only activates
            //       when the value crosses the trigger threshold.

            // Guard against user moving to edit tab where triggers are temporarily reset and there is no previous value
            if (t.previousValue != null)
            {
                // Dynamic type comparisons may possibly fail so wrap 'em in try/catch
                try
                {
                    // What type of value comparison are we making? Deal with each accordingly.
                    switch (t.comparisonType)
                    {
                        case Trigger.ComparisonType.EqualTo:                            
                            if ( (t.previousValue != t.value || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier) && (readValue == t.value) )
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }
                            
                            // Comparison failed? Return false.
                            return false;
                                                  
                        case Trigger.ComparisonType.LessThan:
                            if ( (t.previousValue > t.value || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier) && (readValue < t.value) )
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }

                            // Comparison failed? Return false.
                            return false;
                            
                        case Trigger.ComparisonType.LessThanOrEqualTo:
                            if ( (t.previousValue > t.value || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier) && (readValue <= t.value) )
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }

                            // Comparison failed? Return false.
                            return false;

                        case Trigger.ComparisonType.GreaterThan:
                            if ( (t.previousValue < t.value || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier) && (readValue > t.value) )
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }

                            // Comparison failed? Return false.
                            return false;

                        case Trigger.ComparisonType.GreaterThanOrEqualTo:
                            if ( (t.previousValue < t.value || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier) && (readValue >= t.value) )
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }

                            // Comparison failed? Return false.
                            return false;

                        case Trigger.ComparisonType.NotEqualTo:
                            if ( (t.previousValue == t.value || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier) && (readValue != t.value) )
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }

                            // Comparison failed? Return false.
                            return false;

                        case Trigger.ComparisonType.Changed:
                            if (readValue != t.previousValue || recursiveDepth > 0 || t.triggerType == Trigger.TriggerType.Modifier)
                            {
                                return dependenceCheck(t, recursiveDepth);
                            }

                            // Comparison failed? Return false.
                            return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Dynamic type comparison exception. Exact error is: " + e.Message);
                }

            } // End of it t.previousValue != null block

            // No matches? False it is, then!
            return false;
        }

        // This is the DoWork method for the sonification BackgroundWorker
        public static void performSonification(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // Load tolk library ready for use
            Tolk.Load();

            // Try to detect a screen reader and set a flag if we find one so we know we can use it for sonification events.
            bool screenReaderActive = false;
            string screenReaderName = Tolk.DetectScreenReader();
            if (screenReaderName != null)
            {
                screenReaderActive = true;
                Console.WriteLine("Tolk: The active screen reader driver is: {0}", screenReaderName);
                if (Tolk.HasSpeech())
                {
                    Console.WriteLine("Tolk: This screen reader driver supports speech.");
                }
                if (Tolk.HasBraille())
                {
                    Console.WriteLine("Tolk: This screen reader driver supports braille.");
                }
            }
            else
            {
                Console.WriteLine("Tolk: None of the supported screen readers are running.");
            }

            // Save some typing
            GameConfig gc = MainForm.gameConfig;

            // Convert all trigger 'value' properties (which are of type dynamic) to their actual type
            // Note: This is a ONE-OFF operation that we only do at the start before the main sonification loop
            Trigger t;
            for (int triggerLoop = 0; triggerLoop < gc.triggerList.Count; ++triggerLoop)
            {
                t = MainForm.gameConfig.triggerList[triggerLoop];

                switch (Utils.getWatchWithId(t.watchOneId).valueType)
                {
                    case Watch.ValueType.IntType:
                        t.value = Convert.ChangeType(t.value, TypeCode.Int32);
                        t.previousValue = new int();
                        t.previousValue = t.value; // By value
                        break;
                    case Watch.ValueType.ShortType:
                        t.value = Convert.ChangeType(t.value, TypeCode.Int16);
                        t.previousValue = new short();
                        t.previousValue = t.value; // By value
                        break;
                    case Watch.ValueType.LongType:
                        t.value = Convert.ChangeType(t.value, TypeCode.Int64);
                        t.previousValue = new long();
                        t.previousValue = t.value; // By value
                        break;
                    case Watch.ValueType.FloatType:
                        t.value = Convert.ChangeType(t.value, TypeCode.Single);
                        t.previousValue = new float();
                        t.previousValue = t.value; // By value
                        break;
                    case Watch.ValueType.DoubleType:
                        t.value = Convert.ChangeType(t.value, TypeCode.Double);
                        t.previousValue = new double();
                        t.previousValue = t.value; // By value
                        break;
                    case Watch.ValueType.BoolType:
                        t.value = Convert.ChangeType(t.value, TypeCode.Boolean);
                        t.previousValue = new bool();
                        t.previousValue = t.value; // By value
                        break;
                    case Watch.ValueType.StringUTF8Type:
                    case Watch.ValueType.StringUTF16Type:
                        t.value = Convert.ChangeType(t.value, TypeCode.String);
                        t.previousValue = t.value.ToString(); // Strings are reference types so we create a new copy to ensure value and previousValue don't point to the same thing!
                        break;
                    default:
                        t.value = Convert.ChangeType(t.value, TypeCode.Int32);
                        t.previousValue = new int();
                        t.previousValue = t.value; // By value
                        break;
                }

            } // End of loop over triggers

            // Get the time and the current clock
            startTime = DateTime.Now;

            // Declare a few vars once here to maintain scope throughout the 'game-loop'
            dynamic readValue;
            dynamic readValue2;
            dynamic currentClock = null;
            dynamic lastClock = null;

            // While we are providing sonification...            
            while (!e.Cancel)
            {                
                bool foundMatch = false; // Did we find a match to a sonification condition?             

                // Update all active watch destination addresses (this must happen once per poll!)
                Watch w;
                for (int watchLoop = 0; watchLoop < gc.watchList.Count; ++watchLoop)
                {
                    w = gc.watchList[watchLoop];

                    // Update the destination address of the watch if it's active - don't bother otherwise.
                    if (w.Active)
                    {
                        w.updateDestinationAddress(gc.ProcessHandle, gc.ProcessBaseAddress);
                    }
                }
                
                // Update the game state to be InGame or InMenu if we have a clock
                if (gc.ClockTriggerId != -1)
                { 
                    // Grab the clock trigger
                    t = Utils.getTriggerWithId(gc.ClockTriggerId);

                    // Read the value on it
                    currentClock = Utils.getWatchWithId(t.watchOneId).getDynamicValueFromType();

                    // Check if a round-tick has passed
                    endTime = DateTime.Now;
                    double elapsedMilliseconds = ((TimeSpan)(endTime - startTime)).TotalMilliseconds;

                    // If a GameConfig clock-tick has has passed (i.e. a second or such)
                    if (elapsedMilliseconds >= MainForm.gameConfig.ClockTickMS)
                    {
                        //Console.WriteLine("A clock tick has passed.");

                        // Reset the start time
                        startTime = DateTime.Now;                            

                        // Update the previous gamestate
                        Program.previousGameState = Program.gameState;

                        // If the current and last clocks differ...
                        if ( currentClock != lastClock)
                        {
                            // ...update the last clock to be the current clock and...
                            lastClock = currentClock;
                                
                            // ...set the current gamestate to be InGame.
                            Program.gameState = GameState.InGame;
                            //Console.WriteLine("Program state is InGame");

                            // This condition check stops us from moving briefly into the InGame state when the clock is reset between rounds or matches
                            if ((currentClock == 0 || lastClock == 0 || currentClock == MainForm.gameConfig.ClockMax) && Program.connectedToProcess)  
                            {
                                Console.WriteLine("Suppressed moving to InGame state because clock is 0 or " + MainForm.gameConfig.ClockMax + ".");
                                Program.gameState = GameState.InMenu;
                            }
                        }
                        else // Current and last clock values the same? Then set the gamestate to be InMenu.
                        {
                            Program.gameState = GameState.InMenu;                                
                            //Console.WriteLine("Program state is InMenu");
                        }                            

                    } // End of if a second or more has elapsed block                    

                } // End of game state update block
                
                // Process triggers to provide sonification
                for (int triggerLoop = 0; triggerLoop < gc.triggerList.Count; ++triggerLoop)
                {
                    // Grab a trigger
                    t = MainForm.gameConfig.triggerList[triggerLoop];

                    // Have an active continuous trigger?
                    // Note: This check must occur before the below 'should-we-skip-this-trigger' block to function correctly.
                    if (t.active && t.triggerType == Trigger.TriggerType.Continuous)
                    {
                        // If we're InMenu...
                        if (Program.gameState == GameState.InMenu)
                        {
                            // ...and the continuous trigger is NOT paused...
                            if (!SoundPlayer.IsPaused(t.sampleKey))
                            {
                                // ...then we pause it.
                                SoundPlayer.PauseSample(t.sampleKey);
                            }

                            continue; // No need to process this continuous trigger any further - if it wasn't paused we've now paused it.
                        }
                        else // ...otherwise if we're InGame...
                        {
                            // ...AND the sample is NOT playing, then we reset the volume and speed properties then resume it.
                            if (SoundPlayer.IsPaused(t.sampleKey))
                            {
                                t.currentSampleVolume = t.sampleVolume;
                                t.currentSampleSpeed = t.sampleSpeed;
                                SoundPlayer.ChangeSampleVolume(t.sampleKey, t.currentSampleVolume);
                                SoundPlayer.ChangeSampleSpeed(t.sampleKey, t.currentSampleSpeed);
                                SoundPlayer.ResumeSample(t.sampleKey);
                            }

                            // At this point we should be playing the continuous trigger, so we will NOT skip the rest of this iteration on the trigger
                            // so that any volume/speed adjustments can be made, if necessary.
                        }

                    } // End of if we have an active continuous trigger section

                    // There are other conditions under which we can skip processing triggers - such as...
                    if ( (t.allowanceType == Trigger.AllowanceType.InGame && Program.gameState == GameState.InMenu) || // ...if the trigger allowance and game state don't match...
                         (t.allowanceType == Trigger.AllowanceType.InMenu && Program.gameState == GameState.InGame) || // ...both ways, or... 
                         (Program.gameState != Program.previousGameState)                                           || // ...if we haven't been in this game state for 2 'ticks' or...
                         (t.isClock)                                                                                || // ...if this is the clock trigger or...
                         (!t.active) )                                                                                 // ...if the trigger is not active.
                    {
                        // Skip the rest of the loop for this trigger
                        continue;
                    }

                    // At this stage the trigger can be read and used - so let's get on with it...

                    // Read the value associated with the watch named by this trigger
                    // Note: We don't know the data type - but the watch itself knows the type, and 'getDynamicValueFromType'
                    // will ensure the correct data type is read.
                    readValue = Utils.getWatchWithId(t.watchOneId).getDynamicValueFromType();
                    
                    // Sonfiy for continuous events. Note: Continuous triggers may only be used in the InGame state!
                    if (t.triggerType == Trigger.TriggerType.Continuous && Program.gameState == GameState.InGame)
                    {
                        // Get the secondary watch associated with this continuous trigger
                        readValue2 = Utils.getWatchWithId(t.secondaryId).getDynamicValueFromType();

                        // The trigger value acts as the range between watch values for continuous triggers
                        dynamic maxRange = t.value;

                        // Get the range and make it absolute (i.e. positive)
                        dynamic currentRange = Math.Abs(readValue - readValue2);

                        // Calculate the percentage of the current range to the max range
                        float percentage;

                        /***
                         *  WARNING!
                         *  
                         *  The below switch condition deals with modifying continuous triggers based on whether they should change by volume or pitch, both ascending or descending.
                         *  
                         *  It comes with a VERY IMPORTANT CAVEAT.
                         *  
                         *  If your continuous trigger is varying volume, then you should NOT attach a modifier trigger to it that modifies volume or the results of the modifier trigger
                         *  will be overwritten on the next poll by the calculation of this continuous trigger.
                         *  
                         *  Similarly, if your continuous trigger is varying pitch, then you should NOT attach a modifier trigger to it that modifies pitch or again the result of the
                         *  modifier trigger will be overwritten on the next poll by the calculation of this continuous trigger section.
                         *  
                         *  To reiterate: Continuous changes volume? Modify on pitch only. Continuous changes pitch? Modify on volume only.
                         *  
                         *  Get it? Got it? Good!
                         * 
                         ***/

                        // TODO: Make sure you can't have continuous triggers with use watches with a non-numerical type.

                        // Perform sample volume/rate updates for this continuous trigger
                        switch (t.comparisonType)
                        {
                            case Trigger.ComparisonType.DistanceVolumeDescending:
                                percentage = (float)(currentRange / maxRange);
                                t.currentSampleVolume = t.sampleVolume * percentage;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSampleVolume(t.sampleKey, t.currentSampleVolume);
                                }
                                break;

                            case Trigger.ComparisonType.DistanceVolumeAscending:
                                percentage = (float)(1.0 - (currentRange / maxRange));
                                t.currentSampleVolume = t.sampleVolume * percentage;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSampleVolume(t.sampleKey, t.currentSampleVolume);
                                }
                                break;

                            case Trigger.ComparisonType.DistancePitchDescending:
                                percentage = (float)(currentRange / maxRange);
                                t.currentSampleSpeed = t.sampleSpeed * percentage;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSampleSpeed(t.sampleKey, t.currentSampleSpeed);
                                }
                                break;

                            case Trigger.ComparisonType.DistancePitchAscending:
                                percentage = (float)(1.0 - (currentRange / maxRange));
                                t.currentSampleSpeed = t.sampleSpeed * percentage;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSampleSpeed(t.sampleKey, t.currentSampleSpeed);
                                }
                                break;
                        }

                    } // End of if triggerType is Continuous and gameState is InGame block

                    // Sonify for normal triggers...
                    else if (t.triggerType == Trigger.TriggerType.Normal)
                    {
                        // Check our trigger for a match. Final 0 means we're kicking this off at the top level with no recursive trigger dependencies
                        foundMatch = performComparison( t, Utils.getWatchWithId(t.watchOneId).getDynamicValueFromType(), 0);
                            
                        // If we found a match...
                        if (foundMatch)
                        {
                            // InGame? Fine - play the sample because we don't stop InGame triggers from overlapping too heavily.
                            if (Program.gameState == GameState.InGame)
                            {
                                // If we're using a screen reader for the sonification event of this trigger
                                if (screenReaderActive && t.useTolk)
                                {
                                    // The sample filename contains the text to say
                                    Tolk.Output(t.sampleFilename);
                                }
                                else // Sample is a sample file
                                {
                                    Console.WriteLine("InGame sample: " + t.sampleFilename + " - trigger id: " + t.id + " Volume: " + t.sampleVolume + " Speed: " + t.sampleSpeed);
                                    SoundPlayer.Play(t.sampleKey, t.sampleVolume, t.sampleSpeed, false); // Final false is because normal triggers don't loop
                                }


                                // Remove any queued menu triggers
                                menuTriggerQueue.Clear();
                            }
                            else // GameState must be InMenu
                            {
                                // If we're using a screen reader for the sonification event of this trigger
                                if (screenReaderActive && t.useTolk)
                                {
                                    // The sample filename contains the text to say
                                    Tolk.Output(t.sampleFilename);
                                }
                                else // Sample is a sample file
                                {

                                    // Not already playing a menu sample? Great - play the one that matched.
                                    if (!SoundPlayer.IsPlaying())
                                    {
                                        // Not already playing a sample? So play this menu sample!
                                        Console.WriteLine("InMenu sample: " + t.sampleFilename + " - trigger id: " + t.id + " Volume: " + t.sampleVolume + " Speed: " + t.sampleSpeed);

                                        // Grab the time at which we played our last menu sonification event...
                                        lastMenuSonificationTime = DateTime.Now;

                                        // ...then play the sample.
                                        SoundPlayer.Play(t.sampleKey, t.sampleVolume, t.sampleSpeed, false); // Final false is to NOT loop InMenu triggers - only continuous triggers can do that
                                    }
                                    else // We are in the menus and already playing a menu sonification event...
                                    {
                                        // If there's nothing in the queue add this menu trigger.
                                        if (menuTriggerQueue.Count == 0)
                                        {
                                            menuTriggerQueue.Enqueue(t);
                                            //Console.WriteLine("Queue is less than one so adding menu trigger to queue. New queue size is: " + menuTriggerQueue.Count);
                                        }
                                        else // Already have one or more elements in the queue?
                                        {
                                            // Clear the queue and enque this sample for playing at the next interval
                                            menuTriggerQueue.Clear();
                                            menuTriggerQueue.Enqueue(t);

                                            //Console.WriteLine("Adding element to queue. New queue size is: " + menuTriggerQueue.Count);
                                        }

                                    } // End of section where we're InMenu but a sound is already playing

                                } // End of if sonification is via a sample section

                            } // End of if in InMenu gamestate section                                                        
                            
                        } // End of found sonic match section                       

                    }
                    else // Type must be Trigger.TriggerType.Modifier
                    {
                        // Check our modifier trigger for a match. Final 0 means we're kicking this off at the top level with no recursive trigger dependencies
                        foundMatch = performComparison(t, Utils.getWatchWithId(t.watchOneId).getDynamicValueFromType(), 0);
                        
                        // Get the continuous trigger related to this modifier trigger.
                        // Note: We ALWAYS need this because even if we don't find a match, we may need to reset the volume/pitch of the continuous sample to it's non-modified state
                        Trigger continuousTrigger = Utils.getTriggerWithId(t.secondaryId);

                        //Console.WriteLine("!!Cont vol: " + continuousTrigger.currentSampleVolume + ". Cont speed: " + continuousTrigger.currentSampleSpeed);

                        // Modifier condition met? Okay...
                        if (foundMatch)
                        {
                            // If this modifier trigger is NOT currently active we must activate it because we HAVE found a match for the modifier condition (i.e. foundMatch)
                            if (!t.modificationActive)
                            {
                                // Set the flag on this modification trigger to say it's active
                                t.modificationActive = true;

                                Console.WriteLine("1--Found modifier match for trigger " + t.id + " and modification was NOT active.");
                                Console.WriteLine("1--Continuous trigger's current sample volume is: " + continuousTrigger.currentSampleVolume);
                                Console.WriteLine("1--Modifier trigger's sample volume is: " + t.sampleVolume);
                                Console.WriteLine("1--Continuous trigger's current sample speed is: " + continuousTrigger.currentSampleSpeed);
                                Console.WriteLine("1--Modifier trigger's sample speed is: " + t.sampleSpeed);

                                // Add any volume or pitch changes to the continuous triggers playback
                                continuousTrigger.currentSampleVolume *= t.sampleVolume;
                                continuousTrigger.currentSampleSpeed  *= t.sampleSpeed;
                                SoundPlayer.ChangeSampleVolume(continuousTrigger.sampleKey, continuousTrigger.currentSampleVolume);
                                SoundPlayer.ChangeSampleSpeed(continuousTrigger.sampleKey, continuousTrigger.currentSampleSpeed);

                                Console.WriteLine("1--Multiplying gives new volume of: " + continuousTrigger.currentSampleVolume + " and speed of: " + continuousTrigger.currentSampleSpeed);
                            }

                            // Else modification already active on this continuous trigger? Do nothing.
                        }
                        else // Did NOT match modifier condition. Do we need to reset the continous trigger?
                        {
                            // If this modifier trigger IS currently active and we failed the match we have to reset the continuous triggers playback conditions
                            if (t.modificationActive)
                            {
                                Console.WriteLine("2--Did NOT find modifier match for trigger " + t.id + " and modification WAS active so needs resetting.");
                                Console.WriteLine("2--Continuous trigger's current sample volume is: " + continuousTrigger.currentSampleVolume);
                                Console.WriteLine("2--Modifier trigger's sample volume is: " + t.sampleVolume);
                                Console.WriteLine("2--Continuous trigger's current sample speed is: " + continuousTrigger.currentSampleSpeed);
                                Console.WriteLine("2--Modifier trigger's sample speed is: " + t.sampleSpeed);

                                // Set the flag on this modification trigger to say it's inactive
                                t.modificationActive = false;

                                // Reset the volume and pitch of the continuous trigger based on the modification trigger's volume and pitch
                                continuousTrigger.currentSampleVolume /= t.sampleVolume;
                                continuousTrigger.currentSampleSpeed  /= t.sampleSpeed;
                                SoundPlayer.ChangeSampleVolume(continuousTrigger.sampleKey, continuousTrigger.currentSampleVolume);
                                SoundPlayer.ChangeSampleSpeed(continuousTrigger.sampleKey, continuousTrigger.currentSampleSpeed);

                                Console.WriteLine("2--Dividing gives new volume of: " + continuousTrigger.currentSampleVolume + " and speed of: " + continuousTrigger.currentSampleSpeed);
                            }

                            // Else sonification already inactive after failing match? Do nothing.

                        } // End of if we did NOT match the modifier condition

                    } // End of modifier triggers section

                    // If we have a queued menu trigger and we are now not playing, play the queued trigger and remove it from the menuTriggerQueue

                    // Calculate how many milliseconds since the last menu sonification event
                    double timeSinceLastMenuSonificationMS = ((TimeSpan)(DateTime.Now - lastMenuSonificationTime)).TotalMilliseconds;

                    // If we have a queued menu trigger and either i.) We're not currently playing audio OR ii.) It's been at least half a second since the last menu sonification event...                    
                    if (menuTriggerQueue.Count > 0 && timeSinceLastMenuSonificationMS > 500.0)
                    {
                        // ...then we play the queued sample!
                        
                        //Console.WriteLine("Playing from queue!");

                        // This both gets the trigger and removes it from the queue in one fell swoop!
                        Trigger menuTrigger = menuTriggerQueue.Dequeue();

                        Console.WriteLine("Playing/removing queued sample: " + menuTrigger.sampleKey + " - trigger id: " + menuTrigger.id + " Volume: " + menuTrigger.sampleVolume + " Speed: " + t.sampleSpeed);

                        //Console.WriteLine("*** MS last menu sonification = " + timeSinceLastMenuSonificationMS);

                        // Mark the time at which we played the last menu sonification event...
                        lastMenuSonificationTime = DateTime.Now;

                        // ...then actually play the sample
                        SoundPlayer.Play(menuTrigger.sampleKey, menuTrigger.sampleVolume, menuTrigger.sampleSpeed, false); // Final false is because we don't loop InMenu triggers
                    }

                    // Update our 'previousValue' ready for the next check (used if comparison type is 'Changed').
                    // Note: We do this regardless of whether we found a match
                    t.previousValue = readValue;

                } // End of second loop over triggers

                // Did the user hit the stop button to cancel sonification? In which case do so!
                if (sonificationBGW.CancellationPending && sonificationBGW.IsBusy)
                {
                    e.Cancel = true;
                }

                // Update the SoundEngine
                SoundPlayer.updateEngine();

                // Finally, once all watches have been looked at, we sleep for the amount of time specified in the GameConfig
                Thread.Sleep(MainForm.gameConfig.PollSleepMS);

            } // End of while !e.Cancel

            // Unload tolk when we're stopping sonification
            Tolk.Unload();

            // If we're here then the background worker must have been cancelled so we call stopSonification
            stopSonification(e);
            
        } // End of performSonification method

        // Method to stop the sonification background worker
        public static void stopSonification(System.ComponentModel.DoWorkEventArgs e)
        {
            if (e.Cancel)
            {
                Console.WriteLine("\nSonification stopped.");

                // Flip the flag to say we're no longer connected
                Program.connectedToProcess = false;

                // Clean up our process connection and sonification background workers
                GameConfig.processConnectionBW.Dispose();
                sonificationBGW.Dispose();

                // We do NOT unload all samples here - we only do that on SelectedIndexChanged of the config selection drop-down or on quit.
                // This minimises delay in stopping and starting sonification of the same config.
                // Note: If the user adds new triggers using new samples they will be added to the existing dictionary. 
                //SoundPlayer.UnloadAllSamples();
            }
            else 
            {
                Console.WriteLine( "Sonification error - stopping. Cause: " + e.Result.ToString() ); 
            }

        } // End of stopSonification method

    } // End of Program class

} // End of namespace
