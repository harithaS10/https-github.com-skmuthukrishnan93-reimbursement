using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vivify
{
    public partial class Main : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Fetch user information from cookies
                HttpCookie firstNameCookie = Request.Cookies["FirstName"];
                HttpCookie employeeIdCookie = Request.Cookies["EmployeeId"];

                // Ensure the controls are correctly defined in Main.Master
                if (firstNameCookie != null)
                {
                    lblProfileName.Text = firstNameCookie.Value;
                }
                else
                {
                    lblProfileName.Text = "Guest";
                }

                if (employeeIdCookie != null)
                {
                    // Fetch the role from the database
                    string role = GetUserRole(employeeIdCookie.Value);
                    lblProfileRole.Text = role ?? "Role not available";
                }
                else
                {
                    lblProfileRole.Text = "Role not available";
                }

                // Redirect to the login page if not logged in
                if (GetLoginType() == null)
                {
                    Response.Redirect("AdminPage.aspx");
                }
            }
        }

        private string GetLoginType()
        {
            HttpCookie employeeIdCookie = Request.Cookies["EmployeeId"];
            HttpCookie firstNameCookie = Request.Cookies["FirstName"];

            // Check if the cookies are available
            if (employeeIdCookie == null || firstNameCookie == null)
            {
                return null;
            }

            // Set hidden fields with cookie values
            hdnLoginId.Value = employeeIdCookie.Value;
            hdnUserName.Value = firstNameCookie.Value;

            return string.Empty;
        }

        private string GetUserRole(string employeeId)
        {
            string role = null;
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                string query = "SELECT Roles FROM Employees WHERE EmployeeId = @EmployeeId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        role = result.ToString();
                    }
                }
            }

            return role;
        }
    }
}
