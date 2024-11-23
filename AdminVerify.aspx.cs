using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.IO;
using System.Diagnostics;

namespace Vivify
{
    public partial class AdminVerify : System.Web.UI.Page
    {
        private const string ConnectionStringName = "vivify";
        private const string ServiceIdSessionKey = "ServiceId";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session["TotalClaimable"] = 0m;
                Session["TotalNonClaimable"] = 0m;

                if (Session[ServiceIdSessionKey] is int serviceId)
                {
                    BindExpenseGridView(serviceId);
                }
                else
                {
                    lblMessage.Text = "Service ID not found in session.";
                    lblMessage.Visible = true;
                }
            }
        }

        private void BindExpenseGridView(int serviceId)
        {
            string constr = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string qry = @"
                   SELECT 
      C.Id AS ExpenseId,
      'Conveyance' AS SourceTable,
      c.ExpenseType,
      FORMAT(c.Date, 'dd/MMM/yyyy') AS Date,
      c.FromTime, 
      c.ToTime, 
      c.Particulars,
      c.Distance,     
      c.Amount AS ConveyanceAmount,
      NULL AS FoodAmount,
      NULL AS OtherAmount,
      NULL AS MiscellaneousAmount,
      NULL AS LodgingAmount,
      ISNULL(e.ClaimedAmount, 0) AS ClaimedAmount,
      ISNULL(e.NonClaimedAmount, 0) AS NonClaimedAmount,
      c.Remarks       
  FROM 
      Conveyance c
  LEFT JOIN Expense e ON e.ServiceId = c.ServiceId
  WHERE 
      c.ServiceId = @ServiceId
  UNION ALL
  SELECT 
      f.Id AS ExpenseId,
      'Food' AS SourceTable,
      f.ExpenseType, 
      FORMAT(f.Date, 'dd/MMM/yyyy') AS Date,
      f.FromTime, 
      f.ToTime, 
      f.Particulars,
      NULL AS Distance,
      NULL AS ConveyanceAmount,
      f.Amount AS FoodAmount,
      NULL AS OtherAmount,
      NULL AS MiscellaneousAmount,
      NULL AS LodgingAmount,
      ISNULL(e.ClaimedAmount, 0) AS ClaimedAmount,
      ISNULL(e.NonClaimedAmount, 0) AS NonClaimedAmount,
      f.Remarks       
  FROM 
      Food f
  LEFT JOIN Expense e ON e.ServiceId = f.ServiceId
  WHERE 
      f.ServiceId = @ServiceId
  UNION ALL
  SELECT 
      o.Id AS ExpenseId,
      'Others' AS SourceTable,
      o.ExpenseType, 
      FORMAT(o.Date, 'dd/MMM/yyyy') AS Date,
      o.FromTime, 
      o.ToTime, 
      o.Particulars,
      NULL AS Distance,
      NULL AS ConveyanceAmount,
      NULL AS FoodAmount,
      o.Amount AS OtherAmount,
      NULL AS MiscellaneousAmount,
      NULL AS LodgingAmount,
      ISNULL(e.ClaimedAmount, 0) AS ClaimedAmount,
      ISNULL(e.NonClaimedAmount, 0) AS NonClaimedAmount,
      o.Remarks       
  FROM 
      Others o
  LEFT JOIN Expense e ON e.ServiceId = o.ServiceId
  WHERE 
      o.ServiceId = @ServiceId
  UNION ALL
  SELECT 
      m.Id AS ExpenseId,
      'Miscellaneous' AS SourceTable,
      m.ExpenseType, 
      FORMAT(m.Date, 'dd/MMM/yyyy') AS Date,
      m.FromTime, 
      m.ToTime, 
      m.Particulars,
      NULL AS Distance,
      NULL AS ConveyanceAmount,
      NULL AS FoodAmount,
      NULL AS OtherAmount,
      m.Amount AS MiscellaneousAmount,
      NULL AS LodgingAmount,
      ISNULL(e.ClaimedAmount, 0) AS ClaimedAmount,
      ISNULL(e.NonClaimedAmount, 0) AS NonClaimedAmount,
      m.Remarks       
  FROM 
      Miscellaneous m
  LEFT JOIN Expense e ON e.ServiceId = m.ServiceId
  WHERE 
      m.ServiceId = @ServiceId
  UNION ALL
  SELECT 
      l.Id AS ExpenseId,
      'Lodging' AS SourceTable,
      l.ExpenseType, 
      FORMAT(l.Date, 'dd/MMM/yyyy') AS Date,
      l.FromTime, 
      l.ToTime, 
      l.Particulars,
      NULL AS Distance,
      NULL AS ConveyanceAmount,
      NULL AS FoodAmount,
      NULL AS OtherAmount,
      NULL AS MiscellaneousAmount,
      l.Amount AS LodgingAmount,
      ISNULL(e.ClaimedAmount, 0) AS ClaimedAmount,
      ISNULL(e.NonClaimedAmount, 0) AS NonClaimedAmount,
      l.Remarks       
  FROM 
      Lodging l
  LEFT JOIN Expense e ON e.ServiceId = l.ServiceId
  WHERE 
      l.ServiceId = @ServiceId";

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ExpenseGridView.DataSource = dt;
                        ExpenseGridView.DataBind();
                    }
                }
            }
        }
        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            // Ensure EPPlus LicenseContext is set
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Create DataTables for both claimed and non-claimed expenses
            DataTable dtClaimed = new DataTable();
            dtClaimed.Columns.Add("Source");
            dtClaimed.Columns.Add("ExpenseType");
            dtClaimed.Columns.Add("Date");
            dtClaimed.Columns.Add("FromTime");
            dtClaimed.Columns.Add("ToTime");
            dtClaimed.Columns.Add("Particulars");
            dtClaimed.Columns.Add("Distance");
            dtClaimed.Columns.Add("Remarks");
            dtClaimed.Columns.Add("Amount");

            DataTable dtNonClaimed = new DataTable();
            dtNonClaimed.Columns.Add("Source");
            dtNonClaimed.Columns.Add("ExpenseType");
            dtNonClaimed.Columns.Add("Date");
            dtNonClaimed.Columns.Add("FromTime");
            dtNonClaimed.Columns.Add("ToTime");
            dtNonClaimed.Columns.Add("Particulars");
            dtNonClaimed.Columns.Add("Distance");
            dtNonClaimed.Columns.Add("Remarks");
            dtNonClaimed.Columns.Add("Amount");

            // Loop through GridView rows to collect data based on checkbox state
            foreach (GridViewRow row in ExpenseGridView.Rows)
            {
                // Find checkboxes for each expense source type
                CheckBox chkConveyanceClaimable = (CheckBox)row.FindControl("chkConveyanceClaimable");
                CheckBox chkFoodClaimable = (CheckBox)row.FindControl("chkFoodClaimable");
                CheckBox chkOthersClaimable = (CheckBox)row.FindControl("chkOthersClaimable");
                CheckBox chkMiscellaneousClaimable = (CheckBox)row.FindControl("chkMiscellaneousClaimable");
                CheckBox chkLodgingClaimable = (CheckBox)row.FindControl("chkLodgingClaimable");

                // Collect data for the selected row
                string expenseType = row.Cells[1].Text;  // This is where "Local" or "Tour" would come from (Expense Type)
                string date = row.Cells[2].Text;

                // Collect FromTime and ToTime from Label controls, if available
                Label lblFromTime = (Label)row.FindControl("lblFromTime");
                Label lblToTime = (Label)row.FindControl("lblToTime");

                string fromTime = lblFromTime != null ? lblFromTime.Text : string.Empty;
                string toTime = lblToTime != null ? lblToTime.Text : string.Empty;

                // Collect the Particulars and Distance (from row cells or TextBox, if applicable)
                string particulars = row.Cells[5].Text;
                string distance = row.Cells[6].Text;
                if (string.IsNullOrWhiteSpace(distance) || distance == "&nbsp;")
                {
                    distance = "-";  // Set to hyphen if no value or only non-breaking space
                }
                else
                {
                    distance = distance.Trim();  // Trim any leading/trailing spaces, just in case
                }

                string remarks = row.Cells[7].Text;

                // Initialize Amount as empty string
                string amount = string.Empty;

                // Process Conveyance expenses
                if (chkConveyanceClaimable != null && chkConveyanceClaimable.Checked)
                {
                    TextBox txtConveyanceAmount = (TextBox)row.FindControl("txtConveyanceAmount");
                    amount = txtConveyanceAmount != null ? txtConveyanceAmount.Text : string.Empty;

                    DataRow claimedRow = dtClaimed.NewRow();
                    claimedRow["Source"] = "Conveyance";  // Source is "Conveyance"
                    claimedRow["ExpenseType"] = expenseType;  // ExpenseType can be "Local", "Tour", etc.
                    claimedRow["Date"] = date;
                    claimedRow["FromTime"] = fromTime;
                    claimedRow["ToTime"] = toTime;
                    claimedRow["Particulars"] = particulars;
                    claimedRow["Distance"] = distance;
                    claimedRow["Remarks"] = remarks;
                    claimedRow["Amount"] = amount;
                    dtClaimed.Rows.Add(claimedRow);
                }
                else
                {
                    TextBox txtConveyanceAmount = (TextBox)row.FindControl("txtConveyanceAmount");
                    amount = txtConveyanceAmount != null ? txtConveyanceAmount.Text : string.Empty;
                    if (!string.IsNullOrEmpty(amount) && decimal.TryParse(amount, out decimal parsedAmount) && parsedAmount > 0)
                    {
                        DataRow nonClaimedRow = dtNonClaimed.NewRow();
                        nonClaimedRow["Source"] = "Conveyance";
                        nonClaimedRow["ExpenseType"] = expenseType;
                        nonClaimedRow["Date"] = date;
                        nonClaimedRow["FromTime"] = fromTime;
                        nonClaimedRow["ToTime"] = toTime;
                        nonClaimedRow["Particulars"] = particulars;
                        nonClaimedRow["Distance"] = distance;
                        nonClaimedRow["Remarks"] = remarks;
                        nonClaimedRow["Amount"] = amount;
                        dtNonClaimed.Rows.Add(nonClaimedRow);
                    }
                }

                // Process Food expenses
                if (chkFoodClaimable != null && chkFoodClaimable.Checked)
                {
                    TextBox txtFoodAmount = (TextBox)row.FindControl("txtFoodAmount");
                    amount = txtFoodAmount != null ? txtFoodAmount.Text : string.Empty;

                    DataRow claimedRow = dtClaimed.NewRow();
                    claimedRow["Source"] = "Food";  // Source is "Food"
                    claimedRow["ExpenseType"] = expenseType;
                    claimedRow["Date"] = date;
                    claimedRow["FromTime"] = fromTime;
                    claimedRow["ToTime"] = toTime;
                    claimedRow["Particulars"] = particulars;
                    claimedRow["Distance"] = distance;
                    claimedRow["Remarks"] = remarks;
                    claimedRow["Amount"] = amount;
                    dtClaimed.Rows.Add(claimedRow);
                }
                else
                {
                    TextBox txtFoodAmount = (TextBox)row.FindControl("txtFoodAmount");
                    amount = txtFoodAmount != null ? txtFoodAmount.Text : string.Empty;
                    if (!string.IsNullOrEmpty(amount) && decimal.TryParse(amount, out decimal parsedAmount) && parsedAmount > 0)
                    {
                        DataRow nonClaimedRow = dtNonClaimed.NewRow();
                        nonClaimedRow["Source"] = "Food";
                        nonClaimedRow["ExpenseType"] = expenseType;
                        nonClaimedRow["Date"] = date;
                        nonClaimedRow["FromTime"] = fromTime;
                        nonClaimedRow["ToTime"] = toTime;
                        nonClaimedRow["Particulars"] = particulars;
                        nonClaimedRow["Distance"] = distance;
                        nonClaimedRow["Remarks"] = remarks;
                        nonClaimedRow["Amount"] = amount;
                        dtNonClaimed.Rows.Add(nonClaimedRow);
                    }
                }

                // Process Others expenses
                if (chkOthersClaimable != null && chkOthersClaimable.Checked)
                {
                    TextBox txtOthersAmount = (TextBox)row.FindControl("txtOthersAmount");
                    amount = txtOthersAmount != null ? txtOthersAmount.Text : string.Empty;

                    DataRow claimedRow = dtClaimed.NewRow();
                    claimedRow["Source"] = "Others";  // Source is "Others"
                    claimedRow["ExpenseType"] = expenseType;
                    claimedRow["Date"] = date;
                    claimedRow["FromTime"] = fromTime;
                    claimedRow["ToTime"] = toTime;
                    claimedRow["Particulars"] = particulars;
                    claimedRow["Distance"] = distance;
                    claimedRow["Remarks"] = remarks;
                    claimedRow["Amount"] = amount;
                    dtClaimed.Rows.Add(claimedRow);
                }
                else
                {
                    TextBox txtOthersAmount = (TextBox)row.FindControl("txtOthersAmount");
                    amount = txtOthersAmount != null ? txtOthersAmount.Text : string.Empty;
                    if (!string.IsNullOrEmpty(amount) && decimal.TryParse(amount, out decimal parsedAmount) && parsedAmount > 0)
                    {
                        DataRow nonClaimedRow = dtNonClaimed.NewRow();
                        nonClaimedRow["Source"] = "Others";
                        nonClaimedRow["ExpenseType"] = expenseType;
                        nonClaimedRow["Date"] = date;
                        nonClaimedRow["FromTime"] = fromTime;
                        nonClaimedRow["ToTime"] = toTime;
                        nonClaimedRow["Particulars"] = particulars;
                        nonClaimedRow["Distance"] = distance;
                        nonClaimedRow["Remarks"] = remarks;
                        nonClaimedRow["Amount"] = amount;
                        dtNonClaimed.Rows.Add(nonClaimedRow);
                    }
                }

                // Process Miscellaneous expenses
                if (chkMiscellaneousClaimable != null && chkMiscellaneousClaimable.Checked)
                {
                    TextBox txtMiscellaneousAmount = (TextBox)row.FindControl("txtMiscellaneousAmount");
                    amount = txtMiscellaneousAmount != null ? txtMiscellaneousAmount.Text : string.Empty;

                    DataRow claimedRow = dtClaimed.NewRow();
                    claimedRow["Source"] = "Miscellaneous";  // Source is "Miscellaneous"
                    claimedRow["ExpenseType"] = expenseType;
                    claimedRow["Date"] = date;
                    claimedRow["FromTime"] = fromTime;
                    claimedRow["ToTime"] = toTime;
                    claimedRow["Particulars"] = particulars;
                    claimedRow["Distance"] = distance;
                    claimedRow["Remarks"] = remarks;
                    claimedRow["Amount"] = amount;
                    dtClaimed.Rows.Add(claimedRow);
                }
                else
                {
                    TextBox txtMiscellaneousAmount = (TextBox)row.FindControl("txtMiscellaneousAmount");
                    amount = txtMiscellaneousAmount != null ? txtMiscellaneousAmount.Text : string.Empty;
                    if (!string.IsNullOrEmpty(amount) && decimal.TryParse(amount, out decimal parsedAmount) && parsedAmount > 0)
                    {
                        DataRow nonClaimedRow = dtNonClaimed.NewRow();
                        nonClaimedRow["Source"] = "Miscellaneous";
                        nonClaimedRow["ExpenseType"] = expenseType;
                        nonClaimedRow["Date"] = date;
                        nonClaimedRow["FromTime"] = fromTime;
                        nonClaimedRow["ToTime"] = toTime;
                        nonClaimedRow["Particulars"] = particulars;
                        nonClaimedRow["Distance"] = distance;
                        nonClaimedRow["Remarks"] = remarks;
                        nonClaimedRow["Amount"] = amount;
                        dtNonClaimed.Rows.Add(nonClaimedRow);
                    }
                }

                // Process Lodging expenses
                if (chkLodgingClaimable != null && chkLodgingClaimable.Checked)
                {
                    TextBox txtLodgingAmount = (TextBox)row.FindControl("txtLodgingAmount");
                    amount = txtLodgingAmount != null ? txtLodgingAmount.Text : string.Empty;

                    DataRow claimedRow = dtClaimed.NewRow();
                    claimedRow["Source"] = "Lodging";  // Source is "Lodging"
                    claimedRow["ExpenseType"] = expenseType;
                    claimedRow["Date"] = date;
                    claimedRow["FromTime"] = fromTime;
                    claimedRow["ToTime"] = toTime;
                    claimedRow["Particulars"] = particulars;
                    claimedRow["Distance"] = distance;
                    claimedRow["Remarks"] = remarks;
                    claimedRow["Amount"] = amount;
                    dtClaimed.Rows.Add(claimedRow);
                }
                else
                {
                    TextBox txtLodgingAmount = (TextBox)row.FindControl("txtLodgingAmount");
                    amount = txtLodgingAmount != null ? txtLodgingAmount.Text : string.Empty;
                    if (!string.IsNullOrEmpty(amount) && decimal.TryParse(amount, out decimal parsedAmount) && parsedAmount > 0)
                    {
                        DataRow nonClaimedRow = dtNonClaimed.NewRow();
                        nonClaimedRow["Source"] = "Lodging";
                        nonClaimedRow["ExpenseType"] = expenseType;
                        nonClaimedRow["Date"] = date;
                        nonClaimedRow["FromTime"] = fromTime;
                        nonClaimedRow["ToTime"] = toTime;
                        nonClaimedRow["Particulars"] = particulars;
                        nonClaimedRow["Distance"] = distance;
                        nonClaimedRow["Remarks"] = remarks;
                        nonClaimedRow["Amount"] = amount;
                        dtNonClaimed.Rows.Add(nonClaimedRow);
                    }
                }
            }

            // Now create Excel package and export data
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                // Create a worksheet for "Claimed" data
                var claimedWorksheet = package.Workbook.Worksheets.Add("Claimed");
                claimedWorksheet.Cells["A1"].LoadFromDataTable(dtClaimed, true);

                // Create a worksheet for "Non-Claimed" data
                var nonClaimedWorksheet = package.Workbook.Worksheets.Add("Non-Claimed");
                nonClaimedWorksheet.Cells["A1"].LoadFromDataTable(dtNonClaimed, true);

                // Create a memory stream to hold the Excel file data
                using (var stream = new MemoryStream())
                {
                    // Save the package to the memory stream
                    package.SaveAs(stream);

                    // Send the file to the browser
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment; filename=Expense_Report.xlsx");
                    Response.BinaryWrite(stream.ToArray());
                    Response.End();
                }
            }
        }


        private void ExportToExcel(DataTable dt, string fileName)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                // Create a worksheet for claimed or non-claimed rows
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Expense Data");

                // Load the DataTable into the worksheet starting from cell A1
                worksheet.Cells["A1"].LoadFromDataTable(dt, true); // true to include headers

                // Automatically adjust the column widths based on content
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Set the response headers to trigger the file download
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment; filename={fileName}");

                // Write the Excel package to the response stream
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }



        private DataTable GetExpenseData()
        {
            DataTable dt = new DataTable();
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string qry = @"
      SELECT 
          'Conveyance' AS Source, 
          c.ExpenseType, 
          FORMAT(c.Date, 'dd/MMM/yyyy') AS Date,
          c.FromTime, 
          c.ToTime, 
          c.Particulars,  
          c.Distance,     
          c.Remarks,       
          c.Amount AS Amount
      FROM Conveyance c
      WHERE c.ServiceId = @ServiceId
      UNION ALL
      SELECT 
          'Food' AS Source,
          f.ExpenseType, 
          FORMAT(f.Date, 'dd/MMM/yyyy') AS Date,
          f.FromTime, 
          f.ToTime, 
          f.Particulars,  
          NULL AS Distance,     
          f.Remarks,       
          f.Amount AS Amount
      FROM Food f
      WHERE f.ServiceId = @ServiceId
      UNION ALL
      SELECT 
          'Others' AS Source,
          o.ExpenseType, 
          FORMAT(o.Date, 'dd/MMM/yyyy') AS Date,
          o.FromTime, 
          o.ToTime, 
          o.Particulars,  
          NULL AS Distance,     
          o.Remarks,       
          o.Amount AS Amount
      FROM Others o
      WHERE o.ServiceId = @ServiceId
      UNION ALL
      SELECT 
          'Miscellaneous' AS Source,
          m.ExpenseType, 
          FORMAT(m.Date, 'dd/MMM/yyyy') AS Date,
          m.FromTime, 
          m.ToTime, 
          m.Particulars,  
          NULL AS Distance,     
          m.Remarks,       
          m.Amount AS Amount
      FROM Miscellaneous m
      WHERE m.ServiceId = @ServiceId
      UNION ALL
      SELECT 
          'Lodging' AS Source,
          l.ExpenseType, 
          FORMAT(l.Date, 'dd/MMM/yyyy') AS Date,
          l.FromTime, 
          l.ToTime, 
          l.Particulars,  
          NULL AS Distance,     
          l.Remarks,       
          l.Amount AS Amount
      FROM Lodging l
      WHERE l.ServiceId = @ServiceId";

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    cmd.Parameters.AddWithValue("@ServiceId", Convert.ToInt32(Session[ServiceIdSessionKey]));
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }
                }
            }

            return dt;
        }
        protected void ExpenseGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRow")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = ExpenseGridView.Rows[rowIndex];
                ToggleTextBoxReadOnly(row);
            }
        }

        private void ToggleTextBoxReadOnly(GridViewRow row)
        {
            string[] textBoxIds = { "txtConveyanceAmount", "txtFoodAmount", "txtOthersAmount", "txtMiscellaneousAmount", "txtLodgingAmount" };
            foreach (var id in textBoxIds)
            {
                TextBox textBox = (TextBox)row.FindControl(id);
                if (textBox != null)
                {
                    textBox.ReadOnly = !textBox.ReadOnly; // Toggle ReadOnly state
                }
            }
        }

        protected void chkClaimable_CheckedChanged(object sender, EventArgs e)
        {
            GridViewRow row = (GridViewRow)((CheckBox)sender).NamingContainer;
            UpdateClaimableStatus(row);
            UpdateTotalAmounts();
        }

        private void UpdateClaimableStatus(GridViewRow row)
        {
            string category = row.Cells[1]?.Text; // Assuming the category is in the second cell
            if (string.IsNullOrEmpty(category)) return;

            CheckBox checkBox = (CheckBox)row.FindControl($"chk{category}Claimable");
            decimal amount = GetAmountFromTextBox(row, $"txt{category}Amount");

            if (checkBox != null) // Ensure the checkbox is found
            {
                // Determine the database table and update amount based on the category
                string tableName = "";

                switch (category)
                {
                    case "Conveyance":
                        tableName = "Conveyance";
                        break;
                    case "Food":
                        tableName = "Food";
                        break;
                    case "Others":
                        tableName = "Others";
                        break;
                    case "Miscellaneous":
                        tableName = "Miscellaneous";
                        break;
                    case "Lodging":
                        tableName = "Lodging";
                        break;
                    default:
                        return; // Unknown category, exit the method
                }

                if (checkBox.Checked)
                {
                    UpdateClaimedAmounts(tableName, amount, true);
                }
                else
                {
                    UpdateClaimedAmounts(tableName, amount, false);
                }
            }
        }

        private void UpdateClaimedAmounts(string tableName, decimal amount, bool isClaimed)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                string updateQuery = $@"
            UPDATE {tableName} 
            SET ClaimedAmount = ClaimedAmount + @ClaimedAmount 
            WHERE ServiceId = @ServiceId";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@ServiceId", Convert.ToInt32(Session[ServiceIdSessionKey]));

                    // If claimed, add the amount; otherwise, subtract it (if needed)
                    cmd.Parameters.AddWithValue("@ClaimedAmount", isClaimed ? amount : -amount);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void UpdateTotalAmounts()
        {
            decimal totalClaimed = 0;
            decimal totalNonClaimed = 0;

            foreach (GridViewRow row in ExpenseGridView.Rows)
            {
                totalClaimed += CalculateTotalForRow(row);
            }

            foreach (GridViewRow row in ExpenseGridView.Rows)
            {
                string[] categories = { "Conveyance", "Food", "Others", "Miscellaneous", "Lodging" };
                foreach (var category in categories)
                {
                    CheckBox checkBox = (CheckBox)row.FindControl($"chk{category}Claimable");
                    if (checkBox != null && !checkBox.Checked)
                    {
                        totalNonClaimed += GetAmountFromTextBox(row, $"txt{category}Amount");
                    }
                }
            }

            txtTotalClaimedAmount.Text = totalClaimed.ToString("0.00");
            txtTotalNonClaimedAmount.Text = totalNonClaimed.ToString("0.00");
        }

        private decimal CalculateTotalForRow(GridViewRow row)
        {
            decimal total = 0;
            string[] categories = { "Conveyance", "Food", "Others", "Miscellaneous", "Lodging" };

            foreach (var category in categories)
            {
                total += UpdateTotalBasedOnCheckbox(row, $"chk{category}Claimable", GetAmountFromTextBox(row, $"txt{category}Amount"));
            }

            return total;
        }

        private decimal UpdateTotalBasedOnCheckbox(GridViewRow row, string checkBoxId, decimal amount)
        {
            CheckBox checkBox = (CheckBox)row.FindControl(checkBoxId);
            return checkBox != null && checkBox.Checked ? amount : 0;
        }

        private decimal GetAmountFromTextBox(GridViewRow row, string textBoxId)
        {
            TextBox textBox = (TextBox)row.FindControl(textBoxId);
            decimal.TryParse(textBox?.Text, out decimal amount);
            return amount;
        }

        protected void btnEditRow_Click(object sender, EventArgs e)
        {
            Button btnEdit = (Button)sender;
            GridViewRow row = (GridViewRow)btnEdit.NamingContainer;
            ToggleTextBoxReadOnly(row);
            btnEdit.Text = btnEdit.Text == "Edit" ? "Save" : "Edit";
        }

        //protected void btnCalculateBalance_Click(object sender, EventArgs e)
        //{
        //    // Calling the method to calculate the balance
        //    CalculateBalance();
        //}

        //private void CalculateBalance()
        //{
        //    // Check if ServiceId exists in the session
        //    if (Session[ServiceIdSessionKey] is int serviceId && serviceId > 0)
        //    {
        //        int employeeId = GetCurrentEmployeeId(); // This method should return a valid employee ID

        //        // Debugging: Log session values to ensure they are correct
        //        Debug.WriteLine($"ServiceId: {serviceId}, EmployeeId: {employeeId}");

        //        // Database connection string from web.config
        //        string constr = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

        //        try
        //        {
        //            using (SqlConnection con = new SqlConnection(constr))
        //            {
        //                con.Open();
        //                string query = @"
        //            SELECT 
        //                e.Advance,
        //                e.Total 
        //            FROM 
        //                Expense e 
        //            WHERE 
        //                e.ServiceId = @ServiceId 
        //                AND e.EmployeeId = @EmployeeId";

        //                using (SqlCommand cmd = new SqlCommand(query, con))
        //                {
        //                    // Add parameters to prevent SQL injection
        //                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
        //                    cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

        //                    // Debugging: Log the query with parameters
        //                    Debug.WriteLine($"Executing Query: {query}, ServiceId: {serviceId}, EmployeeId: {employeeId}");

        //                    using (SqlDataReader reader = cmd.ExecuteReader())
        //                    {
        //                        if (reader.Read())
        //                        {
        //                            // Retrieve the advance and total values from the database
        //                            decimal advance = reader.GetDecimal(reader.GetOrdinal("Advance"));
        //                            decimal total = reader.GetDecimal(reader.GetOrdinal("Total"));
        //                            decimal balance = advance - total;  // Subtracting Advance from Total to get remaining balance

        //                            // Display remaining balance with two decimal places
        //                            lblBalance.Text = $"Remaining Balance: {balance:0.00}";
        //                            lblBalance.Visible = true;
        //                        }
        //                        else
        //                        {
        //                            // If no records found in the database, display the message
        //                            lblBalance.Text = "No records found for the given ServiceId and EmployeeId.";
        //                            lblBalance.Visible = true;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log and display any errors that occur during database connection or query execution
        //            lblBalance.Text = $"Error: {ex.Message}";
        //            lblBalance.Visible = true;
        //        }
        //    }
        //    else
        //    {
        //        // If ServiceId is not found in the session, display an error message
        //        lblBalance.Text = "Service ID not found in session.";
        //        lblBalance.Visible = true;
        //    }
        //}


        // Example method to retrieve the current EmployeeId (this should be implemented according to your logic)
        private int GetCurrentEmployeeId()
        {
            // Retrieve the current EmployeeId from session or other relevant source
            // For example:
            return Session["EmployeeId"] is int empId ? empId : 0;  // Default to 0 if not found
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    // Get total claimed and non-claimed amounts from the text boxes
                    decimal totalClaimedAmount = decimal.TryParse(txtTotalClaimedAmount.Text, out decimal claimedAmount) ? claimedAmount : 0;
                    decimal totalNonClaimedAmount = decimal.TryParse(txtTotalNonClaimedAmount.Text, out decimal nonClaimedAmount) ? nonClaimedAmount : 0;

                    // Update the Expense table with these totals
                    string updateQuery = @"
                UPDATE Expense 
                SET ClaimedAmount = @ClaimedAmount, 
                    NonClaimedAmount = @NonClaimedAmount
                WHERE ServiceId = @ServiceId";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ClaimedAmount", totalClaimedAmount);
                        cmd.Parameters.AddWithValue("@NonClaimedAmount", totalNonClaimedAmount);
                        cmd.Parameters.AddWithValue("@ServiceId", Convert.ToInt32(Session[ServiceIdSessionKey]));

                        cmd.ExecuteNonQuery();
                    }

                    // Now, store amounts in their respective tables
                    StoreClaimedAndNonClaimedAmounts(con);
                }

                lblMessage.Text = "Amounts updated successfully!";
                lblMessage.Visible = true;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "An error occurred while updating amounts: " + ex.Message;
                lblMessage.Visible = true;
            }
        }

        private void StoreClaimedAndNonClaimedAmounts(SqlConnection con)
        {
            string[] categories = { "Conveyance", "Food", "Others", "Miscellaneous", "Lodging" };

            foreach (var category in categories)
            {
                decimal claimedAmount = 0;
                decimal nonClaimedAmount = 0;

                foreach (GridViewRow row in ExpenseGridView.Rows)
                {
                    CheckBox checkBox = (CheckBox)row.FindControl($"chk{category}Claimable");
                    decimal amount = GetAmountFromTextBox(row, $"txt{category}Amount");

                    if (checkBox != null)
                    {
                        if (checkBox.Checked)
                        {
                            claimedAmount += amount; // Accumulate claimed amounts
                        }
                        else
                        {
                            nonClaimedAmount += amount; // Accumulate non-claimed amounts
                        }
                    }
                }

                // Update the claimable status based on whether claimed amounts exist
                UpdateClaimableStatusInDatabase(category, claimedAmount > 0, Convert.ToInt32(Session[ServiceIdSessionKey]), con);

                // Now update claimed and non-claimed amounts in the database
                UpdateCategoryTable(category, claimedAmount, nonClaimedAmount, Convert.ToInt32(Session[ServiceIdSessionKey]), con);
            }
        }


        private void UpdateClaimableStatusInDatabase(string category, bool isClaimable, int serviceId, SqlConnection con)
        {
            string updateQuery = $@"
        UPDATE {category} 
        SET IsClaimable = @IsClaimable 
        WHERE ServiceId = @ServiceId";

            using (SqlCommand cmd = new SqlCommand(updateQuery, con))
            {
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                cmd.Parameters.AddWithValue("@IsClaimable", isClaimable); // 1 for checked, 0 for unchecked

                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateCategoryTable(string category, decimal claimedAmount, decimal nonClaimedAmount, int serviceId, SqlConnection con)
        {
            string updateQuery = $@"
        UPDATE {category} 
        SET ClaimedAmount = @ClaimedAmount, 
            NonClaimedAmount = @NonClaimedAmount
        WHERE Id = @Id";

            using (SqlCommand cmd = new SqlCommand(updateQuery, con))
            {
                cmd.Parameters.AddWithValue("@ClaimedAmount", claimedAmount);
                cmd.Parameters.AddWithValue("@NonClaimedAmount", nonClaimedAmount);
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                cmd.ExecuteNonQuery();
            }
        }

        private decimal GetTotalForCategory(string category)
        {
            decimal total = 0;

            foreach (GridViewRow row in ExpenseGridView.Rows)
            {
                total += GetAmountFromTextBox(row, $"txt{category}Amount");
            }

            return total;
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            // Redirect to AdminPage
            Response.Redirect("AdminPage.aspx");
        }


        private void UpdateCategoryTable(string category, decimal amount, int serviceId, SqlConnection con)
        {
            string updateQuery = $@"
                UPDATE {category} 
                SET Amount = @Amount 
                WHERE Id = @Id";

            using (SqlCommand cmd = new SqlCommand(updateQuery, con))
            {
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}