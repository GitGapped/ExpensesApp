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
    public partial class Income : UserControl
    {
        public Income()
        {
            InitializeComponent();
            LoadUserData();
            LoadLineChartDatadefault();
            
            //populate drop down
            comboBoxMonths.Items.Clear();
            comboBoxMonths.Items.Add("All months");
            comboBoxMonths.SelectedIndex = 0;
            for (int month = 1; month <= 12; month++)
            {
                comboBoxMonths.Items.Add(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
            }

            // Attach the event handler for selected index change
            comboBoxMonths.SelectedIndexChanged += ComboBoxMonths_SelectedIndexChanged;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }
        private void LoadUserData()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            try
            {
                // Connect to the database
                dbConnection.Connect();


                string query = "SELECT customer_id, customer_name, customer_job, customer_payment, customer_date FROM Customers";

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

                dataGridView1.Columns["customer_id"].Visible = false;
                dataGridView1.Columns["customer_name"].HeaderText = "Income Name";


                dataGridView1.Columns["customer_job"].HeaderText = "Job Type";


                dataGridView1.Columns["customer_payment"].HeaderText = "Amount";
                dataGridView1.Columns["customer_date"].HeaderText = "Date";
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("SQL Error: " + sqlEx.Message);
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
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Get the edited row index
            int editedRowIndex = e.RowIndex;

            
            string selectedId = dataGridView1.Rows[editedRowIndex].Cells[0].Value.ToString();

            // Get the updated values from the DataGridView row 
            string updatedName = dataGridView1.Rows[editedRowIndex].Cells[1].Value?.ToString();  
            string updatedDesc = dataGridView1.Rows[editedRowIndex].Cells[2].Value?.ToString(); 
            string updatedAmount = dataGridView1.Rows[editedRowIndex].Cells[3].Value?.ToString();   
            string updatedDate = dataGridView1.Rows[editedRowIndex].Cells[4].Value?.ToString();
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
                        string query = "UPDATE customers SET customer_name = @customer_name, customer_payment = @customer_payment, customer_date = @customer_date , customer_job = @customer_job WHERE customer_id = @customer_id";

                        // Create a command
                        using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                        {
                            // Add the parameters for the updated values
                            command.Parameters.AddWithValue("@customer_name", updatedName);
                            command.Parameters.AddWithValue("@customer_payment", updatedAmount);
                            command.Parameters.AddWithValue("@customer_date", updatedDate);
                            command.Parameters.AddWithValue("@customer_job", updatedDesc);
                            command.Parameters.AddWithValue("@customer_id", selectedId);

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
        private void LoadLineChartDatadefault()
        {
            // Create an instance of the database connection
            DatabaseConnection dbConnection = new DatabaseConnection();

            try
            {
                // Connect to the database
                dbConnection.Connect();
                // int monthNumber = DateTime.ParseExact(monthName, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                // SQL query to get total income grouped by date
                string query = @"
             SELECT 
                 CAST(customer_date AS DATE) AS PaymentDate, 
                 SUM(customer_payment) AS TotalIncome
             FROM 
                 Customers
             GROUP BY 
                 CAST(customer_date AS DATE)
             ORDER BY 
                 PaymentDate ASC"; // Group by date and order by the date in ascending order

                // Create a command to execute the query
                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Execute the query and read the data
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear the existing chart series data
                        chart2.Series["IncomeChart"].Points.Clear();

                        // Loop through the result set and add points to the chart
                        while (reader.Read())
                        {
                            string paymentDate = Convert.ToDateTime(reader["PaymentDate"]).ToString("yyyy-MM-dd"); // Format date as needed
                            decimal totalIncome = Convert.ToDecimal(reader["TotalIncome"]);

                            // Add data to the chart series
                            int pointIndex = chart2.Series["IncomeChart"].Points.AddXY(paymentDate, totalIncome);

                            // Optionally, set the label to show the date and total income
                            //chart2.Series["IncomeChart"].Points[pointIndex].Label = $"{paymentDate}: {totalIncome:C}";
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
        private void LoadLineChartData(string monthName)
        {
            DatabaseConnection dbConnection = new DatabaseConnection();

            try
            {
                dbConnection.Connect();
                int monthNumber = DateTime.ParseExact(monthName, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                // SQL query to get total income grouped by date for the selected month
                string query = @"
                    SELECT 
                        CAST(customer_date AS DATE) AS PaymentDate, 
                        SUM(customer_payment) AS TotalIncome
                    FROM 
                        Customers
                    WHERE 
                        MONTH(customer_date) = @MonthNumber
                    GROUP BY 
                        CAST(customer_date AS DATE)
                    ORDER BY 
                        PaymentDate ASC"; // Group by date and order by date

                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Add parameter for the month
                    command.Parameters.AddWithValue("@MonthNumber", monthNumber);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear the existing chart series data
                        chart2.Series["IncomeChart"].Points.Clear();

                        // Loop through the result set and add points to the chart
                        while (reader.Read())
                        {
                            string paymentDate = Convert.ToDateTime(reader["PaymentDate"]).ToString("yyyy-MM-dd");
                            decimal totalIncome = Convert.ToDecimal(reader["TotalIncome"]);

                            // Add data to the chart series
                            int pointIndex = chart2.Series["IncomeChart"].Points.AddXY(paymentDate, totalIncome);
                            chart2.Series["IncomeChart"].Points[pointIndex].Label = $" {totalIncome:C}";
                            chart2.Series["IncomeChart"].Points[pointIndex].LabelForeColor = Color.Silver;
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

        private void ComboBoxMonths_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected month
            string selectedMonthName = comboBoxMonths.SelectedItem.ToString();

            if (selectedMonthName == "All months")
            {
                // Load all data

                LoadLineChartDatadefault();
            }
            else
            {
                // Load chart data for the selected month
                LoadLineChartData(selectedMonthName);
            }
        }
        private void Income_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Get the selected cell
                DataGridViewCell selectedCell = dataGridView1.SelectedCells[0];

                // Get the row index of the selected cell
                int selectedRowIndex = selectedCell.RowIndex;

                // Assuming the primary key or unique identifier is in the first column (adjust if necessary)
                // We get the value of the primary key (e.g., expenses_id) from the selected row, first column
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
                        string query = "DELETE FROM customers WHERE customer_id = @customer_id"; // Adjust column name if necessary

                        // Create a command
                        using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                        {
                            // Add the parameter for the ID
                            command.Parameters.AddWithValue("@customer_id", selectedId);

                            // Execute the command
                            int result = command.ExecuteNonQuery();

                            if (result > 0)
                            {
                                // Remove the row from the DataGridView
                                dataGridView1.Rows.RemoveAt(selectedRowIndex);
                                MessageBox.Show("Record deleted successfully.");
                                LoadLineChartDatadefault();
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete record.");
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
            }
            else
            {
                MessageBox.Show("Please select a cell to delete the corresponding record.");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string payment = textBox2.Text;
            string address = textBox4.Text;
            string jobkind = textBox4.Text;
            string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            DatabaseConnection dbConnection = new DatabaseConnection();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(jobkind) || string.IsNullOrEmpty(payment) || string.IsNullOrEmpty(date))
            {
                MessageBox.Show("none of the fields can be empty");
                return; 
            }

            try
            {
                dbConnection.Connect();

                // SQL query to insert data into the users table
                string query = "INSERT INTO Customers (customer_name, customer_job,  customer_payment, customer_date) VALUES (@name, @jobkind, @payment, @date)";

                // Create command
                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Add parameters to the command
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@jobkind", jobkind);
                    command.Parameters.AddWithValue("@payment", payment);
                    command.Parameters.AddWithValue("@date", date);

                    // Execute the command
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Data inserted successfully.");
                        LoadUserData();
                        LoadLineChartDatadefault();
                        textBox1.Text = "";
                        textBox2.Text = "";
                        textBox4.Text = "";

                        

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
}
