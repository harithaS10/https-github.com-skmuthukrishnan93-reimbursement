using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;

namespace Vivify
{
    public partial class DocView : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBranches();
            }
        }

        private void LoadBranches()
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("SELECT BranchId, BranchName FROM Branch", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlBranch.DataSource = reader;
                ddlBranch.DataTextField = "BranchName";
                ddlBranch.DataValueField = "BranchId";
                ddlBranch.DataBind();
            }

            // Add default "empty" item with an empty space as Text
            // Empty string as the text

            // Set default selection to the empty space
            if (ddlBranch.Items.Count > 0)
            {
                ddlBranch.SelectedIndex = 0;  // Default to the empty space
            }
        }



        protected void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime fromDate = DateTime.Parse(txtFromDate.Text);
            DateTime toDate = DateTime.Parse(txtToDate.Text);
            int branchId = int.Parse(ddlBranch.SelectedValue);
            string branchName = ddlBranch.SelectedItem.Text;

            // Get all service data based on the date range and branch
            var services = GetServiceData(fromDate, toDate, branchId);
            if (services.Count == 0)
            {
                // Handle the case where no service data is found (e.g., show an error message)
                return;
            }

            // Fetch the expense data based on the date range and branch
            DataTable expenseData = GetExpenseData(fromDate, toDate, branchName);

            // Get WorkOrder, ApprovalMail, and Refreshment images data
            var workOrderData = GetWorkOrderImages(fromDate, toDate, branchName);
            var approvalMailAndRefreshmentData = GetApprovalMailAndRefreshmentImages(fromDate, toDate, branchName); // Updated method call

            // Generate the PDF
            GeneratePDF(services, expenseData, workOrderData, approvalMailAndRefreshmentData, fromDate, toDate);
        }

        private List<(int ServiceId, string EmployeeName, string BranchName, DateTime FromDate, DateTime ToDate)> GetServiceData(DateTime fromDate, DateTime toDate, int branchId)
        {
            List<(int ServiceId, string EmployeeName, string BranchName, DateTime FromDate, DateTime ToDate)> serviceList = new List<(int, string, string, DateTime, DateTime)>();
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT s.ServiceId, e.FirstName, b.BranchName, s.FromDate, s.ToDate 
                    FROM Services s
                    JOIN Employees e ON s.EmployeeId = e.EmployeeId
                    JOIN Branch b ON s.BranchId = b.BranchId
                    WHERE s.FromDate >= @FromDate AND s.ToDate <= @ToDate AND s.BranchId = @BranchId", conn);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@BranchId", branchId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        serviceList.Add((
                            ServiceId: reader.GetInt32(0),
                            EmployeeName: reader.GetString(1),
                            BranchName: reader.GetString(2),
                            FromDate: reader.GetDateTime(3),
                            ToDate: reader.GetDateTime(4)
                        ));
                    }
                }
            }

            return serviceList;
        }

        private DataTable GetExpenseData(DateTime fromDate, DateTime toDate, string branchName)
        {
            DataTable expenseTable = new DataTable();
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(constr))
            {
                // Query to fetch ServiceIds for the given date range and branch name
                string serviceQuery = @"
            SELECT s.ServiceId
            FROM Services s
            JOIN Employees e ON s.EmployeeId = e.EmployeeId
            WHERE s.FromDate >= @FromDate 
            AND s.ToDate <= @ToDate 
            AND e.BranchName = @BranchName";

                SqlCommand serviceCmd = new SqlCommand(serviceQuery, conn);
                serviceCmd.Parameters.AddWithValue("@FromDate", fromDate);
                serviceCmd.Parameters.AddWithValue("@ToDate", toDate);
                serviceCmd.Parameters.AddWithValue("@BranchName", branchName);

                conn.Open();
                SqlDataReader serviceReader = serviceCmd.ExecuteReader();

                List<int> serviceIds = new List<int>();

                while (serviceReader.Read())
                {
                    serviceIds.Add(serviceReader.GetInt32(0)); // ServiceId is in the first column
                }

                serviceReader.Close();

                // If no ServiceIds found, return empty expense table
                if (serviceIds.Count == 0)
                {
                    return expenseTable;
                }

                // Create the SQL query to fetch expenses for the ServiceIds
                string query = @"
            SELECT ExpenseType, ClaimedAmount, Image, ServiceId, IsClaimable 
            FROM (
                SELECT 'Conveyance' AS ExpenseType, ExpenseType AS Type, ClaimedAmount, Image, ServiceId, IsClaimable 
                FROM Conveyance 
                WHERE ServiceId IN ({0})
                UNION ALL
                SELECT 'Others' AS ExpenseType, ExpenseType AS Type, ClaimedAmount, Image, ServiceId, IsClaimable 
                FROM Others 
                WHERE ServiceId IN ({0})
                UNION ALL
                SELECT 'Lodging' AS ExpenseType, ExpenseType AS Type, ClaimedAmount, Image, ServiceId, IsClaimable 
                FROM Lodging 
                WHERE ServiceId IN ({0})
                UNION ALL
                SELECT 'Miscellaneous' AS ExpenseType, ExpenseType AS Type, ClaimedAmount, Image, ServiceId, IsClaimable 
                FROM Miscellaneous 
                WHERE ServiceId IN ({0})
            ) AS AllExpenses
            WHERE IsClaimable = 1";

                // Dynamically build the IN clause with parameters
                var parameters = new List<string>();
                for (int i = 0; i < serviceIds.Count; i++)
                {
                    parameters.Add("@ServiceId" + i);  // Create parameter names
                }

                string formattedQuery = string.Format(query, string.Join(",", parameters));

                SqlCommand cmd = new SqlCommand(formattedQuery, conn);

                // Add parameters for each ServiceId
                for (int i = 0; i < serviceIds.Count; i++)
                {
                    cmd.Parameters.AddWithValue("@ServiceId" + i, serviceIds[i]);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(expenseTable);
            }

            return expenseTable;
        }

        private DataTable GetWorkOrderImages(DateTime fromDate, DateTime toDate, string branchName)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
                    SELECT o.ServiceReport, o.ServiceId
                    FROM Others o
                    INNER JOIN Services s ON o.ServiceId = s.ServiceId
                    INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
                    WHERE s.FromDate >= @FromDate AND s.ToDate <= @ToDate
                    AND e.BranchName = @BranchName
                    AND o.IsClaimable = 1
                    UNION ALL
                    SELECT l.ServiceReport, l.ServiceId
                    FROM Lodging l
                    INNER JOIN Services s ON l.ServiceId = s.ServiceId
                    INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
                    WHERE s.FromDate >= @FromDate AND s.ToDate <= @ToDate
                    AND e.BranchName = @BranchName
                    AND l.IsClaimable = 1";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@BranchName", branchName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        private DataTable GetApprovalMailAndRefreshmentImages(DateTime fromDate, DateTime toDate, string branchName)
        {
            string constr = ConfigurationManager.ConnectionStrings["vivify"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
            -- Fetch ApprovalMail images from Others and Lodging tables
            SELECT o.ApprovalMail AS ImageData, o.ServiceId, 'ApprovalMail' AS ImageType
            FROM Others o
            INNER JOIN Services s ON o.ServiceId = s.ServiceId
            INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
            WHERE s.FromDate >= @FromDate AND s.ToDate <= @ToDate
            AND e.BranchName = @BranchName
            AND o.IsClaimable = 1

            UNION ALL

            SELECT l.ApprovalMail AS ImageData, l.ServiceId, 'ApprovalMail' AS ImageType
            FROM Lodging l
            INNER JOIN Services s ON l.ServiceId = s.ServiceId
            INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
            WHERE s.FromDate >= @FromDate AND s.ToDate <= @ToDate
            AND e.BranchName = @BranchName
            AND l.IsClaimable = 1

            -- Fetch Refreshment images from the Refreshment table
            UNION ALL

            SELECT r.Image AS ImageData, r.ServiceId, 'Refreshment' AS ImageType
            FROM Refreshment r
            INNER JOIN Services s ON r.ServiceId = s.ServiceId
            INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
            WHERE s.FromDate >= @FromDate AND s.ToDate <= @ToDate
            AND e.BranchName = @BranchName";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                cmd.Parameters.AddWithValue("@BranchName", branchName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        private void GeneratePDF(
    List<(int ServiceId, string EmployeeName, string BranchName, DateTime FromDate, DateTime ToDate)> services,
    DataTable expenseData,
    DataTable workOrderData,
    DataTable approvalMailAndRefreshmentData,
    DateTime fromDate,
    DateTime toDate)
        {
            Document document = new Document();
            MemoryStream ms = new MemoryStream();
            PdfWriter.GetInstance(document, ms);
            document.Open();

            int a = 0; // Refreshment and Approval image counter
            int b = 0; // Expense image counter
            int w = 0; // Work Order image counter

            // Loop through all services
            foreach (var serviceData in services)
            {
                // Create a Font object for text
                Font font = FontFactory.GetFont("Helvetica", 12, Font.NORMAL);

                // Add images first (Refreshment and Approval, Expense, Work Order)
              

                // After adding images, add service info text
                string serviceInfo = $"Employee: {serviceData.EmployeeName}\nBranch: {serviceData.BranchName}\nFrom: {serviceData.FromDate:yyyy-MM-dd}\nTo: {serviceData.ToDate:yyyy-MM-dd}";
                document.Add(new Paragraph(serviceInfo, font));
                AddRefreshmentAndApprovalImages(approvalMailAndRefreshmentData, serviceData.ServiceId, document, ref a);
                AddExpenseImages(expenseData, serviceData.ServiceId, document, ref b);
                AddWorkOrderImages(workOrderData, serviceData.ServiceId, document, ref w);
                // Add the text using the Font object
               


                // Add a new page after every service (to ensure one image per page)
                document.NewPage();
            }

            // Close the document and generate the PDF
            document.Close();

            // Set the response type to PDF and trigger download
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=Attachments.pdf");
            Response.BinaryWrite(ms.ToArray());
            Response.End();
        }
        private void AddRefreshmentAndApprovalImages(DataTable imageData, int serviceId, Document document, ref int a)
        {
            Font font = FontFactory.GetFont("Helvetica", 12, Font.NORMAL);

            // Create a highlighted version of the font for the "A" count
            Font highlightedFont = FontFactory.GetFont("Helvetica", 16, Font.BOLD, BaseColor.RED);

            foreach (DataRow row in imageData.Rows)
            {
                int rowServiceId = row["ServiceId"] != DBNull.Value ? Convert.ToInt32(row["ServiceId"]) : 0;

                if (rowServiceId == serviceId)
                {
                    byte[] imageBytes = row["ImageData"] as byte[];

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        string imageType = row["ImageType"].ToString();

                        try
                        {
                            // Handle Refreshment images first
                            if (imageType == "Refreshment")
                            {
                                a++;  // Increment refreshment image counter
                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageBytes);

                                // Set maximum width and height for the image
                                float maxWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                                float maxHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;

                                // Scale the image proportionally to fit within maxWidth and maxHeight
                                img.ScaleToFit(maxWidth, maxHeight);

                                img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                                // Add the image first
                                document.Add(img);

                                // Check if there's enough space on the current page to add the count
                                if (document.BottomMargin + img.ScaledHeight < document.PageSize.Height)
                                {
                                    document.Add(new Paragraph($"A{a}", highlightedFont)); // Highlighted count
                                }
                                else
                                {
                                    // If not enough space, add a new page and then add the count
                                    document.NewPage();
                                    document.Add(new Paragraph($"A{a}", highlightedFont)); // Highlighted count
                                }
                            }
                            // Then handle Approval Mail images
                            else if (imageType == "ApprovalMail")
                            {
                                a++;  // Increment approval mail image counter
                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageBytes);

                                // Set maximum width and height for the image
                                float maxWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                                float maxHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;

                                // Scale the image proportionally to fit within maxWidth and maxHeight
                                img.ScaleToFit(maxWidth, maxHeight);

                                img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                                // Add the image first
                                document.Add(img);

                                // Check if there's enough space on the current page to add the count
                                if (document.BottomMargin + img.ScaledHeight < document.PageSize.Height)
                                {
                                    document.Add(new Paragraph($"A{a}", highlightedFont)); // Highlighted count
                                }
                                else
                                {
                                    // If not enough space, add a new page and then add the count
                                    document.NewPage();
                                    document.Add(new Paragraph($"A{a}", highlightedFont)); // Highlighted count
                                }
                            }
                        }
                        catch
                        {
                            // Skipping error handling for image loading
                            // Optionally log the error if required
                        }
                    }
                }
            }
        }

        private void AddExpenseImages(DataTable expenseData, int serviceId, Document document, ref int b)
        {
            if (!expenseData.Columns.Contains("ServiceId"))
            {
                return;
            }

            // Create a highlighted version of the font for the "B" count
            Font highlightedFont = FontFactory.GetFont("Helvetica", 16, Font.BOLD, BaseColor.BLUE);  // Blue color

            foreach (DataRow row in expenseData.Rows)
            {
                int rowServiceId = row["ServiceId"] != DBNull.Value ? Convert.ToInt32(row["ServiceId"]) : 0;

                if (rowServiceId == serviceId)
                {
                    byte[] imageBytes = row["Image"] as byte[];

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        try
                        {
                            b++;  // Increment expense image counter

                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageBytes);

                            // Set maximum width and height for the image
                            float maxWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                            float maxHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;

                            // Scale the image proportionally to fit within maxWidth and maxHeight
                            img.ScaleToFit(maxWidth, maxHeight);

                            img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                            // Add the image first
                            document.Add(img);

                            // Check if there's enough space on the current page to add the count
                            if (document.BottomMargin + img.ScaledHeight < document.PageSize.Height)
                            {
                                document.Add(new Paragraph($"B{b}", highlightedFont)); // Highlighted count
                            }
                            else
                            {
                                // If not enough space, add a new page and then add the count
                                document.NewPage();
                                document.Add(new Paragraph($"B{b}", highlightedFont)); // Highlighted count
                            }
                        }
                        catch
                        {
                            // Skipping error handling for expense image loading
                        }
                    }
                }
            }
        }


        private void AddWorkOrderImages(DataTable workOrderData, int serviceId, Document document, ref int w)
        {
            if (!workOrderData.Columns.Contains("ServiceId"))
            {
                return;
            }

            // Create a highlighted version of the font for the "W" count
            Font highlightedFont = FontFactory.GetFont("Helvetica", 16, Font.BOLD, BaseColor.DARK_GRAY);  // Green color

            foreach (DataRow row in workOrderData.Rows)
            {
                int rowServiceId = row["ServiceId"] != DBNull.Value ? Convert.ToInt32(row["ServiceId"]) : 0;

                if (rowServiceId == serviceId)
                {
                    byte[] imageBytes = row["ServiceReport"] as byte[];

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        try
                        {
                            w++;  // Increment work order image counter

                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageBytes);

                            // Set maximum width and height for the image
                            float maxWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                            float maxHeight = document.PageSize.Height - document.TopMargin - document.BottomMargin;

                            // Scale the image proportionally to fit within maxWidth and maxHeight
                            img.ScaleToFit(maxWidth, maxHeight);

                            img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                            // Add the image first
                            document.Add(img);

                            // Check if there's enough space on the current page to add the count
                            if (document.BottomMargin + img.ScaledHeight < document.PageSize.Height)
                            {
                                document.Add(new Paragraph($"W{w}", highlightedFont)); // Highlighted count
                            }
                            else
                            {
                                // If not enough space, add a new page and then add the count
                                document.NewPage();
                                document.Add(new Paragraph($"W{w}", highlightedFont)); // Highlighted count
                            }
                        }
                        catch
                        {
                            // Skipping error handling for work order image loading
                        }
                    }
                }
            }
        }



    }
}
