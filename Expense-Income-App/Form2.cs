using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Expense_Income_App
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        //close app button with icon
        private void button2_Click(object sender, EventArgs e)
        {
            // Display a message box with Yes and No options
            DialogResult result = MessageBox.Show("Exit app?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void dashboard1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = true;
            income1.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            income1.Visible = true;
            expenses1.Visible = false;
        }

        private void income1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            dashboard1.Visible = false;
            income1.Visible = false;
            expenses1.Visible = true;
        }
    }
}
