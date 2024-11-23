using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebGrease.Activities;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Vivify
{
    public partial class CombinedReport : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBranches(); // Load branches into ddlBranch
            }
        }

        private void LoadBranches()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            string query = "SELECT DISTINCT BranchName FROM Employees ORDER BY BranchName";

            DataTable branchesTable = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(branchesTable);
                    }
                }
            }

            ddlBranch.DataSource = branchesTable;
            ddlBranch.DataTextField = "BranchName";
            ddlBranch.DataValueField = "BranchName";
            ddlBranch.DataBind();

            ddlBranch.Items.Insert(0, new ListItem("Select a branch", ""));
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            string selectedBranch = ddlBranch.SelectedValue;
            DateTime fromDate, toDate;

            // Try parsing the FromDate and ToDate
            bool fromDateValid = DateTime.TryParse(txtFromDate.Text, out fromDate);
            bool toDateValid = DateTime.TryParse(txtToDate.Text, out toDate);

            // Clear previous error message
            lblError.Visible = false;

            // Check if the dates are valid
            if (fromDateValid && toDateValid)
            {
                // Check if FromDate is earlier or equal to ToDate
                if (fromDate <= toDate)
                {
                    try
                    {
                        DataTable exportData = LoadExportData(selectedBranch, fromDate, toDate); // You should create this method similarly
                        // Fetch filtered data for all reports based on selected Branch, FromDate, and ToDate
                        DataTable expenseData = LoadExpenseData(selectedBranch, fromDate, toDate);
                        
                        DataTable smoSoData = LoadSmoSoData(selectedBranch, fromDate, toDate); // You should create this method similarly

                        // Check if data has been returned for any of the reports
                        if (expenseData.Rows.Count > 0 || exportData.Rows.Count > 0 || smoSoData.Rows.Count > 0)
                        {
                            // Bind the fetched data to the respective GridViews
                            gvExpenseReport.DataSource = expenseData;
                            gvExpenseReport.DataBind();

                            gvExportReport.DataSource = exportData;
                            gvExportReport.DataBind();

                            gvSmoSoReport.DataSource = smoSoData;
                            gvSmoSoReport.DataBind();
                        }
                        else
                        {
                            // If no data is returned, display a message
                            lblError.Text = "No data found for the selected filters.";
                            lblError.Visible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Display error if there is an exception while fetching data
                        lblError.Text = "Error fetching data: " + ex.Message;
                        lblError.Visible = true; // Show the error label
                    }
                }
                else
                {
                    // Display error message if FromDate is later than ToDate
                    lblError.Text = "The 'From Date' must be earlier than or equal to the 'To Date'.";
                    lblError.Visible = true;
                }
            }
            else
            {
                // Display error message if the dates are not valid
                lblError.Text = "Please enter valid dates.";
                lblError.Visible = true;
            }
        }


        private DataTable LoadExpenseData(string branch, DateTime fromDate, DateTime toDate)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            string query = @"
    SELECT 
        exp.FirstName AS [EngineerName],  -- Alias 'FirstName' as 'EngineerName'
        ISNULL(SUM(CASE WHEN f.ExpenseType = 'Local' THEN f.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN c.ExpenseType = 'Local' THEN c.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN o.ExpenseType = 'Local' THEN o.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN m.ExpenseType = 'Local' THEN m.ClaimedAmount ELSE 0 END), 0) AS LocalExpenses,
        ISNULL(SUM(CASE WHEN f.ExpenseType = 'Tour' THEN f.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN c.ExpenseType = 'Tour' THEN c.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN m.ExpenseType = 'Tour' THEN m.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN l.ExpenseType = 'Tour' THEN l.ClaimedAmount ELSE 0 END), 0) AS TourExpenses,
        (ISNULL(SUM(CASE WHEN f.ExpenseType = 'Local' THEN f.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN c.ExpenseType = 'Local' THEN c.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN o.ExpenseType = 'Local' THEN o.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN m.ExpenseType = 'Local' THEN m.ClaimedAmount ELSE 0 END), 0)) + 
        (ISNULL(SUM(CASE WHEN f.ExpenseType = 'Tour' THEN f.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN c.ExpenseType = 'Tour' THEN c.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN m.ExpenseType = 'Tour' THEN m.ClaimedAmount ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN l.ExpenseType = 'Tour' THEN l.ClaimedAmount ELSE 0 END), 0)) AS OverallExpenses
    FROM 
        dbo.Expense exp
    LEFT JOIN 
        dbo.Food f ON exp.ServiceId = f.ServiceId
    LEFT JOIN 
        dbo.Conveyance c ON exp.ServiceId = c.ServiceId
    LEFT JOIN 
        dbo.Others o ON exp.ServiceId = o.ServiceId
    LEFT JOIN 
        dbo.Miscellaneous m ON exp.ServiceId = m.ServiceId
    LEFT JOIN 
        dbo.Lodging l ON exp.ServiceId = l.ServiceId
    INNER JOIN 
        dbo.Employees e ON exp.EmployeeID = e.EmployeeID
    WHERE 
        e.BranchName = @BranchName 
        AND exp.ServiceId IN (
            SELECT ServiceId 
            FROM dbo.Services 
            WHERE FromDate >= @FromDate AND ToDate <= @ToDate
        )
    GROUP BY 
        exp.EmployeeID, exp.FirstName
    ORDER BY 
        exp.FirstName;
    ";

            DataTable dataTable = new DataTable();

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Branchname", branch);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dataTable);
                    }
                }
            }

            // Add calculated columns for Local, Tour, Overall, and Overall Expenses
            dataTable.Columns.Add("TotalLocal", typeof(decimal));
            dataTable.Columns.Add("TotalTour", typeof(decimal));
            dataTable.Columns.Add("OverallTotal", typeof(decimal));
            dataTable.Columns.Add("Overall", typeof(decimal));  // Adding OverallExpenses column

            decimal grandTotalLocal = 0;
            decimal grandTotalTour = 0;
            decimal grandOverallTotal = 0;
            decimal grandOverallExpenses = 0;  // Track Overall Expenses Total

            // Iterate through each row to calculate totals and accumulate grand totals
            foreach (DataRow row in dataTable.Rows)
            {
                decimal totalLocal = 0;
                decimal totalTour = 0;

                // Use the correct column name 'EngineerName' from the SQL query alias
                string engineerName = row["EngineerName"].ToString();  // Use 'EngineerName', not 'Engineer Name'
                Console.WriteLine("Engineer Name: " + engineerName);  // Example usage

                decimal localExpenses = row["LocalExpenses"] != DBNull.Value ? Convert.ToDecimal(row["LocalExpenses"]) : 0;
                decimal tourExpenses = row["TourExpenses"] != DBNull.Value ? Convert.ToDecimal(row["TourExpenses"]) : 0;
                decimal overallExpenses = localExpenses + tourExpenses;  // Calculate Overall Expenses for the row

                totalLocal = localExpenses;
                totalTour = tourExpenses;

                row["TotalLocal"] = totalLocal;
                row["TotalTour"] = totalTour;
                row["OverallTotal"] = totalLocal + totalTour;
                row["OverallExpenses"] = overallExpenses;  // Set OverallExpenses for the row

                // Accumulate grand totals for Local, Tour, Overall, and Overall Expenses
                grandTotalLocal += totalLocal;
                grandTotalTour += totalTour;
                grandOverallTotal += totalLocal + totalTour;
                grandOverallExpenses += overallExpenses;  // Accumulate overall expenses
            }

            // Add a new row for the grand totals at the end of the DataTable
            DataRow totalRow = dataTable.NewRow();
            totalRow["EngineerName"] = "Total";  // Label for the grand total row
            totalRow["TotalLocal"] = grandTotalLocal;
            totalRow["TotalTour"] = grandTotalTour;
            totalRow["OverallTotal"] = grandOverallTotal;
            totalRow["OverallExpenses"] = grandOverallExpenses;  // Add Overall Expenses total

            // Add the grand total row to the DataTable
            dataTable.Rows.Add(totalRow);

            return dataTable;
        }



        protected DataTable LoadExportData(string branch, DateTime fromDate, DateTime toDate)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            // SQL Query to fetch necessary data including BranchName and RefreshAmount
            string query = @"
WITH LocalTotals AS (
    SELECT 
        SUM(ISNULL(conv.ClaimedAmount, 0)) AS TotalConveyance,
        SUM(ISNULL(lod.ClaimedAmount, 0)) AS TotalLodging,
        SUM(ISNULL(food.ClaimedAmount, 0)) AS TotalFood,
        SUM(ISNULL(others.ClaimedAmount, 0)) AS TotalOthers,
        SUM(ISNULL(misc.ClaimedAmount, 0)) AS TotalMiscellaneous,
        SUM(ISNULL(conv.ClaimedAmount, 0) + ISNULL(lod.ClaimedAmount, 0) + ISNULL(food.ClaimedAmount, 0) + ISNULL(others.ClaimedAmount, 0) + ISNULL(misc.ClaimedAmount, 0)) AS OverallLocalAmount
    FROM 
        Conveyance conv
    LEFT JOIN 
        Lodging lod ON conv.ServiceId = lod.ServiceId
    LEFT JOIN 
        Food food ON conv.ServiceId = food.ServiceId
    LEFT JOIN 
        Others others ON conv.ServiceId = others.ServiceId
    LEFT JOIN 
        Miscellaneous misc ON conv.ServiceId = misc.ServiceId
    LEFT JOIN 
        Services s ON conv.ServiceId = s.ServiceId
    LEFT JOIN 
        Employees e ON s.EmployeeId = e.EmployeeId
    WHERE 
        conv.ExpenseType = 'Local' 
        AND s.FromDate >= '2024-11-01' AND s.ToDate <= @ToDate
        AND e.BranchName = @BranchName
),
TourTotals AS (
    SELECT 
        SUM(ISNULL(conv.ClaimedAmount, 0)) AS TotalConveyance,
        SUM(ISNULL(lod.ClaimedAmount, 0)) AS TotalLodging,
        SUM(ISNULL(food.ClaimedAmount, 0)) AS TotalFood,
        SUM(ISNULL(others.ClaimedAmount, 0)) AS TotalOthers,
        SUM(ISNULL(misc.ClaimedAmount, 0)) AS TotalMiscellaneous,
        SUM(ISNULL(conv.ClaimedAmount, 0) + ISNULL(lod.ClaimedAmount, 0) + ISNULL(food.ClaimedAmount, 0) + ISNULL(others.ClaimedAmount, 0) + ISNULL(misc.ClaimedAmount, 0)) AS OverallTourAmount
    FROM 
        Conveyance conv
    LEFT JOIN 
        Lodging lod ON conv.ServiceId = lod.ServiceId
    LEFT JOIN 
        Food food ON conv.ServiceId = food.ServiceId
    LEFT JOIN 
        Others others ON conv.ServiceId = others.ServiceId
    LEFT JOIN 
        Miscellaneous misc ON conv.ServiceId = misc.ServiceId
    LEFT JOIN 
        Services s ON conv.ServiceId = s.ServiceId
    LEFT JOIN 
        Employees e ON s.EmployeeId = e.EmployeeId
    WHERE 
        conv.ExpenseType = 'Tour' 
        AND s.FromDate >= @FromDate AND s.ToDate <= @ToDate
        AND e.BranchName = @BranchName
),
SalesTotals AS (
    SELECT 
        SUM(ISNULL(conv.ClaimedAmount, 0)) AS TotalConveyance,
        SUM(ISNULL(food.ClaimedAmount, 0)) AS TotalFood,
        SUM(ISNULL(others.ClaimedAmount, 0)) AS TotalOthers,
        SUM(ISNULL(misc.ClaimedAmount, 0)) AS TotalMiscellaneous,
        SUM(ISNULL(lod.ClaimedAmount, 0)) AS TotalLodging,
        SUM(ISNULL(conv.ClaimedAmount, 0) + ISNULL(food.ClaimedAmount, 0) + ISNULL(others.ClaimedAmount, 0) + ISNULL(misc.ClaimedAmount, 0) + ISNULL(lod.ClaimedAmount, 0)) AS OverallSalesAmount
    FROM 
        Services s
    LEFT JOIN 
        Conveyance conv ON s.ServiceId = conv.ServiceId
    LEFT JOIN 
        Food food ON s.ServiceId = food.ServiceId
    LEFT JOIN 
        Others others ON s.ServiceId = others.ServiceId
    LEFT JOIN 
        Miscellaneous misc ON s.ServiceId = misc.ServiceId
    LEFT JOIN 
        Lodging lod ON s.ServiceId = lod.ServiceId
    LEFT JOIN 
        Employees e ON s.EmployeeId = e.EmployeeId
    WHERE 
        s.Department = 'Sales'
        AND s.FromDate >= @FromDate AND s.ToDate <= @ToDate
        AND e.BranchName = @BranchName
),
ServiceTotals AS (
    SELECT 
        SUM(ISNULL(conv.ClaimedAmount, 0)) AS TotalConveyance,
        SUM(ISNULL(food.ClaimedAmount, 0)) AS TotalFood,
        SUM(ISNULL(others.ClaimedAmount, 0)) AS TotalOthers,
        SUM(ISNULL(misc.ClaimedAmount, 0)) AS TotalMiscellaneous,
        SUM(ISNULL(lod.ClaimedAmount, 0)) AS TotalLodging,
        SUM(ISNULL(conv.ClaimedAmount, 0) + ISNULL(food.ClaimedAmount, 0) + ISNULL(others.ClaimedAmount, 0) + ISNULL(misc.ClaimedAmount, 0) + ISNULL(lod.ClaimedAmount, 0)) AS OverallServicesAmount
    FROM 
        Services s
    LEFT JOIN 
        Conveyance conv ON s.ServiceId = conv.ServiceId
    LEFT JOIN 
        Food food ON s.ServiceId = food.ServiceId
    LEFT JOIN 
        Others others ON s.ServiceId = others.ServiceId
    LEFT JOIN 
        Miscellaneous misc ON s.ServiceId = misc.ServiceId
    LEFT JOIN 
        Lodging lod ON s.ServiceId = lod.ServiceId
    LEFT JOIN 
        Employees e ON s.EmployeeId = e.EmployeeId
    WHERE 
        s.Department = 'Services'
        AND s.FromDate >= @FromDate AND s.ToDate <= @ToDate
        AND e.BranchName = @BranchName
),
RefreshTotals AS (
    SELECT 
        SUM(ISNULL(refresh.RefreshAmount, 0)) AS TotalRefreshment -- Fetching RefreshAmount directly from the Refreshment table
    FROM 
        Refreshment refresh
    LEFT JOIN
        Services s ON refresh.ServiceId = s.ServiceId
    LEFT JOIN 
        Employees e ON s.EmployeeId = e.EmployeeId
    WHERE 
        s.FromDate >= @FromDate AND s.ToDate <= @ToDate
        AND e.BranchName = @BranchName
)

SELECT 
    COALESCE(LocalTotals.OverallLocalAmount, 0) AS OverallLocalAmount,
    COALESCE(TourTotals.OverallTourAmount, 0) AS OverallTourAmount,
    COALESCE(LocalTotals.TotalConveyance, 0) + COALESCE(TourTotals.TotalConveyance, 0) + COALESCE(SalesTotals.TotalConveyance, 0) + COALESCE(ServiceTotals.TotalConveyance, 0) AS OverallConveyanceAmount,
    COALESCE(LocalTotals.TotalLodging, 0) + COALESCE(TourTotals.TotalLodging, 0) + COALESCE(SalesTotals.TotalLodging, 0) + COALESCE(ServiceTotals.TotalLodging, 0) AS OverallLodgingAmount,
    COALESCE(LocalTotals.TotalOthers, 0) + COALESCE(TourTotals.TotalOthers, 0) + COALESCE(SalesTotals.TotalOthers, 0) + COALESCE(ServiceTotals.TotalOthers, 0) AS OverallOthersAmount,
    COALESCE(LocalTotals.TotalFood, 0) + COALESCE(TourTotals.TotalFood, 0) + COALESCE(SalesTotals.TotalFood, 0) + COALESCE(ServiceTotals.TotalFood, 0) AS OverallFoodAmount,
    COALESCE(LocalTotals.TotalMiscellaneous, 0) + COALESCE(TourTotals.TotalMiscellaneous, 0) + COALESCE(SalesTotals.TotalMiscellaneous, 0) + COALESCE(ServiceTotals.TotalMiscellaneous, 0) AS OverallMiscellaneousAmount,
    COALESCE(SalesTotals.OverallSalesAmount, 0) AS OverallSalesAmount,
    COALESCE(ServiceTotals.OverallServicesAmount, 0) AS OverallServicesAmount,
    COALESCE(RefreshTotals.TotalRefreshment, 0) AS TotalRefreshment
FROM 
    LocalTotals
    CROSS JOIN TourTotals
    CROSS JOIN SalesTotals
    CROSS JOIN ServiceTotals
    CROSS JOIN RefreshTotals;
;";

            DataTable exportDataTable = new DataTable();
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BranchName", branch);  // Add BranchName as parameter

                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(exportDataTable);
                    }
                }
            }

            // Process the DataTable as needed and return

            // Create a new DataTable for row-wise display
            DataTable rowWiseData = new DataTable();
            rowWiseData.Columns.Add("ExpenseType", typeof(string));
            rowWiseData.Columns.Add("Amount", typeof(decimal));

            // Extract individual amounts from the fetched data
            decimal overallLocalAmount = Convert.ToDecimal(exportDataTable.Rows[0]["OverallLocalAmount"]);
            decimal overallTourAmount = Convert.ToDecimal(exportDataTable.Rows[0]["OverallTourAmount"]);
            decimal totalConveyance = Convert.ToDecimal(exportDataTable.Rows[0]["OverallConveyanceAmount"]);
            decimal totalLodging = Convert.ToDecimal(exportDataTable.Rows[0]["OverallLodgingAmount"]);
            decimal totalOthers = Convert.ToDecimal(exportDataTable.Rows[0]["OverallOthersAmount"]);
            decimal totalFood = Convert.ToDecimal(exportDataTable.Rows[0]["OverallFoodAmount"]);
            decimal totalMiscellaneous = Convert.ToDecimal(exportDataTable.Rows[0]["OverallMiscellaneousAmount"]);
            decimal totalSalesAmount = Convert.ToDecimal(exportDataTable.Rows[0]["OverallSalesAmount"]);
            decimal totalServicesAmount = Convert.ToDecimal(exportDataTable.Rows[0]["OverallServicesAmount"]);
            decimal totalRefreshment = Convert.ToDecimal(exportDataTable.Rows[0]["TotalRefreshment"]);

            // Add Local and Tour amounts individually
            rowWiseData.Rows.Add("Local", overallLocalAmount);
            rowWiseData.Rows.Add("Tour", overallTourAmount);

            // Calculate "Local&Tour Total" by summing the amounts
            decimal overallLocalTourTotal = overallLocalAmount + overallTourAmount;
            rowWiseData.Rows.Add("Local&Tour Total", overallLocalTourTotal);

            // Add an empty row after "Local & Tour Total"
            rowWiseData.Rows.Add("", 0); // Empty row

            // Add other individual category totals
            rowWiseData.Rows.Add("Conveyance", totalConveyance);
            rowWiseData.Rows.Add("Lodging", totalLodging);
            rowWiseData.Rows.Add("Others", totalOthers);
            rowWiseData.Rows.Add("Food", totalFood);
            rowWiseData.Rows.Add("Miscellaneous", totalMiscellaneous);

            // Calculate the total expense
            decimal totalExpense = totalConveyance + totalLodging + totalOthers + totalFood + totalMiscellaneous;
            rowWiseData.Rows.Add("ExpenseTotal", totalExpense);

            // Add an empty row after "ExpenseTotal"
            rowWiseData.Rows.Add("", 0); // Empty row

            // Add Sales, Services, and Refresh amounts
            rowWiseData.Rows.Add("Sales", totalSalesAmount);
            rowWiseData.Rows.Add("Services", totalServicesAmount);
            rowWiseData.Rows.Add("Refresh", totalRefreshment);

            // Calculate the grand total
            decimal grandTotal = totalSalesAmount + totalServicesAmount + totalRefreshment;
            rowWiseData.Rows.Add("Department Total", grandTotal);

            return rowWiseData;
        }


        protected void gvExportReport_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Check if the row is a data row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Apply thick border to "Local&Tour Total", "ExpenseTotal", and "Department Total" rows
                if (e.Row.Cells[0].Text == "Local&Tour Total" ||
                    e.Row.Cells[0].Text == "ExpenseTotal" ||
                    e.Row.Cells[0].Text == "Department Total")
                {
                    e.Row.CssClass = "thick-border";  // Apply the CSS class to the row
                }

                decimal localAmount = 0;
                decimal tourAmount = 0;

                // If the row is for Local, store the value in localAmount
                if (e.Row.Cells[0].Text == "Local")
                {
                    decimal.TryParse(e.Row.Cells[1].Text, out localAmount); // Local amount in the first column
                }

                // If the row is for Tour, store the value in tourAmount
                if (e.Row.Cells[0].Text == "Tour")
                {
                    decimal.TryParse(e.Row.Cells[1].Text, out tourAmount); // Tour amount in the second column
                }

                // Calculate and display the sum for Local + Tour in the "Overall Total" column
                if (e.Row.Cells[0].Text == "Local" || e.Row.Cells[0].Text == "Tour")
                {
                    if (e.Row.Cells[0].Text == "Local")
                    {
                        // Insert empty row between Local & Tour Total
                        e.Row.Cells[0].Text = "Local";
                        e.Row.Cells[1].Text = (localAmount + tourAmount).ToString("C"); // Format as currency
                    }

                    if (e.Row.Cells[0].Text == "Tour")
                    {
                        // Insert empty row between Local & Tour Total
                        e.Row.Cells[0].Text = "Tour";
                        e.Row.Cells[1].Text = (localAmount + tourAmount).ToString("C"); // Format as currency
                    }
                }
                // Sum the values for other categories
                if (e.Row.Cells[0].Text == "Conveyance" || e.Row.Cells[0].Text == "Lodging" || e.Row.Cells[0].Text == "Food" ||
                    e.Row.Cells[0].Text == "Others" || e.Row.Cells[0].Text == "Miscellaneous")
                {
                    decimal totalAmount = 0;
                    decimal.TryParse(e.Row.Cells[1].Text, out totalAmount);
                    e.Row.Cells[1].Text = totalAmount.ToString("C"); // Format as currency
                }
                // Handle Sales and Service rows, and add the totals in the last row
                if (e.Row.Cells[0].Text == "Sales" || e.Row.Cells[0].Text == "Service")
                {
                    e.Row.Cells[0].Text = "Overall Amount"; // Add for the overall category total
                    e.Row.Cells[1].Text = decimal.Parse(e.Row.Cells[1].Text).ToString("C"); // Format as currency
                }
            }
        }

        private DataTable LoadSmoSoData(string branch, DateTime fromDate, DateTime toDate)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            string query = @"
    WITH CombinedData AS (
        -- Combine data from all tables
        SELECT 
            conv.SmoNo,
            conv.SoNo,
            SUM(ISNULL(conv.ClaimedAmount, 0)) AS TotalClaimedAmount
        FROM Conveyance conv
        INNER JOIN Services srv ON conv.ServiceId = srv.ServiceId
        INNER JOIN Employees emp ON emp.EmployeeId = srv.EmployeeId  -- Join with Employee for BranchName
        WHERE srv.FromDate >= @FromDate 
          AND srv.ToDate <= @ToDate
          AND emp.BranchName = @BranchName
        GROUP BY conv.SmoNo, conv.SoNo
        
        UNION ALL
        
        SELECT 
            food.SmoNo,
            food.SoNo,
            SUM(ISNULL(food.ClaimedAmount, 0)) AS TotalClaimedAmount
        FROM Food food
        INNER JOIN Services srv ON food.ServiceId = srv.ServiceId
        INNER JOIN Employees emp ON emp.EmployeeId = srv.EmployeeId  -- Join with Employee for BranchName
        WHERE srv.FromDate >= @FromDate 
          AND srv.ToDate <= @ToDate
          AND emp.BranchName = @BranchName
        GROUP BY food.SmoNo, food.SoNo
        
        UNION ALL
        
        SELECT 
            lodg.SmoNo,
            lodg.SoNo,
            SUM(ISNULL(lodg.ClaimedAmount, 0)) AS TotalClaimedAmount
        FROM Lodging lodg
        INNER JOIN Services srv ON lodg.ServiceId = srv.ServiceId
        INNER JOIN Employees emp ON emp.EmployeeId = srv.EmployeeId  -- Join with Employee for BranchName
        WHERE srv.FromDate >= @FromDate 
          AND srv.ToDate <= @ToDate
          AND emp.BranchName = @BranchName
        GROUP BY lodg.SmoNo, lodg.SoNo
        
        UNION ALL
        
        SELECT 
            misc.SmoNo,
            misc.SoNo,
            SUM(ISNULL(misc.ClaimedAmount, 0)) AS TotalClaimedAmount
        FROM Miscellaneous misc
        INNER JOIN Services srv ON misc.ServiceId = srv.ServiceId
        INNER JOIN Employees emp ON emp.EmployeeId = srv.EmployeeId  -- Join with Employee for BranchName
        WHERE srv.FromDate >= @FromDate 
          AND srv.ToDate <= @ToDate
          AND emp.BranchName = @BranchName
        GROUP BY misc.SmoNo, misc.SoNo
        
        UNION ALL
        
        SELECT 
            oth.SmoNo,
            oth.SoNo,
            SUM(ISNULL(oth.ClaimedAmount, 0)) AS TotalClaimedAmount
        FROM Others oth
        INNER JOIN Services srv ON oth.ServiceId = srv.ServiceId
        INNER JOIN Employees emp ON emp.EmployeeId = srv.EmployeeId  -- Join with Employee for BranchName
        WHERE srv.FromDate >= @FromDate 
          AND srv.ToDate <= @ToDate
          AND emp.BranchName = @BranchName
        GROUP BY oth.SmoNo, oth.SoNo
    )
    SELECT 
        COALESCE(SmoNo, '') + ' - ' + COALESCE(SoNo, '') AS CombinedNo,
        SUM(TotalClaimedAmount) AS TotalClaimedAmount
    FROM 
        CombinedData
    GROUP BY 
        SmoNo, SoNo
    HAVING 
        SUM(TotalClaimedAmount) > 0
    ORDER BY 
        SmoNo, SoNo;
    ";

            DataTable smoSoDataTable = new DataTable();

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Add parameters for FromDate, ToDate, and BranchName
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BranchName", branch); // Add BranchName parameter

                    // Open connection and fill the DataTable
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(smoSoDataTable);
                    }
                }
            }

            // Add a total row for TotalClaimedAmount
            DataRow totalRow = smoSoDataTable.NewRow();
            totalRow["CombinedNo"] = "Total";
            totalRow["TotalClaimedAmount"] = smoSoDataTable.AsEnumerable().Sum(row => row.Field<decimal>("TotalClaimedAmount"));

            // Add the total row to the DataTable
            smoSoDataTable.Rows.Add(totalRow);

            return smoSoDataTable;
        }



        protected void gvSmoSoReport_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Check if this is the "Total" row
                if (e.Row.Cells[0].Text == "Total")
                {
                    // Apply bold style to all cells in the "Total" row
                    foreach (TableCell cell in e.Row.Cells)
                    {
                        cell.Font.Bold = true;
                    }
                }
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            // Create a new DataTable for each report
            DataTable exportData = LoadExportData(ddlBranch.SelectedValue, DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));
            DataTable expenseData = LoadExpenseData(ddlBranch.SelectedValue, DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));

            DataTable smoSoData = LoadSmoSoData(ddlBranch.SelectedValue, DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));

            // Export all reports to a single Excel file
            ExportToExcel(expenseData, "Expense Report", exportData, "Export Report", smoSoData, "SMO Report");
        }

        private void ExportToExcel(DataTable expenseData, string expenseSheetName, DataTable exportData, string exportSheetName, DataTable smoSoData, string smoSoSheetName)
        {
            // Set the license context for EPPlus (for non-commercial use)
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Clear the response to prepare for file download
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=CombinedReport.xlsx");
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            using (ExcelPackage package = new ExcelPackage())
            {
                // Create a new worksheet for the combined report
                ExcelWorksheet combinedWorksheet = package.Workbook.Worksheets.Add("Combined Report");

                // Start by adding the headers for the Expense Report
                int row = 1;
                combinedWorksheet.Cells[row, 1].LoadFromDataTable(expenseData, true);  // Adding the Expense Data
                row += expenseData.Rows.Count + 2;  // Add some space between reports

                // Now add the headers for the Export Report
                combinedWorksheet.Cells[row, 1].LoadFromDataTable(exportData, true);  // Adding the Export Data
                row += exportData.Rows.Count + 2;  // Add some space between reports

                // Finally, add the headers for the SMO Report
                combinedWorksheet.Cells[row, 1].LoadFromDataTable(smoSoData, true);  // Adding the SMO Data

                // Save the Excel package to a memory stream
                using (MemoryStream stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    stream.WriteTo(Response.OutputStream);
                }
            }

            // End the response and send the file to the client
            Response.End();
        }

        private void ExportToExcel(GridView gridView)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=CombinedReport.xls");
            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            gridView.AllowPaging = false;

            // Refresh data for export using LoadData
            LoadExportData(ddlBranch.SelectedValue, DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));
            LoadExpenseData(ddlBranch.SelectedValue, DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));
            LoadSmoSoData(ddlBranch.SelectedValue, DateTime.Parse(txtFromDate.Text), DateTime.Parse(txtToDate.Text));

            gridView.RenderControl(hw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
        }



        public override void VerifyRenderingInServerForm(Control control)
        {
            // This is required to allow GridView to render properly in certain scenarios
        }
    }
}