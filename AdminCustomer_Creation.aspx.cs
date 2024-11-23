using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vivify
{
    public partial class AdminCustomer_Creation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                bindBranch();
                LoadView();

            }
        }

        private void bindBranch()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                string qry = "Select * from Branch";
                SqlCommand cmd1 = new SqlCommand(qry, con);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        ddlBranch.DataSource = dt;
                        ddlBranch.DataBind();
                        ddlBranch.DataTextField = "BranchName";
                        ddlBranch.DataValueField = "BranchId";
                        ddlBranch.DataBind();
                        //ddlBranch.da
                    }
                }
            }
        }

        protected void btnCustomerCreate_Click(object sender, EventArgs e)
        {
            save();
        }
        private void save()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                // Remove the CustomerCode check since we're not using CustomerCode anymore.
                string qry = "Insert into Customers(CustomerName, BranchId, Address1, Active) values(@CustomerName, @BranchId, @Address1, 1)";

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                    cmd.Parameters.AddWithValue("@BranchId", ddlBranch.SelectedValue);
                    cmd.Parameters.AddWithValue("@Address1", txtAddress.Text);

                    cmd.ExecuteNonQuery();
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "showalert", "alert('Customer Created Successfully.');", true);

                    // Clear the form fields after successful insert
                    txtCustomerName.Text = "";
                    txtAddress.Text = "";
                    ddlBranch.SelectedIndex = 0;
                }
            }
        }
        private void LoadView()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Customers";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Ensure that the GridView is defined on the ASPX page with ID 'gridView1'
                    dt.Columns.Add("Customerid");

                    dt.Columns.Add("Customername");
                    dt.Columns.Add("Address");
                    dt.Columns.Add("Branchid");


                    GridView1.DataSource = dt;
                    GridView1.DataBind();

                }
            }

        }
    }
}