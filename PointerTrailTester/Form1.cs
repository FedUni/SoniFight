using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

using au.edu.federation.PointerChainTester.Properties;

namespace au.edu.federation.PointerChainTester
{
    public partial class Form1 : Form
    {
        //public static ResourceManager resources = new ResourceManager();

        // Background worker which will attempt to connect to the requested process without locking up the UI
        public static BackgroundWorker processConnectionBW = new BackgroundWorker();

        // The value we'll read from memory
        dynamic value;

        // A timer to update the UI
        private System.Windows.Forms.Timer statusTimer;

        // Method to setup and start a timer which will check for updates to the process connection / feature value status
        // and update the UI
        public void InitTimer()
        {
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Tick += new EventHandler(statusTimer_Tick);
            statusTimer.Interval = 100; // in miliseconds
            statusTimer.Start();
        }

        // Each tick of the timer we do this...
        private void statusTimer_Tick(object sender, EventArgs e)
        {
            string s;

            if (!Program.connectedToProcess)
            {
                s = Resources.ResourceManager.GetString("notConnectedString");
                memoryAddressTextBox.Text = s;
                valueTextBox.Text = s;
            }
            else // We are connected to the process? Okay...
            {
                // ...do we have a valid pointer trail?
                if (!Program.validPointerTrail)
                {
                    s = Resources.ResourceManager.GetString("invalidPointerString");
                    memoryAddressTextBox.Text = s;
                    valueTextBox.Text = s;
                }
                else // We do? Great!
                {
                    // Convert int memory address to hex and update memory address field
                    memoryAddressTextBox.Text = Program.featureAddress.ToString("X");

                    // Read correct data type from address & set as the valueTB Text
                    switch (dataTypeComboBox.SelectedIndex)
                    {
                        case 0:
                            value = Utils.getIntFromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 1:
                            value = Utils.getShortFromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 2:
                            value = Utils.getLongFromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 3:
                            value = Utils.getFloatFromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 4:
                            value = Utils.getDoubleFromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 5:
                            value = Utils.getBoolFromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 6:
                            value = Utils.getUTF8FromAddress(Program.processHandle, Program.featureAddress);
                            break;
                        case 7:
                            value = Utils.getUTF16FromAddress(Program.processHandle, Program.featureAddress);
                            break;
                    }
                    valueTextBox.Text = Convert.ToString(value);

                } // End of we-have-a-valid-pointer-trail section

            } // End of if we're connected to the requested process section

        } // End of statusTimer_Tick method

        public Form1()
        {
            InitializeComponent();

            processConnectionBW.DoWork += connectToProcess;
            processConnectionBW.WorkerReportsProgress = false;
            processConnectionBW.WorkerSupportsCancellation = true;

            dataTypeComboBox.SelectedIndex = 0;
            
            processConnectionBW.RunWorkerAsync();

            // Add process handler
            processNameTextBox.TextChanged += (object s, EventArgs ea) =>
            {
                //processConnectionBW.CancelAsync();
                Program.processName = processNameTextBox.Text;
            };

            // Add pointer trail TB text change handler
            pointerTrailTextBox.TextChanged += (object s, EventArgs ea) =>
            {   
                Program.pointerList = Utils.CommaSeparatedStringToStringList(pointerTrailTextBox.Text);
                int x;
                foreach (string pointerValue in Program.pointerList)
                {
                    try
                    {
                        x = Convert.ToInt32(pointerValue, 16); // Convert from hex to int
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show( Resources.ResourceManager.GetString("formatExceptionString") );
                        return;
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show( Resources.ResourceManager.GetString("overflowExceptionString") );
                        return;
                    }
                    catch (ArgumentException ae)
                    {
                        MessageBox.Show( Resources.ResourceManager.GetString("argumentExceptionString") + ae.Message );
                        return;
                    }
                }

            }; // End of pointerTrailTB TextChanged handler

            InitTimer();
        }

        // DoWork method for the process connection background worker
        public void connectToProcess(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // Not connected and we're not cancelling? Then using the background worker...
            //while (!Program.connectedToProcess && !processConnectionBW.CancellationPending)
            while (!processConnectionBW.CancellationPending)
            {
                // Find all instances of the named process running on the local computer.
                // This will return an empty array if the process isn't running.
                Program.processArray = Process.GetProcessesByName(Program.processName);

                // Not found? Indicate we're trying...
                if (Program.processArray.Length < 1)
                {
                    Program.connectedToProcess = false;
                }
                else // Found the process by name?
                {
                    if (!Program.connectedToProcess)
                    {
                        // Flip the flag so we can stop trying to connect
                        Program.connectedToProcess = true;

                        // Get the process handle
                        Program.gameProcess = Program.processArray[0];
                        Program.processHandle = (int)Program.gameProcess.Handle;

                        // Get the process base address
                        Program.processBaseAddress = Utils.findProcessBaseAddress(Program.processName);
                        if (Program.processBaseAddress == 0)
                        {
                            break;
                        }
                    }

                    // We ALWAYS calculate the feature address. Note: This get re-calculated per iteration
                    // Note: findFeatureAddress will also set the Program.validPointerTrail flag to true or false depending on whether it's legal or not.
                    Program.featureAddress = Utils.findFeatureAddress(Program.processHandle, Program.processBaseAddress, Program.pointerList);
                    
                } // End of if we found the process section

                // Sleep briefly to avoid busy-waiting
                Thread.Sleep(100);

            } // End of while loop

        } // End of connectToProcess method

    } // End of class

} // End of namespace
