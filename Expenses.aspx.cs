using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Vivify
{
    public partial class Expenses : Page
    {
        // Declare DataTables at the class level
        private DataTable dtFood = new DataTable();
        private DataTable dtMiscellaneous = new DataTable();
        private DataTable dtOthers = new DataTable();
        private DataTable dtLodging = new DataTable();
        private DataTable dtConveyance = new DataTable(); // Added here
        private DataTable dtRefreshment = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //txtLocalFoodAmount.Text = "";
                //txtTourFoodAmount.Text = "";
                // Set GridView visibility to true by default
                GridViewFood.Visible = true;
                GridViewMiscellaneous.Visible = true;
                GridViewOthers.Visible = true;
                GridViewLodging.Visible = true;
                GridViewConveyance.Visible = true;
                InitializeControls();
                pnlLocalExpenses.Visible = false;
                pnlTourExpenses.Visible = false;
                lblError.Text = string.Empty;

                // Clear amounts for Others and Miscellaneous
                txtLocalMiscAmount.Text = string.Empty;
                txtLocalOthersAmount.Text = string.Empty;
                txtTourMiscAmount.Text = string.Empty;
                txtTourOthersAmount.Text = string.Empty;

                if (Session["ServiceId"] != null)
                {


                    int serviceId = (int)Session["ServiceId"];
                    DisplayExpenses(serviceId);

                }
                else
                {
                    Response.Redirect("Dashboard.aspx");
                }
            }
        }


        protected void InitializeControls()
        {
            pnlLocalExpenses.Visible = false;
            pnlTourExpenses.Visible = false;
            //lblError.Text = string.Empty;

            // Clear amounts for Others and Miscellaneous
            txtLocalMiscAmount.Text = string.Empty;
            txtLocalOthersAmount.Text = string.Empty;
            txtTourMiscAmount.Text = string.Empty;
            txtTourOthersAmount.Text = string.Empty;

            // Initialize dropdowns and textboxes
            ddlLocalExpenseType.SelectedIndex = -1;
            ddlTourExpenseType.SelectedIndex = -1;
            ddlLocalExpenseType.Enabled = false;
            ddlTourExpenseType.Enabled = false;
            ddlLocalMode.SelectedIndex = -1;
            ddlTourTransportMode.SelectedIndex = -1;
            ddlLocalMode.Enabled = false;
            ddlTourTransportMode.Enabled = false;
        }



        // Helper method to bind GridViews
        private void BindGridView(GridView gridView, DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                gridView.DataSource = dataTable;
                gridView.DataBind();
            }
            else
            {
                gridView.DataSource = null; // Clear if no data
                gridView.DataBind(); // Bind to refresh the GridView
            }
        }

        protected void DisplayExpenses(int serviceId)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            // Create DataTables to hold data from each table
            DataTable dtFood = new DataTable();
            DataTable dtMiscellaneous = new DataTable();
            DataTable dtOthers = new DataTable();
            DataTable dtLodging = new DataTable();
            DataTable dtConveyance = new DataTable();

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                // Fetch data from each table
                string[] queries = {
            "SELECT * FROM Food WHERE ServiceId = @ServiceId",
            "SELECT * FROM Miscellaneous WHERE ServiceId = @ServiceId",
            "SELECT * FROM Others WHERE ServiceId = @ServiceId",
            "SELECT * FROM Lodging WHERE ServiceId = @ServiceId",
            "SELECT * FROM Conveyance WHERE ServiceId = @ServiceId"
        };

                DataTable[] tables = { dtFood, dtMiscellaneous, dtOthers, dtLodging, dtConveyance };

                for (int i = 0; i < queries.Length; i++)
                {
                    using (SqlCommand cmd = new SqlCommand(queries[i], con))
                    {
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tables[i]);
                        }
                    }
                }
            }

            // Initialize totals
            decimal totalConveyance = BindGridAndCalculateTotal(GridViewConveyance, dtConveyance, lblTotalLocalConveyance, "Total Conveyance:");
            decimal totalFood = BindGridAndCalculateTotal(GridViewFood, dtFood, lblTotalLocalFood, "Total Food:");
            decimal totalMisc = BindGridAndCalculateTotal(GridViewMiscellaneous, dtMiscellaneous, lblTotalLocalMisc, "Total Miscellaneous:");
            decimal totalOthers = BindGridAndCalculateTotal(GridViewOthers, dtOthers, lblTotalLocalOthers, "Total Others:");
            decimal totalLodging = BindGridAndCalculateTotal(GridViewLodging, dtLodging, lblTotalLodging, "Total Lodging:");

            // Calculate overall total
            decimal overallTotal = totalConveyance + totalFood + totalMisc + totalOthers + totalLodging;

            // Assuming localAmount and tourAmount are calculated or retrieved here
            decimal localAmount = totalFood; // Example calculation
            decimal tourAmount = totalConveyance; // Example calculation

            // Display overall total
            lblTotalReimbursement.Text = $"Overall Total: {overallTotal:N2}"; // Assuming you have a label for overall total

            // Upsert the totals into the Expense table
            UpsertExpenseTotals(serviceId, totalConveyance, totalFood, totalMisc, totalOthers, totalLodging, overallTotal, localAmount, tourAmount);
        }

        private decimal BindGridAndCalculateTotal(GridView gridView, DataTable dataTable, Label totalLabel, string totalText)
        {
            decimal totalAmount = 0;

            // Bind data to GridView
            gridView.DataSource = dataTable;
            gridView.DataBind();

            if (dataTable.Rows.Count > 0)
            {
                // Calculate total amount, using a nullable type to handle DBNull
                totalAmount = dataTable.AsEnumerable()
                    .Sum(row => row.Field<decimal?>("Amount") ?? 0); // Use nullable decimal and default to 0 for DBNull

                // Update the total label with the formatted amount
                totalLabel.Text = $"{totalText} {totalAmount:N2}"; // Format total with descriptive text
            }
            else
            {
                totalLabel.Text = $"{totalText} 0.00"; // Handle the case where there are no rows
            }

            // Optionally call to bind footer totals if required
            BindFooterTotal(gridView, dataTable);

            return totalAmount; // Return the total amount for overall total calculation
        }

        private void BindFooterTotal(GridView gridView, DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                // Use a nullable type and sum only non-null values
                decimal totalAmount = dataTable.AsEnumerable()
                    .Sum(row => row.Field<decimal?>("Amount") ?? 0); // Use nullable decimal

                // Set footer values
                GridViewRow footer = gridView.FooterRow;
                if (footer != null)
                {
                    // Format as currency
                    footer.Cells[1].Text = totalAmount.ToString("C2"); // Adjust index if necessary
                }
            }
        }


        private void UpsertExpenseTotals(int serviceId, decimal totalConveyance, decimal totalFood, decimal totalMisc, decimal totalOthers, decimal totalLodging, decimal overallTotal, decimal localAmount, decimal tourAmount)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            // Corrected SQL query to fetch EmployeeId, FirstName, and Advance
            string upsertExpenseSql = @"
DECLARE @EmployeeId INT, @FirstName NVARCHAR(100), @Advance DECIMAL(18, 2);

-- Fetch EmployeeId and FirstName from Employees table
SELECT @EmployeeId = e.EmployeeId, @FirstName = e.FirstName
FROM Employees e
INNER JOIN Services s ON e.EmployeeId = s.EmployeeId  -- Use Services table (plural)
WHERE s.ServiceId = @ServiceId;

-- Fetch Advance from Services table (plural)
SELECT @Advance = s.Advance  -- Make sure column name is 'Advance'
FROM Services s  -- Using Services table (plural)
WHERE s.ServiceId = @ServiceId;

-- Upsert the Expense record
IF EXISTS (SELECT 1 FROM Expense WHERE ServiceId = @ServiceId)
BEGIN
    UPDATE Expense
    SET ConveyanceTotal = @ConveyanceTotal,
        FoodTotal = @FoodTotal,
        MiscellaneousTotal = @MiscellaneousTotal,
        OthersTotal = @OthersTotal,
        LodgingTotal = @LodgingTotal,
        Total = @TotalAmount,
        LocalAmount = @LocalAmount,
        TourAmount = @TourAmount,
        EmployeeId = @EmployeeId,
        FirstName = @FirstName,
        Advance = @Advance
    WHERE ServiceId = @ServiceId;
END
ELSE
BEGIN
    INSERT INTO Expense (ServiceId, ConveyanceTotal, FoodTotal, MiscellaneousTotal, OthersTotal, LodgingTotal, Total, LocalAmount, TourAmount, EmployeeId, FirstName, Advance)
    VALUES (@ServiceId, @ConveyanceTotal, @FoodTotal, @MiscellaneousTotal, @OthersTotal, @LodgingTotal, @TotalAmount, @LocalAmount, @TourAmount, @EmployeeId, @FirstName, @Advance);
END";


            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                using (SqlCommand cmdUpsertExpense = new SqlCommand(upsertExpenseSql, con))
                {
                    // Add the parameters for the expense details
                    cmdUpsertExpense.Parameters.AddWithValue("@ServiceId", serviceId);
                    cmdUpsertExpense.Parameters.AddWithValue("@ConveyanceTotal", totalConveyance);
                    cmdUpsertExpense.Parameters.AddWithValue("@FoodTotal", totalFood);
                    cmdUpsertExpense.Parameters.AddWithValue("@MiscellaneousTotal", totalMisc);
                    cmdUpsertExpense.Parameters.AddWithValue("@OthersTotal", totalOthers);
                    cmdUpsertExpense.Parameters.AddWithValue("@LodgingTotal", totalLodging);
                    cmdUpsertExpense.Parameters.AddWithValue("@TotalAmount", overallTotal);
                    cmdUpsertExpense.Parameters.AddWithValue("@LocalAmount", localAmount);
                    cmdUpsertExpense.Parameters.AddWithValue("@TourAmount", tourAmount);

                    // Execute the SQL command
                    cmdUpsertExpense.ExecuteNonQuery();
                }
            }





            // Manage visibility based on selected values
            string expenseType = ddlExpenseType.SelectedValue;
            string localExpenseType = ddlLocalExpenseType.SelectedValue;
            string tourExpenseType = ddlTourExpenseType.SelectedValue;

            if (expenseType == "Local")
            {
                if (localExpenseType == "Food")
                {
                    GridViewFood.Visible = true;
                }
                else if (localExpenseType == "Miscellaneous")
                {
                    GridViewMiscellaneous.Visible = true;
                }
                else if (localExpenseType == "Others")
                {
                    GridViewOthers.Visible = true;
                }
                else if (localExpenseType == "Conveyance")
                {
                    GridViewConveyance.Visible = true;
                }
            }
            else if (expenseType == "Tour")
            {
                // Show only the GridView corresponding to the selected tour expense type
                if (tourExpenseType == "Food")
                {
                    GridViewFood.Visible = true;
                }
                else if (tourExpenseType == "Miscellaneous")
                {
                    GridViewMiscellaneous.Visible = true;
                }
                else if (tourExpenseType == "Others")
                {
                    GridViewOthers.Visible = true;
                }
                else if (tourExpenseType == "Conveyance")
                {
                    GridViewConveyance.Visible = true;
                }
                else if (tourExpenseType == "Lodging")
                {
                    GridViewLodging.Visible = true;
                }
            }
        }


        private int GetServiceId()
        {
            // Assuming ServiceId is stored in the session
            if (Session["ServiceId"] != null)
            {
                return Convert.ToInt32(Session["ServiceId"]);
            }
            else
            {
                // Handle cases where ServiceId is not found in the session
                // For example, throw an exception or return a default value
                throw new InvalidOperationException("ServiceId not found in session.");
            }
        }


        protected void ddlExpenseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            // Hide all panels initially
            pnlLocalExpenses.Visible = false;
            pnlTourExpenses.Visible = false;

            // Hide all local and tour panels initially
            pnlLocalFoodFields.Visible = false;
            pnlLocalMiscellaneousFields.Visible = false;
            pnlLocalOthersFields.Visible = false;
            pnlLocalConvenience.Visible = false;

            pnlBikeFields.Visible = false;
            pnlCabFields.Visible = false;

            pnlTourFoodFields.Visible = false;
            pnlTourMiscellaneousFields.Visible = false;
            pnlTourOthersFields.Visible = false;
            pnlTourConvenience.Visible = false;

            pnlFlightFields.Visible = false;
            pnlBusFields.Visible = false;
            pnlTrainFields.Visible = false;
            pnlcabTourFields.Visible = false;

            // Show or hide panels based on the selected expense type
            if (ddlExpenseType.SelectedValue == "Local")
            {
                pnlLocalExpenses.Visible = true;
                ddlLocalExpenseType.Enabled = true; // Enable Local expense type dropdown
            }
            else if (ddlExpenseType.SelectedValue == "Tour")
            {
                pnlTourExpenses.Visible = true;
                ddlTourExpenseType.Enabled = true; // Enable Tour expense type dropdown
            }
            else
            {
                ddlLocalExpenseType.Enabled = false; // Disable Local expense type dropdown for other types
                ddlTourExpenseType.Enabled = false; // Disable Tour expense type dropdown for other types
            }

            // Ensure to refresh the display based on current selections
            int serviceId = GetServiceId(); // Implement this method to get the current ServiceId
            DisplayExpenses(serviceId);
        }

        protected void ddlLocalExpenseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int serviceId = GetServiceId(); // Implement this method to get the current ServiceId
            DisplayExpenses(serviceId);
            string selectedValue = ddlLocalExpenseType.SelectedValue;

            // Hide all local expense panels initially
            pnlLocalFoodFields.Visible = false;
            pnlLocalMiscellaneousFields.Visible = false;
            pnlLocalOthersFields.Visible = false;
            pnlLocalConvenience.Visible = false;


            // Show the relevant panel based on the selected expense type
            switch (selectedValue)
            {
                case "Food":
                    pnlLocalFoodFields.Visible = true;
                    txtLocalFoodAmount.Text = ""; // Clear the amount field
                    totalAccumulatedHours = 0; // Reset accumulated hours
                    break;
                case "Miscellaneous":
                    pnlLocalMiscellaneousFields.Visible = true;
                    break;
                case "Others":
                    pnlLocalOthersFields.Visible = true;
                    break;

                case "Conveyance":
                    pnlLocalConvenience.Visible = true;
                    ddlLocalMode.Enabled = true; // Enable Local mode dropdown
                    break;
            }
        }



        // Dummy method to demonstrate fetching existing dates



        protected void txtFromTime_TextChanged(object sender, EventArgs e)
        {
            CalculateFoodAmount();
        }

        protected void txtToTime_TextChanged(object sender, EventArgs e)
        {
            CalculateFoodAmount();
        }

        private double totalAccumulatedHours = 0;

        private void CalculateFoodAmount()
        {
            TimeSpan fromTimeSpan;
            TimeSpan toTimeSpan;

            // Try to parse the From and To time inputs as TimeSpan
            bool fromParsed = TimeSpan.TryParse(txtLocalFoodFromTime.Text, out fromTimeSpan);
            bool toParsed = TimeSpan.TryParse(txtLocalFoodToTime.Text, out toTimeSpan);

            if (fromParsed && toParsed)
            {
                // Calculate total hours worked
                double totalHoursWorked = (toTimeSpan - fromTimeSpan).TotalHours;

                // Handle crossing over midnight
                if (totalHoursWorked < 0)
                {
                    totalHoursWorked += 24; // Assume toTime is on the next day
                }

                // Add to accumulated hours
                totalAccumulatedHours += totalHoursWorked;

                // Determine amount based on accumulated hours worked
                if (totalAccumulatedHours >= 12)
                {
                    txtLocalFoodAmount.Text = "300"; // Amount for 12 hours or more
                }
                else
                {
                    txtLocalFoodAmount.Text = "150"; // Amount for less than 12 hours
                }
            }
            else
            {
                txtLocalFoodAmount.Text = ""; // Clear amount field for invalid input
            }
        }

        protected void ddlLocalMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hide all panels first
            pnlBikeFields.Visible = false;
            pnlCabFields.Visible = false;
            pnlAutoFields.Visible = false;

            // Show the selected panel
            switch (ddlLocalMode.SelectedValue)
            {
                case "Bike":
                    pnlBikeFields.Visible = true;
                    break;
                case "Cab/Bus":
                    pnlCabFields.Visible = true;
                    break;
                case "Auto":
                    pnlAutoFields.Visible = true;
                    break;
                default:
                    // Optionally handle the case when no valid selection is made
                    break;
            }
        }
        protected void txtLocalDistance_TextChanged(object sender, EventArgs e)
        {
            if (ddlLocalMode.SelectedValue == "Bike")
            {
                CalculateAmount(txtLocalDistance.Text, 13.5, txtLocalAmount); // Example rate for bike
            }
            else if (ddlLocalMode.SelectedValue == "Auto")
            {
                CalculateAmount(txtLocalAutoDistance.Text, 13.5, txtLocalAutoAmount); // Rate for auto
            }
        }

        private void CalculateAmount(string distanceInput, double ratePerKm, TextBox amountTextBox)
        {
            if (double.TryParse(distanceInput, out double distance))
            {
                double amount = distance * ratePerKm;
                amountTextBox.Text = amount.ToString("0.00");
            }
            else
            {
                amountTextBox.Text = "0.00"; // Reset if input is invalid
            }
        }

        protected void ddlTourExpenseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            int serviceId = GetServiceId(); // Implement this method to get the current ServiceId
            DisplayExpenses(serviceId);

            string selectedValue = ddlTourExpenseType.SelectedValue;

            // Hide all tour expense panels initially
            pnlTourFoodFields.Visible = false;
            pnlTourMiscellaneousFields.Visible = false;
            pnlTourOthersFields.Visible = false;
            pnlTourConvenience.Visible = false;

            // Show panels based on selected tour expense type
            if (selectedValue == "Food")
            {
                pnlTourFoodFields.Visible = true;
            }
            else if (selectedValue == "Miscellaneous")
            {
                pnlTourMiscellaneousFields.Visible = true;
            }
            else if (selectedValue == "Lodging")
            {
                pnlTourOthersFields.Visible = true;
            }
            else if (selectedValue == "Conveyance")
            {
                pnlTourConvenience.Visible = true;
                ddlTourTransportMode.Enabled = true; // Enable Tour mode dropdown
            }
            else
            {
                // Hide all panels if no valid selection
                pnlTourFoodFields.Visible = false;
                pnlTourMiscellaneousFields.Visible = false;
                pnlTourOthersFields.Visible = false;
                pnlTourConvenience.Visible = false;
            }
        }
        protected void txtTourFoodFromTime_TextChanged(object sender, EventArgs e)
        {
            CalculateTourFoodAmount();
        }

        protected void ddlTourFoodDesignation_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateTourFoodAmount();
        }

        protected void txtTourFoodToTime_TextChanged(object sender, EventArgs e)
        {
            CalculateTourFoodAmount();
        }

        private void CalculateTourFoodAmount()
        {
            string selectedValue = txtTourFoodDesignation.SelectedValue;
            double designatedAmount = 0;

            // Set the designated amount based on the selected designation
            switch (selectedValue)
            {
                case "FSE":
                    designatedAmount = 1000;
                    break;
                case "FST":
                    designatedAmount = 850;
                    break;
                default:
                    txtTourFoodAmount.Text = "Select a valid designation";
                    return;
            }

            // Check if From and To times are provided
            if (!string.IsNullOrWhiteSpace(txtTourFoodFromTime.Text) && !string.IsNullOrWhiteSpace(txtTourFoodToTime.Text))
            {
                DateTime fromTime;
                DateTime toTime;

                // Attempt to parse the time values directly from the TextBoxes
                bool fromParsed = DateTime.TryParse(txtTourFoodFromTime.Text, out fromTime);
                bool toParsed = DateTime.TryParse(txtTourFoodToTime.Text, out toTime);

                // Check if parsing failed
                if (!fromParsed || !toParsed)
                {
                    txtTourFoodAmount.Text = "Invalid time format. Use hh:mm AM/PM.";
                    return;
                }

                // Handle crossing over midnight
                if (toTime < fromTime)
                {
                    toTime = toTime.AddDays(1); // Assume toTime is on the next day
                }

                // Calculate total hours worked
                double totalHoursWorked = (toTime - fromTime).TotalHours;
                double amountToDisplay = 0;

                // Determine amount based on total hours worked and designation
                if (totalHoursWorked <= 12)
                {
                    if (selectedValue == "FSE")
                    {
                        amountToDisplay = 500; // Half amount for FSE
                    }
                    else if (selectedValue == "FST")
                    {
                        amountToDisplay = 425; // Half amount for FST
                    }
                }
                else
                {
                    amountToDisplay = designatedAmount; // Full amounts for 12 hours or more
                }

                // Set the amount in the text box
                txtTourFoodAmount.Text = amountToDisplay > 0 ? amountToDisplay.ToString("F2") : string.Empty;
            }
            else
            {
                txtTourFoodAmount.Text = "Enter both From and To times.";
            }
        }

        protected void ddlTourTransportMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMode = ddlTourTransportMode.SelectedValue;

            // Hide all transport panels initially
            pnlFlightFields.Visible = false;
            pnlBusFields.Visible = false;
            pnlTrainFields.Visible = false;
            pnlcabTourFields.Visible = false;
            pnlAutoTourFields.Visible = false;

            // Show the selected transport panel
            switch (selectedMode)
            {
                case "Flight":
                    pnlFlightFields.Visible = true;
                    break;
                case "Bus":
                    pnlBusFields.Visible = true;
                    break;
                case "Train":
                    pnlTrainFields.Visible = true;
                    break;
                case "Cab":
                    pnlcabTourFields.Visible = true;
                    break;
                case "Auto":
                    pnlAutoTourFields.Visible = true;
                    break;
                default:
                    break;
            }
        }
        protected void txtAutoTourDistance_TextChanged(object sender, EventArgs e)
        {
            // Assuming a rate of 13.5 per km for Auto
            CalculateTourAmount(txtTourAutoDistance.Text, 13.5, txtTourAutoAmount);
        }

        private void CalculateTourAmount(string distanceInput, double ratePerKm, TextBox amountTextBox)
        {
            if (double.TryParse(distanceInput, out double distance))
            {
                double amount = distance * ratePerKm;
                amountTextBox.Text = amount.ToString("0.00");
            }
            else
            {
                amountTextBox.Text = "0.00"; // Reset if input is invalid
            }
        }
        protected void txtLocalMiscItem_TextChanged(object sender, EventArgs e)
        {
            // Example logic: Display the text of the TextBox in a label (or any other logic you need)
            // lblMiscItem.Text = txtLocalMiscItem.Text;
        }


        private decimal GetAdvanceAmount(SqlConnection con, SqlTransaction transaction, int serviceId, int employeeId)
        {
            string getAdvanceSql = @"
        SELECT Advance
        FROM Services 
        WHERE ServiceId = @ServiceId AND EmployeeId = @EmployeeId";

            using (SqlCommand cmdGetAdvance = new SqlCommand(getAdvanceSql, con, transaction))
            {
                cmdGetAdvance.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdGetAdvance.Parameters.AddWithValue("@EmployeeId", employeeId);
                object advanceResult = cmdGetAdvance.ExecuteScalar();

                return advanceResult != null ? Convert.ToDecimal(advanceResult) : 0;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            lblError.Text = ""; // Clear previous messages

            if (Session["ServiceId"] == null)
            {
                lblError.Text = "Invalid Service ID.";
                return;
            }

            int serviceId = (int)Session["ServiceId"];
            int employeeId;
            const int maxRetries = 3;
            int retryCount = 0;
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                while (retryCount < maxRetries)
                {
                    try
                    {
                        con.Open();
                        break;
                    }
                    catch (SqlException)
                    {
                        retryCount++;
                        Thread.Sleep(1000);
                    }
                }

                if (retryCount == maxRetries)
                {
                    lblError.Text = "Failed to connect to database after retries.";
                    return; // Exit the method if connection fails after retries
                }

                // Retrieve EmployeeId based on ServiceId
                string getEmployeeIdSql = @"
SELECT EmployeeId 
FROM Services 
WHERE ServiceId = @ServiceId";

                using (SqlCommand cmdGetEmployeeId = new SqlCommand(getEmployeeIdSql, con))
                {
                    cmdGetEmployeeId.Parameters.AddWithValue("@ServiceId", serviceId);
                    object result = cmdGetEmployeeId.ExecuteScalar();
                    if (result != null)
                    {
                        employeeId = Convert.ToInt32(result);
                    }
                    else
                    {
                        lblError.Text = "Service ID not found.";
                        return;
                    }
                }

                // Validate expenses
                bool hasLocalExpenses = CheckLocalExpenses();
                bool hasTourExpenses = CheckTourExpenses();

                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Insert expenses
                        InsertExpenses(con, transaction, serviceId);

                        // Fetch advance amount
                        decimal advanceAmount = GetAdvanceAmount(con, transaction, serviceId, employeeId);

                        // Commit the transaction
                        transaction.Commit();

                        // Display the expenses (calculate and show the overall total)
                        DisplayExpenses(serviceId);

                        lblError.Text = "Data saved successfully.";

                    }
                    catch (SqlException sqlEx)
                    {
                        // Rollback if there's an error
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (InvalidOperationException) { }

                        // Check for duplicate entry error
                        if (sqlEx.Number == 2627)
                        {
                            lblError.Text = "Refreshment is already added for the specified date range.";
                        }
                        else
                        {
                            lblError.Text = $"SQL Error: {sqlEx.Message}";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback if there's an error
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (InvalidOperationException) { }

                        lblError.Text = $"Error: {ex.Message}";
                    }

                }
                ClearExpenseFields();
            }
        }


        protected void btnChangeStatus_Click(object sender, EventArgs e)
        {
            lblStatusError.Text = ""; // Clear previous messages
            if (Session["ServiceId"] == null)
            {
                lblError.Text = "Invalid Service ID.";
                return;
            }

            int serviceId = (int)Session["ServiceId"];
            int employeeId;

            // Retrieve EmployeeId based on ServiceId
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                string getEmployeeIdSql = @"
  SELECT EmployeeId 
  FROM Services 
  WHERE ServiceId = @ServiceId";

                using (SqlCommand cmdGetEmployeeId = new SqlCommand(getEmployeeIdSql, con))
                {
                    cmdGetEmployeeId.Parameters.AddWithValue("@ServiceId", serviceId);
                    object result = cmdGetEmployeeId.ExecuteScalar();
                    if (result != null)
                    {
                        employeeId = Convert.ToInt32(result);
                    }
                    else
                    {
                        lblStatusError.Text = "Service ID not found.";
                        return;
                    }
                }

                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    bool transactionCompleted = false; // Flag to track transaction status

                    try
                    {
                        // Update the status to "Reimbursement Submitted" with status ID 2
                        UpdateStatus(transaction, serviceId, employeeId, 2, "Reimbursement Submitted");

                        // Commit the transaction
                        transaction.Commit();
                        transactionCompleted = true; // Mark transaction as completed
                        lblStatusError.Text = "Status changed to 'Reimbursement Submitted' successfully.";
                        lblStatusError.Visible = true;

                    }
                    catch (SqlException sqlEx)
                    {
                        lblStatusError.Text = "SQL Error: " + sqlEx.Message; // Capture SQL errors
                    }
                    catch (Exception ex)
                    {
                        lblStatusError.Text = "Error changing status: " + ex.Message; // General error
                    }
                    finally
                    {
                        if (!transactionCompleted) // Only rollback if not already committed
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
        }
        private void InsertExpenses(SqlConnection con, SqlTransaction transaction, int serviceId)
        {
            // Inserting logic for expenses based on type
            if (ddlExpenseType.SelectedValue == "Local")
            {
                if (!string.IsNullOrEmpty(txtLocalOthersAmount.Text))
                {
                    // Get SMO No, Ref No, and SO No values from their respective input fields
                    string smoNo = txtLocalOthersSMONo.Text;
                    string refNo = txtLocalOthersRefNo.Text;
                    string soNo = txtLocalOthersSoNo.Text;

                    // Convert file upload to byte array
                    byte[] fileUploadLocalBillBytes = null;
                    if (fileUploadLocalBill.HasFile)
                    {
                        fileUploadLocalBillBytes = GetFileBytes(fileUploadLocalBill); // Convert FileUpload control to byte array
                    }

                    byte[] serviceReport = null;
                    if (fileServiceReport.HasFile)
                    {
                        // Convert the uploaded file into byte array (service report)
                        serviceReport = GetFileBytes(fileServiceReport);
                    }

                    // Call InsertOthersExpense with the serviceReport byte array
                    InsertOthersExpense(
                        con,
                        transaction,
                        serviceId,
                        txtLocalOthersAmount.Text,
                        txtLocalOthersDate.Text,
                        txtLocalOthersFromTime.Text,
                        txtLocalOthersToTime.Text,
                        txtLocalOthersParticulars.Text,
                        txtLocalOthersRemarks.Text,
                        fileUploadLocalBillBytes,   // Pass the byte array
                        othersfileUploadApproval,   // Pass the approval file upload control
                        smoNo,
                        refNo,
                        soNo,
                        serviceReport            // Pass the service report as byte array
                    );
                }







                if (!string.IsNullOrEmpty(txtLocalMiscItem.Text))
                {
                    // Parse times from the input fields
                    TimeSpan fromTime = TimeSpan.Parse(txtLocalMiscFromTime.Text);
                    TimeSpan toTime = TimeSpan.Parse(txtLocalMiscToTime.Text);

                    // Retrieve the values for SMO No, Ref No, and SO No from the input fields
                    string smoNo = txtLocalMiscSMONo.Text; // New SMO No input
                    string refNo = txtLocalMiscRefNo.Text; // New Ref No input
                    string soNo = txtLocalMiscSONo.Text;   // SO No input

                    // Assuming 'fileUploadLocalMiscellaneous' is for the bill, and 'fileUploadApproval' is for the approval mail
                    InsertMiscellaneousExpense(
                        con,
                        transaction,
                        serviceId,
                        txtLocalMiscItem.Text,                     // Purchased Item
                        txtLocalMiscAmount.Text,                   // Amount
                        txtLocalMiscDate.Text,                     // Date
                        fileUploadLocalMiscellaneous,              // File upload for the bill
                        "Local",                                   // Expense Type
                        fromTime,                                  // From Time
                        toTime,                                    // To Time
                        txtLocalMiscParticulars.Text,              // Particulars
                        txtLocalMiscRemarks.Text,                  // Remarks
                        smoNo,                                     // SMO No
                        refNo,                                     // Ref No
                        soNo                                       // SO No
                    );
                }


                if (!string.IsNullOrEmpty(txtLocalFoodAmount.Text))
                {
                    TimeSpan? fromTime = string.IsNullOrEmpty(txtLocalFoodFromTime.Text) ? (TimeSpan?)null : TimeSpan.Parse(txtLocalFoodFromTime.Text);
                    TimeSpan? toTime = string.IsNullOrEmpty(txtLocalFoodToTime.Text) ? (TimeSpan?)null : TimeSpan.Parse(txtLocalFoodToTime.Text);

                    // Assuming you have text boxes for SMO No, Ref No, and SONO
                    string smoNo = txtLocalSMONo.Text; // Change this to the actual control ID
                    string refNo = txtLocalRefNo.Text; // Change this to the actual control ID
                    string soNo = txtLocalFoodSONo.Text; // Add this line to get SONO value

                    InsertFoodExpense(
                        con,
                        transaction,
                        serviceId,
                        "Local",
                        txtLocalFoodAmount.Text,
                        txtLocalFoodDate.Text,
                        DBNull.Value,
                        fromTime,
                        toTime,
                        txtLocalFoodParticulars.Text,
                        txtLocalFoodRemarks.Text,
                        smoNo,
                        refNo,
                        soNo // Pass the SONO value here
                    );
                }

                if (ddlLocalMode.SelectedValue == "Bike")
                {
                    string bikeDistance = txtLocalDistance.Text; // Capture distance for Bike
                    InsertConveyanceExpense(con, transaction, serviceId, "Bike", txtLocalAmount.Text, txtLocalBikeDate.Text,
                                            txtLocalBikeFromTime.Text, txtLocalBikeToTime.Text, txtLocalBikeParticular.Text,
                                            txtLocalBikeRemarks.Text, null, "Local", bikeDistance,
                                            txtLocalBikeSMONo.Text, txtLocalBikeRefNo.Text, txtLocalBikeSONo.Text); // Pass SMO No, Ref No, and SO No
                }
                else if (ddlLocalMode.SelectedValue == "Cab/Bus")
                {
                    InsertConveyanceExpense(con, transaction, serviceId, "Cab/Bus", txtLocalCabAmount.Text, txtLocalCabDate.Text,
                                            txtLocalCabFromTime.Text, txtLocalCabToTime.Text, txtLocalCabParticular.Text, txtLocalCabRemarks.Text,
                                            fileUploadLocalCab, "Local", null,
                                            txtLocalCabSMONo.Text, txtLocalCabRefNo.Text, txtLocalCabSONo.Text); // Pass SMO No, Ref No, and SO No
                }
                else if (ddlLocalMode.SelectedValue == "Auto" && !string.IsNullOrEmpty(txtLocalAutoAmount.Text))
                {
                    decimal autoAmount = Convert.ToDecimal(txtLocalAutoAmount.Text);
                    string autoDistance = txtLocalAutoDistance.Text; // Capture distance for Auto
                    InsertConveyanceExpense(con, transaction, serviceId, "Auto", autoAmount.ToString(), txtLocalAutoDate.Text,
                                            txtLocalAutoFromTime.Text, txtLocalAutoToTime.Text, txtLocalAutoParticular.Text,
                                            txtLocalAutoRemarks.Text, txtfileUploadLocalAuto, "Local", autoDistance,
                                            txtLocalAutoSMONo.Text, txtLocalAutoRefNo.Text, txtLocalAutoSONo.Text); // Pass SMO No, Ref No, and SO No
                }

            }


            else if (ddlExpenseType.SelectedValue == "Tour")
            {
                if (!string.IsNullOrEmpty(txtTourOthersAmount.Text))
                {
                    // Get SMO No, Ref No, and SO No values from the text boxes
                    string smoNo = txtTourOthersSmoNo.Text;
                    string refNo = txtTourOthersRefNo.Text;
                    string soNo = txtTourOthersSoNo.Text;

                    // Get the byte array for the Service Report file
                    byte[] serviceReport = null;
                    if (fileUploadServiceReport.HasFile)
                    {
                        serviceReport = GetFileBytes(fileUploadServiceReport);
                    }

                    // Call InsertLodgingExpense method
                    InsertLodgingExpense(
                        con,
                        transaction,
                        serviceId,
                        txtTourOthersAmount.Text,
                        txtTourOthersDate.Text,
                        txtFromTimeTourOthers.Text,
                        txtToTimeTourOthers.Text,
                        txtParticularsTourOthers.Text,
                        txtRemarksTourOthers.Text,
                        fileUploadTourOthers,
                         fileUploadTourApproval,// File Upload (for bill or image)
                        smoNo,                // SMO No
                        refNo,                // Ref No
                        soNo,                 // SO No
                        serviceReport         // Service Report byte array
                    );
                }



                if (!string.IsNullOrEmpty(txtTourMiscItem.Text))
                {
                    // Parse the from and to times from the TextBox controls
                    TimeSpan fromTime = TimeSpan.Parse(txtTourMiscFromTime.Text);
                    TimeSpan toTime = TimeSpan.Parse(txtTourMiscToTime.Text);

                    // Retrieve values for SMO No, Ref No, and SO No
                    string smoNo = txtTourMiscSmoNo.Text; // SMO No input
                    string refNo = txtTourMiscRefNo.Text; // Ref No input
                    string soNo = txtTourMiscSoNo.Text;   // SO No input

                    // Call the InsertMiscellaneousExpense method, passing all required parameters including the approval mail file upload
                    InsertMiscellaneousExpense(
                        con,
                        transaction,
                        serviceId,
                        txtTourMiscItem.Text,                // Purchased Item
                        txtTourMiscAmount.Text,              // Amount
                        txtTourMiscDate.Text,                // Date
                        fileUploadTourMiscellaneous,         // File upload for the bill
                                                             // File upload for the approval mail
                        "Tour",                              // Expense Type
                        fromTime,                            // From Time
                        toTime,                              // To Time
                        txtTourMiscParticulars.Text,         // Particulars
                        txtTourMiscRemarks.Text,             // Remarks
                        smoNo,                               // SMO No
                        refNo,                               // Ref No
                        soNo                                  // SO No
                    );
                }


                if (!string.IsNullOrEmpty(txtTourFoodAmount.Text))
                {
                    TimeSpan? fromTime = string.IsNullOrEmpty(txtTourFoodFromTime.Text) ? (TimeSpan?)null : TimeSpan.Parse(txtTourFoodFromTime.Text);
                    TimeSpan? toTime = string.IsNullOrEmpty(txtTourFoodToTime.Text) ? (TimeSpan?)null : TimeSpan.Parse(txtTourFoodToTime.Text);

                    // Assuming you have text boxes for SMO No and Ref No
                    string smoNo = txtTourFoodSMONo.Text; // Change this to the actual control ID
                    string refNo = txtTourFoodRefNo.Text; // Change this to the actual control ID
                    string soNo = txtTourFoodSONo.Text;
                    InsertFoodExpense(
                        con,
                        transaction,
                        serviceId,
                        "Tour",
                        txtTourFoodAmount.Text,
                        txtTourFoodDate.Text,
                        txtTourFoodDesignation.SelectedValue,
                        fromTime,
                        toTime,
                        txtTourFoodParticulars.Text,
                        txtTourFoodRemarks.Text,
                        smoNo,
                        refNo,
                        soNo
                    );
                }


                string distance = null; // Default distance to null

                if (ddlTourTransportMode.SelectedValue == "Cab")
                {
                    string smoNo = txtCabSmoNo.Text; // Capture SMO No
                    string refNo = txtCabRefNo.Text; // Capture Ref No
                    string soNo = txtCabSoNo.Text;
                    InsertConveyanceExpense(con, transaction, serviceId, "Cab", txtCabAmount.Text, txtCabDate.Text,
                                            txtFromTimeCab.Text, txtToTimeCab.Text, txtParticularsCab.Text, txtRemarksCab.Text,
                                            fileUploadCab, "Tour", distance, smoNo, refNo, soNo); // Pass SMO No and Ref No
                }
                else if (ddlTourTransportMode.SelectedValue == "Train")
                {
                    string smoNo = txtTrainSmoNo.Text; // Capture SMO No
                    string refNo = txtTrainRefNo.Text; // Capture Ref No
                    string soNo = txtTrainSoNo.Text;
                    InsertConveyanceExpense(con, transaction, serviceId, "Train", txtTrainAmount.Text, txtTrainDate.Text,
                                            txtFromTimeTrain.Text, txtToTimeTrain.Text, txtParticularsTrain.Text, txtRemarksTrain.Text,
                                            fileUploadTrain, "Tour", distance, smoNo, refNo, soNo); // Pass SMO No and Ref No
                }
                else if (ddlTourTransportMode.SelectedValue == "Flight")
                {
                    string smoNo = txtFlightSmoNo.Text; // Capture SMO No
                    string refNo = txtFlightRefNo.Text; // Capture Ref No
                    string soNo = txtFlightSoNo.Text;
                    InsertConveyanceExpense(con, transaction, serviceId, "Flight", txtFlightAmount.Text,
                                            txtFlightDate.Text, txtFlightFromTime.Text, txtFlightToTime.Text,
                                            txtFlightParticulars.Text, txtFlightRemarks.Text, fileUploadFlight, "Tour", distance, smoNo, refNo, soNo); // Pass SMO No and Ref No
                }
                else if (ddlTourTransportMode.SelectedValue == "Bus")
                {
                    string smoNo = txtBusSmoNo.Text; // Capture SMO No
                    string refNo = txtBusRefNo.Text; // Capture Ref No 
                    string soNo = txtBusSoNo.Text;
                    InsertConveyanceExpense(con, transaction, serviceId, "Bus", txtBusAmount.Text,
                                            txtBusDate.Text, txtFromTimeBus.Text, txtToTimeBus.Text,
                                            txtParticularsBus.Text, txtRemarksBus.Text, fileUploadBus, "Tour", distance, smoNo, refNo, soNo); // Pass SMO No and Ref No
                }
                else if (ddlTourTransportMode.SelectedValue == "Auto" && !string.IsNullOrEmpty(txtTourAutoAmount.Text))
                {
                    decimal autoAmount = Convert.ToDecimal(txtTourAutoAmount.Text);
                    string autoDistance = txtTourAutoDistance.Text; // Capture distance for Auto
                    string smoNo = txtTourAutoSmoNo.Text; // Capture SMO No
                    string refNo = txtTourAutoRefNo.Text; // Capture Ref No
                    string soNo = txtTourAutoSoNo.Text;
                    InsertConveyanceExpense(con, transaction, serviceId, "Auto", txtTourAutoAmount.Text, txtTourAutoDate.Text,
                                            txtTourAutoFromTime.Text, txtTourAutoToTime.Text, txtTourAutoParticular.Text,
                                            txTourAutoRemarks.Text, fileUploadTourAuto, "Tour", autoDistance, smoNo, refNo, soNo); // Pass SMO No and Ref No
                }


            }
        }



        private bool CheckLocalExpenses()
        {
            return !string.IsNullOrEmpty(txtLocalFoodAmount.Text) ||
                   !string.IsNullOrEmpty(txtLocalMiscAmount.Text) ||
                   !string.IsNullOrEmpty(txtLocalOthersAmount.Text) ||
                   !string.IsNullOrEmpty(txtLocalAmount.Text);
        }

        private bool CheckTourExpenses()
        {
            return !string.IsNullOrEmpty(txtTourFoodAmount.Text) ||
                   !string.IsNullOrEmpty(txtTourMiscAmount.Text) ||
                   !string.IsNullOrEmpty(txtTourOthersAmount.Text) ||
                   !string.IsNullOrEmpty(txtFlightAmount.Text) ||
                   !string.IsNullOrEmpty(txtBusAmount.Text) ||
                   !string.IsNullOrEmpty(txtTrainAmount.Text) ||
                   !string.IsNullOrEmpty(txtCabAmount.Text);
        }

        private void InsertFoodExpense(
        SqlConnection con,
        SqlTransaction transaction,
        int serviceId,
        string expenseType,
        string amount,
        string date,
        object designation,
        TimeSpan? fromTime,
        TimeSpan? toTime,
        string particulars,
        string remarks,
        string smoNo, // Parameter for SMO No
        string refNo, // Parameter for Ref No
        string soNo   // Parameter for SONO
    )
        {
            // Check for mandatory fields
            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(date))
            {
                throw new ArgumentException("Amount and Date are required fields.");
            }

            string sqlFood = @"
INSERT INTO Food (ServiceId, ExpenseType, Designation, Date, Amount, FromTime, ToTime, Particulars, Remarks, Smono, Refno, Sono) 
SELECT @ServiceId, @ExpenseType, @Designation, @Date, @Amount, @FromTime, @ToTime, @Particulars, @Remarks, @Smono, @Refno, @Sono
WHERE NOT EXISTS (
    SELECT 1 FROM Food WHERE ServiceId = @ServiceId AND ExpenseType = @ExpenseType AND Date = @Date
)";

            using (SqlCommand cmdFood = new SqlCommand(sqlFood, con, transaction))
            {
                // Add parameters with null checks for nullable types
                cmdFood.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdFood.Parameters.AddWithValue("@ExpenseType", expenseType);
                cmdFood.Parameters.AddWithValue("@Designation", designation ?? DBNull.Value);
                cmdFood.Parameters.AddWithValue("@Date", DateTime.Parse(date));
                cmdFood.Parameters.AddWithValue("@Amount", Convert.ToDecimal(amount));
                cmdFood.Parameters.AddWithValue("@FromTime", fromTime.HasValue ? (object)fromTime.Value : DBNull.Value);
                cmdFood.Parameters.AddWithValue("@ToTime", toTime.HasValue ? (object)toTime.Value : DBNull.Value);
                cmdFood.Parameters.AddWithValue("@Particulars", string.IsNullOrEmpty(particulars) ? (object)DBNull.Value : particulars);
                cmdFood.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);

                // Parameters for SMO No, Ref No, and SONO
                cmdFood.Parameters.AddWithValue("@Smono", string.IsNullOrEmpty(smoNo) ? (object)DBNull.Value : smoNo);
                cmdFood.Parameters.AddWithValue("@Refno", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo);
                cmdFood.Parameters.AddWithValue("@Sono", string.IsNullOrEmpty(soNo) ? (object)DBNull.Value : soNo);

                // Execute the command
                cmdFood.ExecuteNonQuery();
            }
        }




        private void InsertMiscellaneousExpense(
         SqlConnection con,
         SqlTransaction transaction,
         int serviceId,
         string item,
         string amount,
         string date,
         FileUpload fileUpload,        // File upload for the bill
         string expenseType,
         TimeSpan fromTime,
         TimeSpan toTime,
         string particulars,
         string remarks,
         string smoNo,  // New parameter for SMO No
         string refNo,   // New parameter for Ref No
         string soNo     // New parameter for SO No
     )
        {
            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(date))
            {
                return; // Validation: Ensure amount and date are not empty
            }

            // SQL query to insert data into Miscellaneous table
            string sqlMisc = @"
    INSERT INTO Miscellaneous (ServiceId, ExpenseType, PurchasedItem, Amount, Date, Image, FromTime, ToTime, Particulars, Remarks, Smono, Refno, SoNo) 
    SELECT @ServiceId, @ExpenseType, @Item, @Amount, @Date, @Image, @FromTime, @ToTime, @Particulars, @Remarks, @Smono, @Refno, @SoNo
    WHERE NOT EXISTS (
        SELECT 1 FROM Miscellaneous WHERE ServiceId = @ServiceId AND PurchasedItem = @Item AND Date = @Date
    )";

            // Creating SQL command to execute
            using (SqlCommand cmdMisc = new SqlCommand(sqlMisc, con, transaction))
            {
                // Adding parameters for the other fields
                cmdMisc.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdMisc.Parameters.AddWithValue("@ExpenseType", expenseType);
                cmdMisc.Parameters.AddWithValue("@Item", item);
                cmdMisc.Parameters.AddWithValue("@Amount", string.IsNullOrEmpty(amount) ? (object)DBNull.Value : Convert.ToDecimal(amount));
                cmdMisc.Parameters.AddWithValue("@Date", string.IsNullOrEmpty(date) ? (object)DBNull.Value : DateTime.Parse(date));

                // For the bill image, if the file is uploaded, convert it to byte array
                cmdMisc.Parameters.Add("@Image", SqlDbType.VarBinary).Value = fileUpload?.HasFile == true ? GetFileBytes(fileUpload) : (object)DBNull.Value;

                cmdMisc.Parameters.AddWithValue("@FromTime", fromTime);
                cmdMisc.Parameters.AddWithValue("@ToTime", toTime);
                cmdMisc.Parameters.AddWithValue("@Particulars", string.IsNullOrEmpty(particulars) ? (object)DBNull.Value : particulars);
                cmdMisc.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);

                // Add parameters for SMO No, Ref No, and SO No (optional)
                cmdMisc.Parameters.AddWithValue("@Smono", string.IsNullOrEmpty(smoNo) ? (object)DBNull.Value : smoNo);
                cmdMisc.Parameters.AddWithValue("@Refno", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo);
                cmdMisc.Parameters.AddWithValue("@SoNo", string.IsNullOrEmpty(soNo) ? (object)DBNull.Value : soNo);

                // Execute the SQL command to insert the data into the database
                cmdMisc.ExecuteNonQuery();
            }
        }


        private void InsertOthersExpense(
        SqlConnection con,
        SqlTransaction transaction,
        int serviceId,
        string amount,
        string date,
        string fromTime,
        string toTime,
        string particulars,
        string remarks,
        byte[] fileUploadLocalBill, // Accept byte[] for the file
        FileUpload fileUploadApproval, // FileUpload control for approval bill
        string smoNo, // SMO No
        string refNo, // Ref No
        string soNo, // SO No
        byte[] serviceReport // ServiceReport as byte[]
    )
        {
            // Validate the Amount and Date input fields
            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(date))
            {
                throw new Exception("Amount and Date are required.");
            }

            // Convert Amount to Decimal
            decimal parsedAmount;
            if (!decimal.TryParse(amount, out parsedAmount))
            {
                throw new Exception("Invalid amount format.");
            }

            // Convert Date to DateTime
            DateTime parsedDate;
            if (!DateTime.TryParse(date, out parsedDate))
            {
                throw new Exception("Invalid date format.");
            }

            // Convert FromTime and ToTime to TimeSpan
            TimeSpan parsedFromTime;
            if (!TimeSpan.TryParse(fromTime, out parsedFromTime))
            {
                throw new Exception("Invalid 'FromTime' format.");
            }

            TimeSpan parsedToTime;
            if (!TimeSpan.TryParse(toTime, out parsedToTime))
            {
                throw new Exception("Invalid 'ToTime' format.");
            }

            string sqlOthers = @"
        INSERT INTO Others (ServiceId, ExpenseType, Date, Amount, FromTime, ToTime, Particulars, Remarks, Image, SmoNo, RefNo, SoNo, ServiceReport, ApprovalMail) 
        SELECT @ServiceId, 'Local', @Date, @Amount, @FromTime, @ToTime, @Particulars, @Remarks, @Image, @SmoNo, @RefNo, @SoNo, @ServiceReport, @ApprovalMail
        WHERE NOT EXISTS (
            SELECT 1 FROM Others WHERE ServiceId = @ServiceId AND Date = @Date AND Amount = @Amount
        )";

            using (SqlCommand cmdOthers = new SqlCommand(sqlOthers, con, transaction))
            {
                cmdOthers.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdOthers.Parameters.AddWithValue("@Date", parsedDate); // Use parsed DateTime
                cmdOthers.Parameters.AddWithValue("@Amount", parsedAmount); // Use parsed decimal
                cmdOthers.Parameters.AddWithValue("@FromTime", parsedFromTime); // Use parsed TimeSpan
                cmdOthers.Parameters.AddWithValue("@ToTime", parsedToTime); // Use parsed TimeSpan
                cmdOthers.Parameters.AddWithValue("@Particulars", string.IsNullOrEmpty(particulars) ? (object)DBNull.Value : particulars);
                cmdOthers.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);

                // For the bill image, if the file is uploaded, convert it to byte array
                cmdOthers.Parameters.Add("@Image", SqlDbType.VarBinary).Value = fileUploadLocalBill ?? (object)DBNull.Value;

                // Add parameters for SMO No, Ref No, and SO No
                cmdOthers.Parameters.AddWithValue("@SmoNo", string.IsNullOrEmpty(smoNo) ? (object)DBNull.Value : smoNo);
                cmdOthers.Parameters.AddWithValue("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo);
                cmdOthers.Parameters.AddWithValue("@SoNo", string.IsNullOrEmpty(soNo) ? (object)DBNull.Value : soNo);

                // Add the ServiceReport (as byte array)
                cmdOthers.Parameters.Add("@ServiceReport", SqlDbType.VarBinary).Value = serviceReport ?? (object)DBNull.Value;

                // For the approval bill, if the file is uploaded, convert it to byte array
                cmdOthers.Parameters.Add("@ApprovalMail", SqlDbType.VarBinary).Value = fileUploadApproval.HasFile ? GetFileBytes(fileUploadApproval) : (object)DBNull.Value;

                try
                {
                    // Execute the SQL command to insert the data into the database
                    cmdOthers.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Log or display the error to debug further
                    throw new Exception("Error inserting Others expense: " + ex.Message);
                }
            }
        }



        private void InsertLodgingExpense(
       SqlConnection con,
       SqlTransaction transaction,
       int serviceId,
       string amount,
       string date,
       string fromTime,
       string toTime,
       string particulars,
       string remarks,
       FileUpload fileUpload,         // File upload for the bill
       FileUpload fileUploadTourApproval, // New parameter for the approval bill upload
       string smoNo,                  // SMO No
       string refNo,                  // Ref No
       string soNo,                   // SO No
       byte[] serviceReport           // New parameter for the byte array of the service report
   )
        {
            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(date))
            {
                return; // Ensure amount and date are not empty
            }

            // SQL query to insert data into the Lodging table
            string sqlLodging = @"
    INSERT INTO Lodging (ServiceId, ExpenseType, Date, Amount, FromTime, ToTime, Particulars, Remarks, Image, ServiceReport, ApprovalMail, SmoNo, RefNo, SoNo) 
    VALUES (@ServiceId, 'Tour', @Date, @Amount, @FromTime, @ToTime, @Particulars, @Remarks, @Image, @ServiceReport, @ApprovalMail, @SmoNo, @RefNo, @SoNo)";

            using (SqlCommand cmdLodging = new SqlCommand(sqlLodging, con, transaction))
            {
                cmdLodging.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdLodging.Parameters.AddWithValue("@Date", string.IsNullOrEmpty(date) ? (object)DBNull.Value : DateTime.Parse(date));
                cmdLodging.Parameters.AddWithValue("@Amount", string.IsNullOrEmpty(amount) ? (object)DBNull.Value : Convert.ToDecimal(amount));
                cmdLodging.Parameters.AddWithValue("@FromTime", string.IsNullOrEmpty(fromTime) ? (object)DBNull.Value : TimeSpan.Parse(fromTime));
                cmdLodging.Parameters.AddWithValue("@ToTime", string.IsNullOrEmpty(toTime) ? (object)DBNull.Value : TimeSpan.Parse(toTime));
                cmdLodging.Parameters.AddWithValue("@Particulars", string.IsNullOrEmpty(particulars) ? (object)DBNull.Value : particulars);
                cmdLodging.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);

                // Store the image (bill) file as byte array
                cmdLodging.Parameters.Add("@Image", SqlDbType.VarBinary).Value = fileUpload?.HasFile == true ? GetFileBytes(fileUpload) : (object)DBNull.Value;

                // Store the service report as byte array
                cmdLodging.Parameters.Add("@ServiceReport", SqlDbType.VarBinary).Value = serviceReport ?? (object)DBNull.Value;

                // Store the approval bill as byte array (if the file is uploaded)
                cmdLodging.Parameters.Add("@ApprovalMail", SqlDbType.VarBinary).Value = fileUploadTourApproval?.HasFile == true ? GetFileBytes(fileUploadTourApproval) : (object)DBNull.Value;

                // Add parameters for SMO No, Ref No, and SO No
                cmdLodging.Parameters.AddWithValue("@SmoNo", string.IsNullOrEmpty(smoNo) ? (object)DBNull.Value : smoNo);
                cmdLodging.Parameters.AddWithValue("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo);
                cmdLodging.Parameters.AddWithValue("@SoNo", string.IsNullOrEmpty(soNo) ? (object)DBNull.Value : soNo);

                // Execute the command to insert the data
                cmdLodging.ExecuteNonQuery();
            }
        }

        private void InsertConveyanceExpense(
     SqlConnection con,
     SqlTransaction transaction,
     int serviceId,
     string transportType,
     string amount,
     string date,
     string fromTime,
     string toTime,
     string particulars,
     string remarks,
     FileUpload fileUpload,
     string expenseType,
     string distance,
     string smoNo,  // New parameter for SMO No
     string refNo,  // New parameter for Ref No
     string soNo    // New parameter for SO No
 )
        {
            if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(date))
            {
                return; // Handle missing required values
            }

            string sqlConveyance = @"
    INSERT INTO Conveyance (ServiceId, TransportType, Amount, ExpenseType, Date, FromTime, ToTime, Particulars, Remarks, Distance, Image, SmoNo, RefNo, SoNo) 
    SELECT @ServiceId, @TransportType, @Amount, @ExpenseType, @Date, @FromTime, @ToTime, @Particulars, @Remarks, @Distance, @Image, @SmoNo, @RefNo, @SoNo
    WHERE NOT EXISTS (
        SELECT 1 FROM Conveyance WHERE ServiceId = @ServiceId AND TransportType = @TransportType AND Date = @Date
    )";

            using (SqlCommand cmdConveyance = new SqlCommand(sqlConveyance, con, transaction))
            {
                cmdConveyance.CommandTimeout = 120; // Increase timeout
                cmdConveyance.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdConveyance.Parameters.AddWithValue("@TransportType", transportType);
                cmdConveyance.Parameters.AddWithValue("@Amount", string.IsNullOrEmpty(amount) ? (object)DBNull.Value : Convert.ToDecimal(amount));
                cmdConveyance.Parameters.AddWithValue("@Date", string.IsNullOrEmpty(date) ? (object)DBNull.Value : DateTime.Parse(date));
                cmdConveyance.Parameters.AddWithValue("@FromTime", string.IsNullOrEmpty(fromTime) ? (object)DBNull.Value : TimeSpan.Parse(fromTime));
                cmdConveyance.Parameters.AddWithValue("@ToTime", string.IsNullOrEmpty(toTime) ? (object)DBNull.Value : TimeSpan.Parse(toTime));
                cmdConveyance.Parameters.AddWithValue("@Particulars", string.IsNullOrEmpty(particulars) ? (object)DBNull.Value : particulars);
                cmdConveyance.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);
                cmdConveyance.Parameters.AddWithValue("@ExpenseType", expenseType); // Ensure this is set
                cmdConveyance.Parameters.AddWithValue("@Distance", string.IsNullOrEmpty(distance) ? (object)DBNull.Value : Convert.ToDecimal(distance)); // Added distance parameter

                // Add parameters for SMO No, Ref No, and SO No
                cmdConveyance.Parameters.AddWithValue("@SmoNo", string.IsNullOrEmpty(smoNo) ? (object)DBNull.Value : smoNo);
                cmdConveyance.Parameters.AddWithValue("@RefNo", string.IsNullOrEmpty(refNo) ? (object)DBNull.Value : refNo);
                cmdConveyance.Parameters.AddWithValue("@SoNo", string.IsNullOrEmpty(soNo) ? (object)DBNull.Value : soNo); // SO No parameter

                cmdConveyance.Parameters.Add("@Image", SqlDbType.VarBinary).Value = fileUpload?.HasFile == true ? GetFileBytes(fileUpload) : (object)DBNull.Value;

                cmdConveyance.ExecuteNonQuery();
            }
        }





        private byte[] GetFileBytes(FileUpload fileUpload)
        {
            if (fileUpload.HasFile)
            {
                using (var binaryReader = new System.IO.BinaryReader(fileUpload.PostedFile.InputStream))
                {
                    return binaryReader.ReadBytes(fileUpload.PostedFile.ContentLength);
                }
            }
            return null;
        }





        private void UpdateStatus(SqlTransaction transaction, int serviceId, int employeeId, int statusId, string status)
        {
            string updateStatusSql = @"
        UPDATE Services
        SET StatusId = @StatusId, Status = @StatusDescription
        WHERE ServiceId = @ServiceId AND EmployeeId = @EmployeeId";

            using (SqlCommand cmdUpdateStatus = new SqlCommand(updateStatusSql, transaction.Connection, transaction))
            {
                cmdUpdateStatus.Parameters.AddWithValue("@ServiceId", serviceId);
                cmdUpdateStatus.Parameters.AddWithValue("@EmployeeId", employeeId);
                cmdUpdateStatus.Parameters.AddWithValue("@StatusId", statusId);
                cmdUpdateStatus.Parameters.AddWithValue("@StatusDescription", status);
                cmdUpdateStatus.ExecuteNonQuery();
            }
        }

        private void ClearExpenseFields()
        {
            // Capture the selected ExpenseType value before clearing fields
            string selectedExpenseType = ddlExpenseType.SelectedValue;

            // Local Expenses Fields
            txtLocalOthersAmount.Text = "";
            txtLocalOthersDate.Text = "";
            txtLocalOthersFromTime.Text = "";
            txtLocalOthersToTime.Text = "";
            txtLocalOthersParticulars.Text = "";
            txtLocalOthersRemarks.Text = "";
            txtLocalOthersSMONo.Text = "";
            txtLocalOthersRefNo.Text = "";
            txtLocalOthersSoNo.Text = "";

            txtLocalMiscItem.Text = "";
            txtLocalMiscAmount.Text = "";
            txtLocalMiscDate.Text = "";
            txtLocalMiscFromTime.Text = "";
            txtLocalMiscToTime.Text = "";
            txtLocalMiscParticulars.Text = "";
            txtLocalMiscRemarks.Text = "";
            txtLocalMiscSMONo.Text = "";
            txtLocalMiscRefNo.Text = "";
            txtLocalMiscSONo.Text = "";

            txtLocalFoodAmount.Text = "";
            txtLocalFoodDate.Text = "";
            txtLocalFoodFromTime.Text = "";
            txtLocalFoodToTime.Text = "";
            txtLocalFoodParticulars.Text = "";
            txtLocalFoodRemarks.Text = "";
            txtLocalSMONo.Text = "";
            txtLocalRefNo.Text = "";
            txtLocalFoodSONo.Text = "";

            txtLocalDistance.Text = "";
            txtLocalAmount.Text = "";
            txtLocalBikeDate.Text = "";
            txtLocalBikeFromTime.Text = "";
            txtLocalBikeToTime.Text = "";
            txtLocalBikeParticular.Text = "";
            txtLocalBikeRemarks.Text = "";
            txtLocalBikeSMONo.Text = "";
            txtLocalBikeSONo.Text = "";
            txtLocalBikeRefNo.Text = "";
            txtLocalCabDate.Text = "";
            txtLocalCabAmount.Text = "";
            txtLocalCabFromTime.Text = "";
            txtLocalCabToTime.Text = "";
            txtLocalCabParticular.Text = "";
            txtLocalCabRemarks.Text = "";
            txtLocalCabSMONo.Text = "";
            txtLocalCabSONo.Text = "";
            txtLocalCabRefNo.Text = "";
            txtLocalAutoAmount.Text = "";
            txtLocalAutoDate.Text = "";
            txtLocalAutoFromTime.Text = "";
            txtLocalAutoToTime.Text = "";
            txtLocalAutoParticular.Text = "";
            txtLocalAutoRemarks.Text = "";
            txtLocalAutoDistance.Text = "";
            txtLocalAutoSMONo.Text = "";
            txtLocalAutoRefNo.Text = "";
            txtLocalAutoSONo.Text = "";

            // Reset dropdowns for Local
            ddlLocalMode.SelectedIndex = 0;

            // Reset Tour Expenses Fields
            txtTourOthersAmount.Text = "";
            txtTourOthersDate.Text = "";
            txtFromTimeTourOthers.Text = "";
            txtToTimeTourOthers.Text = "";
            txtParticularsTourOthers.Text = "";
            txtRemarksTourOthers.Text = "";
            txtTourOthersSmoNo.Text = "";
            txtTourOthersRefNo.Text = "";
            txtTourOthersSoNo.Text = "";

            txtTourMiscItem.Text = "";
            txtTourMiscAmount.Text = "";
            txtTourMiscDate.Text = "";
            txtTourMiscFromTime.Text = "";
            txtTourMiscToTime.Text = "";
            txtTourMiscParticulars.Text = "";
            txtTourMiscRemarks.Text = "";
            txtTourMiscSmoNo.Text = "";
            txtTourMiscRefNo.Text = "";
            txtTourMiscSoNo.Text = "";

            txtTourFoodAmount.Text = "";
            txtTourFoodDate.Text = "";
            txtTourFoodFromTime.Text = "";
            txtTourFoodToTime.Text = "";
            txtTourFoodParticulars.Text = "";
            txtTourFoodRemarks.Text = "";
            txtTourFoodSMONo.Text = "";
            txtTourFoodRefNo.Text = "";
            txtTourFoodSONo.Text = "";

            // Reset Tour Conveyance Fields
            txtCabAmount.Text = "";
            txtCabDate.Text = "";
            txtFromTimeCab.Text = "";
            txtToTimeCab.Text = "";
            txtParticularsCab.Text = "";
            txtRemarksCab.Text = "";
            txtCabSmoNo.Text = "";
            txtCabRefNo.Text = "";
            txtCabSoNo.Text = "";

            txtTrainAmount.Text = "";
            txtTrainDate.Text = "";
            txtFromTimeTrain.Text = "";
            txtToTimeTrain.Text = "";
            txtParticularsTrain.Text = "";
            txtRemarksTrain.Text = "";
            txtTrainSmoNo.Text = "";
            txtTrainRefNo.Text = "";
            txtTrainSoNo.Text = "";

            txtFlightAmount.Text = "";
            txtFlightDate.Text = "";
            txtFlightFromTime.Text = "";
            txtFlightToTime.Text = "";
            txtFlightParticulars.Text = "";
            txtFlightRemarks.Text = "";
            txtFlightSmoNo.Text = "";
            txtFlightRefNo.Text = "";
            txtFlightSoNo.Text = "";

            txtBusAmount.Text = "";
            txtBusDate.Text = "";
            txtFromTimeBus.Text = "";
            txtToTimeBus.Text = "";
            txtParticularsBus.Text = "";
            txtRemarksBus.Text = "";
            txtBusSmoNo.Text = "";
            txtBusRefNo.Text = "";
            txtBusSoNo.Text = "";

            txtTourAutoAmount.Text = "";
            txtTourAutoDate.Text = "";
            txtTourAutoFromTime.Text = "";
            txtTourAutoToTime.Text = "";
            txtTourAutoParticular.Text = "";
            txTourAutoRemarks.Text = "";
            txtTourAutoDistance.Text = "";
            txtTourAutoSmoNo.Text = "";
            txtTourAutoRefNo.Text = "";
            txtTourAutoSoNo.Text = "";

            // Reset Transport Mode dropdown for Tour
            ddlTourTransportMode.SelectedIndex = 0;

            // After clearing all fields, reset the ExpenseType dropdown to the previously selected value
            if (!string.IsNullOrEmpty(selectedExpenseType))
            {
                ddlExpenseType.SelectedValue = selectedExpenseType; // Set the previously selected value
            }
        }
    }

}