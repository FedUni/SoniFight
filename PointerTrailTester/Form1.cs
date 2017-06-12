using SoniFight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointerTrailTester
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
        }

        private void processTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add pointer trail TB text change handler
            pointerTrailTB.TextChanged += (object s, EventArgs ea) =>
            {
                List<string> tempPointerList = Utils.CommaSeparatedStringToStringList(pointerTB.Text);
                int x;
                foreach (string pointerValue in tempPointerList)
                {
                    try
                    {
                        x = Convert.ToInt32(pointerValue, 16); // Convert from hex to int
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Illegal Pointer Value " + pointerValue + " in watch with id " + currentWatch.Id + " cannot be cast to int. Do not prefix pointer hops with 0x or such.");
                        return;
                    }
                }
            };


        }
    }
}
