using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Expense_Income_App
{
    public partial class Form1 : Form
    {
        private DatabaseConnection dbConnection;
        public Form1()
        {
            InitializeComponent();
            dbConnection = new DatabaseConnection();
        }
        
        //Login Button with icon
        private void button1_Click(object sender, EventArgs e)
        {

            string username = textBox1.Text;
            string password = textBox2.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username or password cannot be empty");
                return; // Exit the method
            }

            try
            {
                dbConnection.Connect();

                // Create a command and parameterize the query to prevent SQL injection
                string query = "SELECT * FROM users WHERE uname = @uname AND pword = @pword";
                using (SqlCommand sqlCommand = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Add parameters to the command
                    sqlCommand.Parameters.AddWithValue("@uname", username);
                    sqlCommand.Parameters.AddWithValue("@pword", password);

                    // Execute the command and read the results
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("Login successful");
                            // Redirect to Form2
                            this.Hide(); // Hide the current form

                            Form2 newForm = new Form2();
                            newForm.FormClosed += (s, args) => this.Show();
                            newForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("An error occurred while connecting to the database: " + ex.Message);
            }
            finally
            {
                dbConnection.Disconnect();
            }
        
    }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //Close Button with icon
        private void button2_Click(object sender, EventArgs e)
        {
            // Display a message box with Yes and No options
            DialogResult result = MessageBox.Show("Exit app?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit(); 
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label2.Visible = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            label3.Visible = false;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }
    }
}
