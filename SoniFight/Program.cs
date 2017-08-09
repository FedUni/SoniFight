using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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
        

        // We start un-muted - triggers with a Mute control type can flip this flag, resetting triggers sets it back to false
        // TODO: REMOVE! We no longer mute things, we rely on the GameState (InMenu or InGame) along with threshold checks as we pass from, say, above to below a value.
        //private static bool currentlyMuted = false;

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

        // This method checks for successful comparisons between a trigger and the value read from that triggers watch
        public static bool performComparison(Trigger t, dynamic readValue, int recursiveDepth)
        {
            // Don't stack more than 5 triggers dependent on each-other - that would be silly. Avoiding silliness.
            if (recursiveDepth >= 5)
            {
                return false;
            }

            // Note: These 'opposite' comparison checks stop multiple retriggers of a sample as it only happens when the value first crosses the trigger threshold

            // Guard against user moving to edit tab where triggers are temporarily reset and there is no previous value
            if (t.previousValue != null)
            {
                // Dynamic type comparisons may possibly fail so wrap 'em in try/catch
                try
                {

                    switch (t.comparisonType)
                    {
                        case Trigger.ComparisonType.EqualTo:
                            if ((t.previousValue != t.value || recursiveDepth > 0) && (readValue == t.value))
                            {
                                Console.WriteLine("Trigger " + t.id + " matched equal with perform comparison on depth of: " + recursiveDepth);

                                // No dependent triggers (even if we ARE a dependent trigger at a recursive depth > 0?) - then we've already made a match so return true.
                                if (t.watchTwoId == -1)
                                {
                                    return true;
                                }
                                else
                                {
                                    // Trigger has a dependent trigger? Get and check it.
                                    Trigger dependentT = Utils.getTriggerWithId(t.watchTwoId);

                                    // If the dependent trigger is active, then our return type from THIS method is the return from checking the comparison
                                    // with the dependent trigger within this one (which has already matched or we wouldn't be here). This will recurse as
                                    // deep as the trigger dependencies are linked! Bwahahaha! =D Same with all others - just don't cyclic dependency it or we'll crash! 
                                    if (dependentT.active)
                                    {
                                        Watch dependentWatch = Utils.getWatchWithId(dependentT.watchOneId);

                                        // Dependent watch not active? Then obviously we must fail.
                                        if (!dependentWatch.Active)
                                        {
                                            return false;
                                        }

                                        bool dependentResult = performComparison(dependentT, dependentWatch.getDynamicValueFromType(), recursiveDepth + 1);

                                        if (!dependentResult)
                                        {
                                            Console.WriteLine("Suppressed trigger " + t.id + " due to failure of dependent trigger " + dependentT.id + ".");
                                        }

                                        return dependentResult;
                                    }
                                    else // Dependent trigger was not active so dependencies fail and we record no-match as the end result.
                                    {
                                        return false;
                                    }

                                } // End of if watchTwoId was not -1 section

                            } // Comparison failed?
                            return false;                            
                        case Trigger.ComparisonType.LessThan:
                            if ((t.previousValue > t.value) && (readValue < t.value)) { return true; }
                            //TODO: Add dependency recursion.
                            break;
                        case Trigger.ComparisonType.LessThanOrEqualTo:
                            if ((t.previousValue > t.value) && (readValue <= t.value)) { return true; }
                            //TODO: Add dependency recursion.
                            break;
                        case Trigger.ComparisonType.GreaterThan:
                            if ((t.previousValue < t.value) && (readValue > t.value)) { return true; }
                            //TODO: Add dependency recursion.
                            break;
                        case Trigger.ComparisonType.GreaterThanOrEqualTo:
                            if ((t.previousValue < t.value) && (readValue >= t.value)) { return true; }
                            //TODO: Add dependency recursion.
                            break;
                        case Trigger.ComparisonType.NotEqualTo:
                            if ((t.previousValue == t.value) && (readValue != t.value)) { return true; }
                            //TODO: Add dependency recursion.
                            break;
                        case Trigger.ComparisonType.Changed:
                            if (readValue != t.previousValue) { return true; }
                            //TODO: Add dependency recursion.
                            break;
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

        // This is the DoWork method for the BackgroundWorker
        public static void performSonification(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // Initialise the soundplayer
            //SoundPlayer.Init();

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

            // Look through all triggers...
            dynamic readValue;
            dynamic readValue2;
            dynamic currentClock = null;
            dynamic lastClock = null;

            // While we are providing sonification...            
            while (!e.Cancel)
            {                
                bool foundSonicMatch = false; // Did we find a match to a sonification condition?             

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
                
                // Update the game state to be InGame or InMenu
                for (int triggerLoop = 0; triggerLoop < gc.triggerList.Count; ++triggerLoop)
                {
                    // Grab a trigger
                    t = MainForm.gameConfig.triggerList[triggerLoop];

                    // Found the clock trigger?
                    if (t.isClock)
                    {
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
                            if (currentClock != lastClock)
                            {
                                // ...update the last clock to be the current clock and...
                                lastClock = currentClock;
                                
                                // ...set the current gamestate to be InGame.
                                Program.gameState = GameState.InGame;
                                //Console.WriteLine("Program state is InGame");
                            }
                            else // Current and last clock values the same? Then set the gamestate to be InMenu.
                            {
                                Program.gameState = GameState.InMenu;                                
                                //Console.WriteLine("Program state is InMenu");
                            }                            

                        } // End of if a second or more has elapsed block

                        // Get of the loop finding the clock trigger if we've found and processed it (i.e. no need to process further triggers)
                        break;

                    } // End of isClock block

                } // End of loop over triggers used to find the clock trigger 
                
                // Process triggers to provide sonification
                for (int triggerLoop = 0; triggerLoop < gc.triggerList.Count; ++triggerLoop)
                {
                    // Grab a trigger
                    t = MainForm.gameConfig.triggerList[triggerLoop];

                    // Have an active continuous trigger?
                    // Note: This CHECK must occur before the below block to function correctly.
                    if (t.active && t.triggerType == Trigger.TriggerType.Continuous)
                    {
                        // If we're InMenu we pause it...
                        if (Program.gameState == GameState.InMenu)
                        {
                            SoundPlayer.PauseSample(t.sampleKey);

                            continue; // No need to process this continuous trigger any further
                        }
                        else // ...otherwise if we're InGame we resume it.
                        {
                            
                            SoundPlayer.ResumeSample(t.sampleKey);
                        }
                    }

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
                    // Note: We don't know the type - but the watch itself knows the type, and 'getDynamicValueFromType'
                    // will ensure the correct data type is read.
                    readValue = Utils.getWatchWithId(t.watchOneId).getDynamicValueFromType();
                    
                    // Sonfiy for continuous events - ONLY in InGame state!
                    if (t.triggerType == Trigger.TriggerType.Continuous && Program.gameState == GameState.InGame)
                    {
                        // Read the second value associated with the second watch in this trigger
                        readValue2 = Utils.getWatchWithId(t.watchTwoId).getDynamicValueFromType();

                        // The trigger value acts as the range between watch values for continuous triggers
                        dynamic maxRange = t.value;

                        // Get the range and make it absolute (i.e. positive)
                        dynamic currentRange = Math.Abs(readValue - readValue2);

                        // Calculate the percentage of the current range to the max range
                        dynamic percentage;

                        switch (t.comparisonType)
                        {
                            case Trigger.ComparisonType.DistanceVolumeDescending:
                                percentage = currentRange / maxRange;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSampleVolume(t.sampleKey, Convert.ChangeType(t.sampleVolume * percentage, TypeCode.Single));
                                }
                                break;

                            case Trigger.ComparisonType.DistanceVolumeAscending:
                                percentage = 1.0 - (currentRange / maxRange);
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSampleVolume(t.sampleKey, Convert.ChangeType(t.sampleVolume * percentage, TypeCode.Single));
                                }
                                break;

                            case Trigger.ComparisonType.DistancePitchDescending:
                                percentage = currentRange / maxRange;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSamplePitch(t.sampleKey, Convert.ChangeType(t.sampleSpeed * percentage, TypeCode.Single));
                                }
                                break;

                            case Trigger.ComparisonType.DistancePitchAscending:
                                percentage = maxRange / currentRange;
                                if (SoundPlayer.CurrentlyPlaying(t.sampleKey))
                                {
                                    SoundPlayer.ChangeSamplePitch(t.sampleKey, Convert.ChangeType(t.sampleSpeed * percentage, TypeCode.Single));
                                }
                                break;
                        }

                    } // End of if triggerType is Continuous and gameState is InGame block

                    // Sonify for normal (i.e. recurring) triggers...
                    else if (t.triggerType == Trigger.TriggerType.Normal)
                    {
                        // Check our trigger for a match. Final 0 means we're kicking this off at the top level with no recursive trigger dependencies
                        foundSonicMatch = performComparison( t, Utils.getWatchWithId(t.watchOneId).getDynamicValueFromType(), 0);
                            
                        // If we found a match...
                        if (foundSonicMatch)
                        {
                            // InGame? Fine - play the sample because we don't stop InGame triggers from overlapping too heavily.
                            if (Program.gameState == GameState.InGame)
                            {
                                Console.WriteLine("InGame sample: " + t.sampleFilename + " - trigger id: " + t.id + " Volume: " + t.sampleVolume + " Speed: " + t.sampleSpeed);
                                
                                SoundPlayer.Play(t.sampleKey, t.sampleVolume, t.sampleSpeed, false); // Final false is because normal triggers don't loop

                                // Remove any queued menu triggers
                                menuTriggerQueue.Clear();
                            }
                            else // GameState must be InMenu
                            {
                                //Console.WriteLine("Found InMenu match: " + t.sampleFilename);

                                // Not already playing a menu sample? Great - play the one that matched.
                                if ( !SoundPlayer.IsPlaying() )
                                {
                                    //Console.WriteLine("DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!DING!");

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
                                
                            } // End of if in InMenu gamestate section                                                        

                        } // End of found sonic match section                       

                    } // End of if trigger type is Normal section                        

                    // If we have a queued menu trigger and we are now not playing, play the queued trigger and remove it from the menuTriggerQueue

                    // Calculate how many milliseconds since the last menu sonification event
                    double timeSinceLastMenuSonificationMS = ((TimeSpan)(DateTime.Now - lastMenuSonificationTime)).TotalMilliseconds;

                    // If we have a queued menu trigger and either i.) We're not currently playing audio OR ii.) It's been at least half a second since the last menu sonification event...
                    //if (menuTriggerQueue.Count > 0 && (!SoundPlayer.IsPlaying() || timeSinceLastMenuSonificationMS > 500.0) )
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

                // We do NOT unload all samples here - we only do that on SelectedIndexChanged of the config selection drop-down.
                // This minimises delay in stopping and starting sonification of the same config.
                // Note: If the user adds new triggers using new samples they will be added to the dictionary. 
                //SoundPlayer.UnloadAllSamples();
            }
            else 
            {
                Console.WriteLine("Sonification error - stopping. Cause: " + e.Result.ToString()); 
            }            
        }

    } // End of Program class

} // End of namespace
