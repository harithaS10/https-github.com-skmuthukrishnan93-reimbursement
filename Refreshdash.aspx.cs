using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vivify
{
    public partial class Refreshdash : Page
    {
        // Event handler for Page Load
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Bind the GridView data when the page loads
                BindGridView();
            }
        }

        // Method to bind data to GridView
        private void BindGridView()
        {
            // Get the connection string from the web.config
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            // Establish a connection to the database
            using (SqlConnection con = new SqlConnection(constr))
            {
                // Query to fetch EmployeeId and FirstName from the Employees table,
                // excluding those with the "Admin" role or where the role is NULL
                string qry = @"
                    SELECT 
                        e.EmployeeId,
                        e.FirstName,
                        e.Roles
                    FROM 
                        Employees e
                    WHERE 
                        e.Roles != 'Admin' OR e.Roles IS NULL;";  // Exclude employees with the role 'Admin' or where role is NULL

                // Create a SqlCommand to execute the query
                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    // Use SqlDataAdapter to fill the data into a DataTable
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        // DataTable to hold the fetched data
                        DataTable dt = new DataTable();
                        da.Fill(dt);  // Fill the DataTable with the data

                        // Bind the DataTable to the GridView
                        GridView1.DataSource = dt;
                        GridView1.DataBind();
                    }
                }
            }
        }

        // Event handler for GridView command button click
        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Reimburse")
            {
                // Debugging: Check if the event is firing
                System.Diagnostics.Debug.WriteLine("GridView RowCommand fired");

                // Get the row index from the CommandArgument
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[rowIndex];

                // Get the EmployeeId and FirstName labels from the GridView row
                Label lblEmployeeId = (Label)row.FindControl("lblEmployeeId");
                Label lblFirstName = (Label)row.FindControl("lblFirstName");

                // Ensure that EmployeeId and FirstName are not null
                if (lblEmployeeId != null && lblFirstName != null)
                {
                    // Get the EmployeeId from the GridView
                    int employeeId = Convert.ToInt32(lblEmployeeId.Text);
                    string firstName = lblFirstName.Text;

                    // Get the ServiceId from the session or another data source (e.g., from the GridView if available)
                    // You might need to capture the ServiceId, or retrieve it from the session or database
                    int serviceId = 1;  // Assuming you know the ServiceId value, otherwise retrieve it dynamically

                    // Insert the data into the Refreshment table
                    string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        // SQL query to insert the data into the Refreshment table
                        string insertQuery = @"
                            INSERT INTO Refreshment (EmployeeId, ServiceId)
                            VALUES (@EmployeeId, @ServiceId);";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                        {
                            // Add parameters to prevent SQL injection
                            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                            cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                            // Open the connection and execute the query
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }

                    // Optionally, store the EmployeeId and EmployeeFirstName in session for use in the next page
                    Session["EmployeeId"] = employeeId;
                    Session["EmployeeFirstName"] = firstName;

                    // Debugging: Check if the session values are set correctly
                    System.Diagnostics.Debug.WriteLine("EmployeeId in session: " + Session["EmployeeId"]);
                    System.Diagnostics.Debug.WriteLine("EmployeeFirstName in session: " + Session["EmployeeFirstName"]);

                    // Redirect to Refreshment.aspx (if needed)
                    Response.Redirect("Refreshment.aspx");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("EmployeeId or FirstName is null");
                }
            }
        }
    }
}
