using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace Vivify
{
    public partial class Refreshment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if the EmployeeId exists in session
                if (Session["EmployeeId"] != null)
                {
                    // Load employee name, etc. from session if available
                    txtEmployeeName.Text = Session["EmployeeFirstName"]?.ToString();
                }
                else
                {
                    lblValidationMessage.Text = "EmployeeId not found in session.";
                    lblValidationMessage.Visible = true;
                    // Optionally redirect to login page if the session is expired
                    Response.Redirect("Login.aspx");
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if EmployeeId is in session
                if (Session["EmployeeId"] == null)
                {
                    lblValidationMessage.Text = "EmployeeId not found in session.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Get the EmployeeId from session
                int employeeId = Convert.ToInt32(Session["EmployeeId"]);

                string fromDateStr = txtLocalRefreshmentFromDate.Text;
                string toDateStr = txtLocalRefreshmentToDate.Text;

                DateTime fromDate;
                DateTime toDate;

                // Validate the dates
                if (!DateTime.TryParse(fromDateStr, out fromDate) || !DateTime.TryParse(toDateStr, out toDate))
                {
                    lblValidationMessage.Text = "Please enter valid dates in the format MM/dd/yyyy.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Calculate the total days between FromDate and ToDate (including both start and end dates)
                int totalDays = (toDate - fromDate).Days + 1;  // Add 1 to include both start and end date

                // Validate the date range to ensure total days is 30 or 31
                if (totalDays != 30 && totalDays != 31)
                {
                    lblValidationMessage.Text = "The date range must be exactly 30 or 31 days.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Check if the employee has already been assigned a refreshment for the month
                if (IsRefreshmentAssignedForMonth(fromDate, employeeId)) // Pass employeeId here
                {
                    lblValidationMessage.Text = "A refreshment has already been assigned for this month.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Validate the refreshment amount
                decimal amount;
                if (!decimal.TryParse(txtLocalRefreshmentAmount.Text, out amount))
                {
                    lblValidationMessage.Text = "Please enter a valid amount.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Save the uploaded bill file
                byte[] imageBytes = SaveFile();
                if (imageBytes == null)
                {
                    lblValidationMessage.Text = "Only JPG, JPEG, and PNG files are allowed.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Get the ServiceId from the Refreshment table based on EmployeeId
                int serviceId = GetServiceIdFromEmployeeId(employeeId);
                if (serviceId == 0)
                {
                    lblValidationMessage.Text = "No ServiceId found for this employee.";
                    lblValidationMessage.Visible = true;
                    return;
                }

                // Insert the refreshment record into the database
                string connectionString = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Refreshment (Image, FromDate, ToDate, RefreshAmount, ServiceId, FirstName, Department, ServiceType, EmployeeId) " +
                                   "VALUES (@DataImage, @FromDate, @ToDate, @RefreshAmount, @ServiceId, @FirstName, @Department, @ServiceType, @EmployeeId)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DataImage", imageBytes);
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ToDate", toDate);
                        cmd.Parameters.AddWithValue("@RefreshAmount", amount);
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        cmd.Parameters.AddWithValue("@FirstName", Session["EmployeeFirstName"]?.ToString());
                        cmd.Parameters.AddWithValue("@Department", "Refresh");
                        cmd.Parameters.AddWithValue("@ServiceType", "Refresh");
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId); // Insert EmployeeId here

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblSuccessMessage.Text = "Refreshment assigned successfully!";
                lblSuccessMessage.Visible = true;
            }
            catch (Exception ex)
            {
                lblValidationMessage.Text = $"Error: {ex.Message}";
                lblValidationMessage.Visible = true;
            }
        }


        // Method to fetch ServiceId from the Service table based on EmployeeId
        private int GetServiceIdFromEmployeeId(int employeeId)
        {
            int serviceId = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Query to get the ServiceId from the Services table based on EmployeeId
                string query = "SELECT ServiceId FROM Services WHERE EmployeeId = @EmployeeId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        serviceId = Convert.ToInt32(result);
                    }
                    else
                    {
                        // Log if no ServiceId is found
                        System.Diagnostics.Debug.WriteLine($"No ServiceId found for EmployeeId: {employeeId}");
                    }
                }
            }

            return serviceId;
        }

        // Method to check if refreshment is already assigned for the given month and employeeId
        private bool IsRefreshmentAssignedForMonth(DateTime date, int employeeId)
        {
            int serviceId = GetServiceIdFromEmployeeId(employeeId);
            if (serviceId == 0)
            {
                System.Diagnostics.Debug.WriteLine($"No service found for EmployeeId: {employeeId}");
                return false;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            string query = "SELECT COUNT(*) FROM Refreshment WHERE ServiceId = @ServiceId AND EmployeeId = @EmployeeId " +
                           "AND MONTH(FromDate) = @Month AND YEAR(FromDate) = @Year";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    cmd.Parameters.AddWithValue("@Month", date.Month);
                    cmd.Parameters.AddWithValue("@Year", date.Year);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();

                    return count > 0; // Return true if refreshment exists for the given month and employee
                }
            }
        }

        // Method to save the uploaded image file to byte array
        private byte[] SaveFile()
        {
            if (fileUploadRefBill.HasFile)
            {
                // Check file type (e.g., only allow images)
                string fileExtension = Path.GetExtension(fileUploadRefBill.FileName).ToLower();
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    lblValidationMessage.Text = "Only JPG, JPEG, and PNG files are allowed.";
                    lblValidationMessage.Visible = true;
                    return null;
                }

                using (var memoryStream = new MemoryStream())
                {
                    fileUploadRefBill.PostedFile.InputStream.CopyTo(memoryStream);
                    return memoryStream.ToArray(); // Convert the file to byte array
                }
            }
            return null; // Return null if no file is uploaded
        }
    }
}
