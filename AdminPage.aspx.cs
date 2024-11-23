using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vivify
{
    public partial class AdminPage : System.Web.UI.Page
    {
        // On Page Load, bind the GridView and the Branch dropdown list
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                BindBranchDropdown(); // Bind the branch dropdown on first page load
            }
        }

        // Method to bind the branch dropdown
        private void BindBranchDropdown()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string qry = "SELECT DISTINCT BranchName FROM Employees"; // Assuming 'Employees' table has 'BranchName' column
                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlBranch.DataSource = reader;
                    ddlBranch.DataTextField = "BranchName";
                    ddlBranch.DataValueField = "BranchName"; // Use BranchName as both value and text
                    ddlBranch.DataBind();
                }
            }
            ddlBranch.Items.Insert(0, new ListItem("Select Branch", "0")); // Insert default option at the top
        }


        // Method to bind the GridView based on branch selection
        private void BindGridView()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string qry = @"
        SELECT 
            c.CustomerName, 
            s.ServiceId, 
            e.FirstName, 
            exp.Total, 
            s.Advance,
            FORMAT(s.FromDate, 'dd-MMM-yyyy') AS FormattedFromDate
        FROM 
            Customers c
        INNER JOIN 
            Services s ON c.CustomerId = s.CustomerId
        LEFT JOIN 
            Employees e ON s.EmployeeId = e.EmployeeId
        LEFT JOIN 
            Expense exp ON s.ServiceId = exp.ServiceId";

                // Apply branch filter if a branch is selected
                string branchFilter = ddlBranch.SelectedValue;
                if (branchFilter != "0")
                {
                    qry += " WHERE e.BranchName = @BranchName";
                }

                // Order by FromDate in descending order
                qry += " ORDER BY s.FromDate DESC"; // Sorting by FromDate in descending order

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    if (branchFilter != "0")
                    {
                        cmd.Parameters.AddWithValue("@BranchName", branchFilter); // Add the branch filter parameter
                    }

                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        GridView1.DataSource = dt;
                        GridView1.DataBind();
                    }
                }
            }

            // Show the GridView only after the data is bound
            GridView1.Visible = GridView1.Rows.Count > 0; // Show GridView only if there is data
        }


        protected void btnFilter_Click(object sender, EventArgs e)
        {
            // Check if a branch is selected
            if (ddlBranch.SelectedValue != "0")
            {
                // Bind the GridView with the selected branch filter
                BindGridView();
            }
            else
            {
                // Optionally, display a message if no branch is selected
                GridView1.Visible = false; // Hide the GridView
                lblError.Text = "Please select a branch to filter.";
            }
        }
        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Verify")
            {
                // Get the ServiceId from the CommandArgument (the value passed in the CommandArgument)
                int serviceId = Convert.ToInt32(e.CommandArgument);

                Session["ServiceId"] = serviceId;
                FetchExpenseTotals(serviceId);
                FetchFromDate(serviceId);
                Response.Redirect("AdminVerify.aspx");
                // Your verification logic here, for example:
                // Verify the service and update its status or perform any other actions

                // You can display a success message or reload the GridView after the operation
                // Optionally, rebind the GridView to refresh the data
                BindGridView();
            }
        }

        private void FetchFromDate(int serviceId)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                // SQL query to fetch FromDate for the selected ServiceId
                string qry = "SELECT FORMAT(FromDate, 'dd-MMM-yyyy') FROM Services WHERE ServiceId = @ServiceId";
                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        // Assuming you have a label to display FromDate in the verification form
                        // lblFromDate.Text = result.ToString(); // Example label to show FromDate
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // Handle submission logic to save the updated expense details
            // Example:
            // Validate input, update database, and provide feedback to the user
        }

        private void FetchExpenseTotals(int serviceId)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                // SQL query to fetch expense totals for the selected ServiceId
                string qry = @"
        SELECT 
            ConveyanceTotal,
            FoodTotal,
            MiscellaneousTotal,
            OthersTotal,
            LodgingTotal
        FROM 
            Expense WHERE ServiceId = @ServiceId";

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        // Assuming these are your TextBox IDs in the ASPX
                        txtConveyanceTotal.Text = reader["ConveyanceTotal"].ToString();
                        txtFoodTotal.Text = reader["FoodTotal"].ToString();
                        txtMiscellaneousTotal.Text = reader["MiscellaneousTotal"].ToString();
                        txtOthersTotal.Text = reader["OthersTotal"].ToString();
                        txtLodgingTotal.Text = reader["LodgingTotal"].ToString();
                    }
                }
            }
        }


        // Button click event to filter the GridView based on selected branch


        // Your existing GridView RowCommand event handler and other methods here
    }
}