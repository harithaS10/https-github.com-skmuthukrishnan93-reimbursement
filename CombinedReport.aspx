<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="CombinedReport.aspx.cs" Inherits="Vivify.CombinedReport" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Web.UI" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style>
        .mydatagrid th, .mydatagrid td {
            border: 1px solid black;
            padding: 8px;
            margin: 0;
            width:200px;
            align-items:center;
        }
        .mydatagrid td{
            background-color:white;
        }
        .mydatagrid th {
            background-color: #3f418d;
            color: white;
            position: sticky;
            top: 0;
            z-index: 10;
        }
        .scrollable-container {
            max-height: 400px;
            overflow-x: auto;
            overflow-y: auto;
            border: 1px solid #ccc;
            margin-bottom: 10px;
            width:400px;
        }
        .sidebar {
            width: 100%;
            max-width: 300px;
            float: left;
            background: #3f418d;
            padding: 20px;
            box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
        }
        .content {
            margin-left: 10px;
            padding: 20px;
            background-color: #cadcfc;
        }
        .footer {
            background-color: rgb(249, 243, 243);
            text-align: center;
            padding: 10px;
            color: ghostwhite;
        }
        .footer a {
            color: midnightblue;
            text-decoration: none;
        }
        .footer a:hover {
            text-decoration: underline;
        }
         /* CSS classes for custom border and background colors */

  .blue-background {
            background-color: #007bff; /* Blue background */
            color: white; /* White text for contrast */
            border: 3px solid #000000; /* Black border */
        }
.local-tour-border {
    border: 3px solid #FF5733;  /* Orange border for Local & Tour Total */
    background-color: #007bff;  /* Blue background */
    color: white;  /* Text color to white to make it stand out on the blue background */
}

.expense-total-border {
    border: 3px solid #28a745;  /* Green border for Expense Total */
    background-color: #007bff;  /* Blue background */
    color: white;  /* Text color to white */
}

.department-total-border {
    border: 3px solid #007bff;  /* Blue border for Department Total */
    background-color: #007bff;  /* Blue background */
    color: white;  /* Text color to white */
}

    </style>

       <aside id="sidebar" class="sidebar" style="box-shadow: 0 2px 10px darkblue;">

   <ul class="sidebar-nav" id="sidebar-nav">
      <%-- <li class="nav-item">
    <a class="nav-link" href="Dashboard.aspx">
        <i class="bi bi-grid"></i>
        <span>Dashboard</span>
    </a>
</li>--%>
         <li class="nav-item">
    <a class="nav-link" href="AdminPage.aspx">
       <i class="bi bi-pc-display"></i>
        <span>AdminPage</span>
    </a>
</li>
          <li class="nav-item">
  <a class="nav-link " href="Employeecreation.aspx">
      <i class="bi bi-personbi bi-person-circle"></i><span>Employee Creation</span>
  </a>
</li>
                   
        
      <li class="nav-item">
    
            <a class="nav-link " href="AdminCustomer_Creation.aspx">
               <i class="bi bi-person-workspace"></i><span>Customer Creation</span>
            </a>
          </li>

                          <li class="nav-item">
  <a class="nav-link " href="AdminService_Assign.aspx">
      <i class="bi bi-diagram-3"></i><span>Service Assignment</span>
  </a>
</li>
                                        <li class="nav-item">
  <a class="nav-link " href="Reportform.aspx">
      <i class="bi bi-filetype-exe"></i><span>Report</span>
  </a>

</li>

                                 <li class="nav-item">
  <a class="nav-link " href="CombinedReport.aspx">
      <i class="bi bi-folder-fill"></i><span>Combined Report</span>
  </a>

</li>
       
                                 
       
                <li class="nav-item">
  <a class="nav-link " href="DocView.aspx">
      <i class="bi bi-file-earmark-pdf-fill"></i><span> Attachment</span>
  </a>

</li>
            
            
   </ul>
        </aside>

    <div class="content">
        <main id="main" class="main">
            <section class="section dashboard">
                <div class="row">
                    <div class="col">
                        <div class="card">
                            <h5 class="card-title" style="text-align:center;background-color:#3f418d;color:white">Combined Reports</h5>
                            <section class="form-container section error-404 d-flex flex-column align-items-center justify-content-center" >
                                 <div>
      
                             <asp:Label ID="lblFromDate" runat="server" Text="From Date:" ></asp:Label>
                             <asp:TextBox ID="txtFromDate" runat="server" TextMode="date"></asp:TextBox>
                             <asp:Label ID="lblToDate" runat="server" Text="To Date:"></asp:Label>
                             <asp:TextBox ID="txtToDate" runat="server" TextMode="date"></asp:TextBox>
                             <asp:Label ID="BranchName" runat="server" Text="Branch Name:"></asp:Label>
                             <asp:DropDownList ID="ddlBranch" runat="server"></asp:DropDownList>
                             <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" />
                         </div>
                                <br />
                                <h2>Expense Report</h2>
                                
                                <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                               
                                <div class="scrollable-container1" style="width:500px">
                                    <asp:GridView ID="gvExpenseReport" runat="server" AutoGenerateColumns="False" CssClass="mydatagrid" Style="margin: 0; padding: 0;">
    <Columns>
        <asp:BoundField DataField="EngineerName" HeaderText="Engineer Name" SortExpression="EngineerName" />
        <asp:BoundField DataField="LocalExpenses" HeaderText="LocalExpenses" />
        <asp:BoundField DataField="TourExpenses" HeaderText="TourExpenses" />
        <asp:BoundField DataField="OverallExpenses" HeaderText="Overall Expenses" />
    </Columns>
</asp:GridView>

                                </div>

                                <h2>SMO Report</h2>
                                <div class="scrollable-container">
                                    <asp:GridView ID="gvSmoSoReport" runat="server" AutoGenerateColumns="False" CssClass="mydatagrid" Style="margin: 0; padding: 0;">
                                        <Columns>
                                            
                                               
                                            <asp:BoundField DataField="CombinedNo" HeaderText="Smo No - So No" />
                                            <asp:BoundField DataField="TotalClaimedAmount" HeaderText="Total Amount" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            <h2>Export Report</h2>
                           <div class="scrollable-container">
 <asp:GridView ID="gvExportReport" runat="server" AutoGenerateColumns="False" CssClass="mydatagrid" 
              OnRowDataBound="gvExportReport_RowDataBound">
        <Columns>
            <asp:TemplateField HeaderText="Expense Type">
                <ItemTemplate>
                    <%# Eval("ExpenseType") %>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Amount">
                <ItemTemplate>
                    <%# Eval("Amount") %>  
                </ItemTemplate>
            </asp:TemplateField>

<%--            <asp:TemplateField HeaderText="Overall Total">
                <ItemTemplate>
                    <%# Eval("OverallTotal") %>  <!-- Currency formatting for overall total remains -->
                </ItemTemplate>
            </asp:TemplateField>--%>
        </Columns>
    </asp:GridView>
</div>

                        </section>
                    </div>
                </div>
            </div>
        </section>

        <asp:Button ID="btnGenerate" runat="server" Text="Generate Excel" OnClick="btnGenerate_Click" CssClass="btn-primary" style="background-color:#3f418d; color:white;"/>
    </main>
</div>

<footer class="footer">
    <p>&copy; 2024 Your Company. All rights reserved.</p>
</footer>
</asp:Content>