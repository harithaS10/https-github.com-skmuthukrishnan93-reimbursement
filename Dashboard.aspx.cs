using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vivify
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGridView();
            }
        }

        private void InsertServiceIdIntoChildTables(int serviceId)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                string[] tables = { "Food", "Others", "Lodging", "Miscellaneous", "Conveyance" };

                foreach (var table in tables)
                {
                    InsertIntoTable(table, serviceId, con);
                }
            }
        }

        private void InsertIntoTable(string tableName, int serviceId, SqlConnection con)
        {
            string insertQry = $"INSERT INTO {tableName} (ServiceId) VALUES (@ServiceId)";
            using (SqlCommand cmd = new SqlCommand(insertQry, con))
            {
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Log or display error details
                    System.Diagnostics.Debug.WriteLine($"Error inserting into {tableName}: {ex.Message}");
                }
            }
        }


        private void BindGridView()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                // Updated SQL query to include ORDER BY for ServiceId in descending order
                string qry = @"
            SELECT 
                c.CustomerName, 
                s.Advance, 
            s.FromDate,
                s.ServiceType, 
                s.Status,
                s.StatusId,
                s.ServiceId
            FROM 
                Customers c
            INNER JOIN 
                Services s
            ON 
                c.CustomerId = s.CustomerId
            ORDER BY 
                s.ServiceId DESC"; // Sorting by ServiceId in descending order

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        GridView1.DataSource = dt;
                        GridView1.DataBind();
                    }
                }
            }
        }


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Reimburse")
            {
                // Get the row index
                int rowIndex = Convert.ToInt32(e.CommandArgument);

                // Get the row from the GridView
                GridViewRow row = GridView1.Rows[rowIndex];

                // Retrieve the hidden field containing the ServiceId
                HiddenField hdnServiceId = (HiddenField)row.FindControl("hdnServiceId");
                if (hdnServiceId != null && int.TryParse(hdnServiceId.Value, out int serviceId))
                {
                    // Store ServiceId in the session
                    Session["ServiceId"] = serviceId;

                    // Insert ServiceId into child tables
                    //InsertServiceIdIntoChildTables(serviceId);
                }
                else
                {
                    // Log or display a message indicating that the ServiceId is not a valid integer
                }
                
                Response.Redirect("Expenses.aspx");
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Example code to demonstrate handling the selected index change
            // This code could be used to display details of the selected row or perform some action

            if (GridView1.SelectedRow != null)
            {
                GridViewRow row = GridView1.SelectedRow;
                string customerName = row.Cells[0].Text; // Adjust index as needed
                string serviceId = row.Cells[2].Text; // Adjust index as needed

                // Example action: Display the selected data in a label or log it
                // lblSelectedData.Text = $"Selected Customer: {customerName}, Service Type: {serviceType}";

                // Or perform other actions based on the selected data
            }
        }
    }
}
