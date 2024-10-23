using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Expense_Income_App
{
    public partial class Expenses : UserControl
    {
        public Expenses()
        {
            InitializeComponent();
            LoadUserData();
            LoadPieChartByMonth();

            comboBox1.Items.Clear();
            comboBox1.Items.Add("All months");
            comboBox1.SelectedIndex = 0;
            for (int month = 1; month <= 12; month++)
            {
                comboBox1.Items.Add(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
            }

            // Attach the event handler for selected index change
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;

        }

        //load data
        private void LoadUserData()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            try
            {
                // Connect to the database
                dbConnection.Connect();


                string query = "SELECT expenses_id, expenses_name, expenses_amount, expenses_date, expenses_desc FROM Expenses";

                // Create a DataTable to hold the data
                DataTable dt = new DataTable();

                // Create a data adapter
                using (SqlDataAdapter da = new SqlDataAdapter(query, DatabaseConnection.conn))
                {
                    // Fill the DataTable with the result of the SQL query
                    da.Fill(dt);
                }
             
                // Bind the DataTable to the DataGridView
                dataGridView1.DataSource = dt;

                dataGridView1.Columns["expenses_id"].Visible = false;
                dataGridView1.Columns["expenses_name"].HeaderText = "Expense Name";
                dataGridView1.Columns["expenses_amount"].HeaderText = "Amount";
                dataGridView1.Columns["expenses_date"].HeaderText = "Date";
                dataGridView1.Columns["expenses_desc"].HeaderText = "Desc";
            }
            catch (SqlException sqlEx)
            {
                // Handle SQL exception
                MessageBox.Show("SQL Error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                // Disconnect from the database
                dbConnection.Disconnect();
            }
        }

        //edit datagrid view info into the database with events (lil bolt)
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Get the edited 
            int editedRowIndex = e.RowIndex;

           
            string selectedId = dataGridView1.Rows[editedRowIndex].Cells[0].Value.ToString();

            // Get the updated values from the DataGridView row 
            string updatedName = dataGridView1.Rows[editedRowIndex].Cells[1].Value?.ToString();  
            string updatedAmount = dataGridView1.Rows[editedRowIndex].Cells[2].Value?.ToString(); 
            string updatedDate = dataGridView1.Rows[editedRowIndex].Cells[3].Value?.ToString();   
            string updatedDesc = dataGridView1.Rows[editedRowIndex].Cells[4].Value?.ToString();

            // Ensure the ID is not empty and all the necessary fields are updated
            if (!string.IsNullOrEmpty(selectedId) && updatedName != null && updatedAmount != null && updatedDate != null)
            {
                // Prompt the user for confirmation before proceeding with the update
                DialogResult dialogResult = MessageBox.Show(
                    "Update?",
                    "Update Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    DatabaseConnection dbConnection = new DatabaseConnection();
                    try
                    {
                        // Connect to the database
                        dbConnection.Connect();

                        // SQL query to update the record in the database
                        string query = "UPDATE expenses SET expenses_name = @expenses_name, expenses_amount = @expenses_amount, expenses_date = @expenses_date , expenses_desc = @expenses_desc WHERE expenses_id = @expenses_id";

                        // Create a command
                        using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                        {
                            // Add the parameters for the updated values
                            command.Parameters.AddWithValue("@expenses_name", updatedName);
                            command.Parameters.AddWithValue("@expenses_amount", updatedAmount);
                            command.Parameters.AddWithValue("@expenses_date", updatedDate);
                            command.Parameters.AddWithValue("@expenses_desc", updatedDesc);
                            command.Parameters.AddWithValue("@expenses_id", selectedId);

                            // Execute the command
                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Record updated successfully.");
                            }
                            else
                            {
                                MessageBox.Show("Failed to update record.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        dbConnection.Disconnect();
                    }
                }
                else
                {
                    
                    LoadUserData(); // Reload data 
                }
            }
            else
            {
                MessageBox.Show("Invalid data or ID not found.");
            }
        }
        private void LoadPieChartByMonth()
        {
            // Create an instance of the database connection
            DatabaseConnection dbConnection = new DatabaseConnection();

            try
            {
                // Connect to the database
                dbConnection.Connect();

                
                string query = @"
            SELECT 
                DATENAME(month, expenses_date) AS MonthName, 
                SUM(expenses_amount) AS TotalAmount 
            FROM 
                Expenses
            GROUP BY 
                DATENAME(month, expenses_date), 
                MONTH(expenses_date)
            ORDER BY 
                MONTH(expenses_date)";

                // Create a command to execute the query
                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Execute the query and read the data
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear the existing pie chart series data
                        chart1.Series["ExpensesByMonth"].Points.Clear();

                        // Loop through the result set and add points to the pie chart
                        while (reader.Read())
                        {
                            string monthName = reader["MonthName"].ToString();
                            decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);

                            // Add data to the pie chart series
                            int pointIndex = chart1.Series["ExpensesByMonth"].Points.AddXY(monthName, totalAmount);




                            // Set the label to show both the month and the value
                            chart1.Series["ExpensesByMonth"].Points[pointIndex].Label = $"{monthName}: {totalAmount:C}";


                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Handle any SQL exceptions
                MessageBox.Show("An error occurred while connecting to the database: " + ex.Message);
            }
            finally
            {
                // Ensure the database connection is closed
                dbConnection.Disconnect();
            }

        }

        private void LoadPieChartData(string monthName)
        {
            DatabaseConnection dbConnection = new DatabaseConnection();

            try
            {
                dbConnection.Connect();
                int monthNumber = DateTime.ParseExact(monthName, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                // SQL query to get total expenses grouped by date for the selected month
                string query = @"
            SELECT 
                CAST(expenses_date AS DATE) AS ExpensesDate, 
                SUM(expenses_amount) AS TotalExpense
            FROM 
                Expenses
            WHERE 
                MONTH(expenses_date) = @MonthNumber
            GROUP BY 
                CAST(expenses_date AS DATE)
            ORDER BY 
                ExpensesDate ASC"; 

                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Add parameter for the month
                    command.Parameters.AddWithValue("@MonthNumber", monthNumber);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear the existing chart series data
                        chart1.Series["ExpensesByMonth"].Points.Clear();

                        // Loop through the result set and add points to the chart
                        while (reader.Read())
                        {
                            string expensesDate = Convert.ToDateTime(reader["ExpensesDate"]).ToString("yyyy-MM-dd");
                            decimal totalExpense = Convert.ToDecimal(reader["TotalExpense"]);

                            // Add data to the chart series
                            int pointIndex = chart1.Series["ExpensesByMonth"].Points.AddXY(expensesDate, totalExpense);

                            // Set the label for the pie chart point
                            chart1.Series["ExpensesByMonth"].Points[pointIndex].Label = $"{totalExpense:C}";
                            chart1.Series["ExpensesByMonth"].Points[pointIndex].LabelForeColor = Color.Silver; // Set label color
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Handle any SQL exceptions
                MessageBox.Show("An error occurred while connecting to the database: " + ex.Message);
            }
            finally
            {
                // Ensure the database connection is closed
                dbConnection.Disconnect();
            }
        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected month
            string selectedMonthName = comboBox1.SelectedItem.ToString();

            if (selectedMonthName == "All months")
            {
                LoadPieChartByMonth();
            }
            else
            {
                // Load chart data for the selected month
                LoadPieChartData(selectedMonthName);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            {
                string name = textBox1.Text;
                string amount = textBox2.Text;
                string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                string desc = textBox3.Text;
                DatabaseConnection dbConnection = new DatabaseConnection();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(desc))
                {
                    MessageBox.Show("none of the fields can be empty");
                    return; //Exit the method to prevent further execution
                }

                try
                {
                    // Connect to the database
                    dbConnection.Connect();

                    // SQL query to insert data into the users table
                    string query = "INSERT INTO Expenses (expenses_name, expenses_amount, expenses_date, expenses_desc) VALUES (@name, @amount, @date, @desc)";

                    // Create command
                    using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@amount", amount);
                        command.Parameters.AddWithValue("@date", date);
                        command.Parameters.AddWithValue("@desc", desc);

                        // Execute the command
                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Data inserted successfully.");
                            
                            // DisplayIncomeData();
                            textBox1.Text = "";
                            textBox2.Text = "";
                            textBox3.Text = "";
                            LoadUserData();
                            LoadPieChartByMonth();

                        }
                        else
                        {
                            MessageBox.Show("Failed to insert data.");
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
        }
        //delete button
        private void button2_Click(object sender, EventArgs e)
        {
            // Check if any cell is selected
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Get the selected cell
                DataGridViewCell selectedCell = dataGridView1.SelectedCells[0];

                // Get the row index of the selected cell
                int selectedRowIndex = selectedCell.RowIndex;

                
                string selectedId = dataGridView1.Rows[selectedRowIndex].Cells[0].Value.ToString();

                // Confirm before deleting
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this record?", "Delete Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    DatabaseConnection dbConnection = new DatabaseConnection();
                    try
                    {
                        // Connect to the database
                        dbConnection.Connect();

                        // SQL query to delete the selected record
                        string query = "DELETE FROM expenses WHERE expenses_id = @expenses_id"; // Adjust column name if necessary

                        // Create a command
                        using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                        {
                            // Add the parameter for the ID
                            command.Parameters.AddWithValue("@expenses_id", selectedId);

                            // Execute the command
                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                // Remove the row from the DataGridView
                                dataGridView1.Rows.RemoveAt(selectedRowIndex);
                                MessageBox.Show("Record deleted successfully.");
                                LoadPieChartByMonth();
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete record.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exception
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        // Disconnect from the database
                        dbConnection.Disconnect();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a cell to delete the corresponding record.");
            }
        }

        private void Expenses_Load(object sender, EventArgs e)
        {

        }
    }
}
