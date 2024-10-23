using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Expense_Income_App
{
    public partial class dashboard12 : UserControl
    {
        public dashboard12()
        {
            InitializeComponent();
            DisplayIncomeData();
            DisplayExpensesData();
            DisplayProfitData();
            LoadPieChartByMonth();
            LoadLineChartDatadefault();
           
            //populate with months
            comboBoxMonths.Items.Clear();
            comboBoxMonths.Items.Add("All months");
            comboBoxMonths.SelectedIndex = 0;
            for (int month = 1; month <= 12; month++)
            {
                comboBoxMonths.Items.Add(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
            }

            // Attach the event handler for selected index change
            comboBoxMonths.SelectedIndexChanged += ComboBoxMonths_SelectedIndexChanged;

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
        public void DisplayIncomeData()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            try
            {
                // Connect to the database
                dbConnection.Connect();

                // SQL query to select all data from the Income table
                string query = "SELECT SUM(customer_payment) AS customer_payment FROM Customers;";

                // Create command
                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Execute the command and read data
                    SqlDataReader reader = command.ExecuteReader();

                    // Check if there are rows
                    if (reader.HasRows)
                    {
                        StringBuilder sb = new StringBuilder();
                        StringBuilder sb2 = new StringBuilder();
                        StringBuilder sb3 = new StringBuilder();
                        // Read each row and build a string to display
                        while (reader.Read())
                        {

                            sb.AppendLine($"{reader["customer_payment"]}");




                        }
                        // Display the value in the label
                        Income_Label.Text = sb.ToString();

                    }
                    else
                    {
                        // No rows found
                        Income_Label.Text = "No income data found";
                    }

                    reader.Close();
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
        public void DisplayExpensesData()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            try
            {
                dbConnection.Connect();

                // SQL query to select all data from the Income table
                string query = "SELECT SUM(expenses_amount) AS expenses_amount FROM Expenses;";

                // Create command
                using (SqlCommand command = new SqlCommand(query, DatabaseConnection.conn))
                {
                    // Execute the command and read data
                    SqlDataReader reader = command.ExecuteReader();

                    // Check if there are rows
                    if (reader.HasRows)
                    {
                        StringBuilder sb = new StringBuilder();

                        while (reader.Read())
                        {

                            sb.AppendLine($"{reader["expenses_amount"]}");




                        }
                        // Display the value in the label
                        Expenses_Label.Text = sb.ToString();

                    }
                    else
                    {
                        // No rows found
                        Expenses_Label.Text = "No expense data found";
                    }
                    reader.Close();
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
        private void DisplayProfitData()
        {
            DatabaseConnection dbConnection = new DatabaseConnection();

            string queryIncome = "SELECT SUM(customer_payment) FROM Customers";
            string queryExpenses = "SELECT SUM(expenses_amount) FROM Expenses";

            // Declare variables for storing the sums
            decimal totalIncome = 0;
            decimal totalExpenses = 0;
            decimal profit = 0;

            try
            {
                // Connect to the database
                dbConnection.Connect();

                // Retrieve total income
                using (SqlCommand cmdIncome = new SqlCommand(queryIncome, DatabaseConnection.conn)) 
                {
                    object resultIncome = cmdIncome.ExecuteScalar();
                    if (resultIncome != DBNull.Value)
                    {
                        totalIncome = Convert.ToDecimal(resultIncome);
                    }
                }

                // Retrieve total expenses
                using (SqlCommand cmdExpenses = new SqlCommand(queryExpenses, DatabaseConnection.conn)) 
                {
                    object resultExpenses = cmdExpenses.ExecuteScalar();
                    if (resultExpenses != DBNull.Value)
                    {
                        totalExpenses = Convert.ToDecimal(resultExpenses);
                    }
                }

                // Calculate the profit
                profit = totalIncome - totalExpenses;

                if (profit >= 0)
                {
                    Profit_Label.Text = profit.ToString();
                }
                else
                {
                    Profit_Label.Text = profit.ToString();
                }
            }
            catch (SqlException ex)
            {
                // Handle any SQL exceptions
                MessageBox.Show("An error occurred while connecting to the database: " + ex.Message);
            }
            finally
            {
                dbConnection.Disconnect();
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

                // SQL query to get the total expenses grouped by month
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

        private void LoadLineChartDatadefault()
        {
            // Create an instance of the database connection
            DatabaseConnection dbConnection = new DatabaseConnection();

            try
            {
                // Connect to the database
                dbConnection.Connect();
                
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
                dbConnection.Disconnect();
            }
        }

        private void ComboBoxMonths_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected month
            string selectedMonthName = comboBoxMonths.SelectedItem.ToString();

            if (selectedMonthName == "All months")
            {
                LoadLineChartDatadefault();
            }
            else
            {
                // Load chart data for the selected month
                LoadLineChartData(selectedMonthName);
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
                ExpensesDate ASC"; // Group by date and order by date

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

                            // Add data to the chart series (use the correct chart series here)
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
                // Load all data

                LoadPieChartByMonth();
            }
            else
            {
                // Load chart data for the selected month
                LoadPieChartData(selectedMonthName);
            }
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            // Refresh data on button click
            DisplayIncomeData();
            DisplayExpensesData();
            DisplayProfitData();
            LoadPieChartByMonth();
        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
