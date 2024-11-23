using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace Vivify
{
    public partial class Employeecreation : System.Web.UI.Page
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
                string qry = "SELECT * FROM Branch";
                SqlCommand cmd1 = new SqlCommand(qry, con);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        ddlBranch.DataSource = dt;
                        ddlBranch.DataTextField = "BranchName";
                        ddlBranch.DataValueField = "BranchId";
                        ddlBranch.DataBind();
                    }
                }
            }
        }

        protected void btn1_Click(object sender, EventArgs e)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(constr))
            {
                connection.Open();

                // Check if the employee code already exists
                string checkQuery = "SELECT COUNT(*) FROM Employees WHERE EmployeeCode = @EmployeeCode";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@EmployeeCode", txtcode.Text);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Employee code already exists
                        ScriptManager.RegisterStartupScript(this, GetType(), "Alert", "alert('Employee code already exists.');", true);
                        return;
                    }
                }

                // Employee code does not exist, proceed with insertion
                string insertQuery = @"
                    INSERT INTO Employees (Active, EmployeeCode, FirstName, LastName, MobileNumber, OfficialMail, Designation, BranchId,BranchName)
                    VALUES (1, @EmployeeCode, @FirstName, @LastName, @MobileNumber, @OfficialMail, @Designation, @BranchId,@Bname)";

                using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                {
                    // Add parameters to the command
                    insertCmd.Parameters.AddWithValue("@EmployeeCode", txtcode.Text);
                    insertCmd.Parameters.AddWithValue("@FirstName", txtName.Text);
                    insertCmd.Parameters.AddWithValue("@LastName", txtLname.Text);
                    insertCmd.Parameters.AddWithValue("@MobileNumber", txtMobno.Text);
                    insertCmd.Parameters.AddWithValue("@OfficialMail", txtOfcemail.Text);
                    insertCmd.Parameters.AddWithValue("@Designation", ddldesignation.SelectedItem.Text);
                    insertCmd.Parameters.AddWithValue("@BranchId", ddlBranch.SelectedValue);
                    insertCmd.Parameters.AddWithValue("@Bname", ddlBranch.SelectedItem.Text);
                    // Execute the insert query
                    insertCmd.ExecuteNonQuery();

                    // Notify user of success
                    ScriptManager.RegisterStartupScript(this, GetType(), "Alert", "alert('Employee created successfully.');", true);
                    connection.Close();
                    formClear();
                }
            }
        }

        void formClear()
        {
            txtcode.Text = "";
            txtName.Text = "";
            txtLname.Text = "";
            txtMobno.Text = "";
            txtOfcemail.Text = "";
            ddldesignation.SelectedIndex = 0; // Reset to default
        }

        private void LoadView()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Employees";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Set column names if necessary
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
        }
    }
}