using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

using System.Resources;

namespace au.edu.federation.PointerTrailTester
{
    public partial class Form1 : Form
    {
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
            if (!Program.connectedToProcess)
            {
                memoryAddressTB.Text = "NOT CONNECTED TO PROCESS";
                valueTB.Text = "NOT CONNECTED TO PROCESS";
            }
            else // We are connected to the process? Okay...
            {
                // ...do we have a valid pointer trail?
                if (!Program.validPointerTrail)
                {
                    memoryAddressTB.Text = "INVALID POINTER TRAIL";
                    valueTB.Text = "INVALID POINTER TRAIL";
                }
                else // We do? Great!
                {
                    // Convert int memory address to hex and update memory address field
                    memoryAddressTB.Text = Program.featureAddress.ToString("X");

                    // Read correct data type from address & set as the valueTB Text
                    switch (dataTypeCB.SelectedIndex)
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
                    valueTB.Text = Convert.ToString(value);

                } // End of we-have-a-valid-pointer-trail section

            } // End of if we're connected to the requested process section

        } // End of statusTimer_Tick method

        // Constructor
        public Form1()
        {
            InitializeComponent();
            
            processConnectionBW.DoWork += connectToProcess;
            processConnectionBW.WorkerReportsProgress = false;
            processConnectionBW.WorkerSupportsCancellation = true;

            dataTypeCB.SelectedIndex = 0;

            InitTimer();
        }       

        private void Form1_Load(object sender, EventArgs e)
        {
            processConnectionBW.RunWorkerAsync();

            // Add process handler
            processNameTB.TextChanged += (object s, EventArgs ea) =>
            {
                //processConnectionBW.CancelAsync();
                Program.processName = processNameTB.Text;                
            };

            // Add pointer trail TB text change handler
            pointerTrailTB.TextChanged += (object s, EventArgs ea) =>
            {
                Program.pointerList = Utils.CommaSeparatedStringToStringList(pointerTrailTB.Text);
                int x;
                foreach (string pointerValue in Program.pointerList)
                {
                    try
                    {
                        x = Convert.ToInt32(pointerValue, 16); // Convert from hex to int
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Illegal pointer trail - do not prefix pointer hops with 0x or such and separate each hop with a comma.");
                        return;
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("Illegal pointer trail - individual pointer hop value exceeds 32-bit limit.");
                        return;
                    }
                    catch (ArgumentException ae)
                    {
                        MessageBox.Show("Illegal pointer trail - argument exception: " + ae.Message);
                        return;
                    }
                }

            }; // End of pointerTrailTB TextChanged handler

        } // End of Form1_Load method

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
                    //Console.Write(".");
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
                            //MessageBox.Show("Error: No process called " + processName + " found. Activation failed.");
                            //e.Cancel = true;
                            break;
                        }
                        else
                        {
                            //Console.WriteLine("Found process base address at: " + MainForm.gameConfig.ProcessBaseAddress);
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

    } // End of Form1 partial class

} // End of namespace
