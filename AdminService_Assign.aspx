<%@ Page Title="" Language="C#" MasterPageFile="/Main.Master" AutoEventWireup="true" CodeBehind="AdminService_Assign.aspx.cs" Inherits="Vivify.AdminService_Assign" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <head>
        <title>Service Assignment</title>
        <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
        <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
        <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
        <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
        <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.bundle.min.js"></script>

        <style>
     .ui-datepicker {
         width: 150px;
         font-size: 12px;
     }
     .ui-datepicker-header {
         background: #007bff;
         color: white;
     }
     .ui-datepicker-calendar .ui-state-active {
         background: #007bff;
         color: white;
     }
     .ui-datepicker-calendar .ui-state-disabled {
         background: #f5f5f5;
         color: #ccc;
     }
     .form-select {
         width: 100%;
         padding: 5px;
      /*   margin-bottom: 15px;*/
         border-radius: 4px;
         border: 2px solid darkblue;
     }
     .form-control {
         width: 100%;
         padding: 5px;
        /* margin-bottom: 15px;*/
         border-radius: 4px;
         border: 2px solid darkblue;
     }
     .form-label {
         color: black;
         font-weight: bold;
     }
     .form-container {
         padding: 20px;
         background-color: #f8f9fa;
         border-radius: 5px;
         box-shadow: 0 2px 10px darkblue;
     }
     .btn-primary{
         background-color:#3f418d;
     }
     @media (max-width: 768px) {
         .form-container {
             margin: 10px;
             padding: 15px;
         }
         .form-select, .form-control {
             font-size: 14px;
         }
         .form-label {
             font-size: 14px;
         }
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
    </head>
    <main id="main" class="main">
        <div class="formarea">
            <section class="section dashboard">
                <div class="row">
                    <div class="col">
                        <div class="card">
                            <h5 class="card-title" style="text-align:center; background-color:#3f418d; color:white">Service Assignment</h5>
                            <section class="form-container section error-404 d-flex flex-column align-items-center justify-content-center" style=" box-shadow: 0 2px 10px darkblue; margin:0px;">
                                <div class="row g-3 needs-validation">
                                    <div class="col-12 ">
                                        <label for="ddlBranch" class="form-label">Branch</label>
                                        <asp:DropDownList ID="ddlBranch" runat="server" OnSelectedIndexChanged="ddlBranch_SelectedIndexChanged" AutoPostBack="true" class="form-select">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-12 ">
                                        <label for="ddlCustId" class="form-label">Customer Name</label>
                                        <asp:DropDownList ID="ddlCustId" runat="server" class="form-select">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-12 ">
                                        <label for="ddlEmpId" class="form-label">Emp Name & ID</label>
                                        <asp:DropDownList ID="ddlEmpId" runat="server" class="form-select">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-12 ">
                                        <label for="ddlservice" class="form-label">Service Type</label>
                                        <asp:DropDownList ID="ddlservice" runat="server" class="form-select">
                                            <asp:ListItem Text="Select" Value="0"></asp:ListItem>
                                            <asp:ListItem Text="CM" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="PM" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="Installation" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="Shifting" Value="4"></asp:ListItem>
                                            <asp:ListItem Text="Others" Value="5"></asp:ListItem>
                                            
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-12 ">
                                        <label for="txtAdvance" class="form-label">Advance</label>
                                        <asp:TextBox id="txtAdvance" runat="server" ValidationGroup="TimeSlot" ClientIDMode="Static" class="form-control"></asp:TextBox>
                                    </div>
                                    <div class="col-12 ">
                                        <label for="txtFromDate" class="form-label">From Date</label>
                                        <asp:TextBox ID="txtFromDate" runat="server" class="form-control" ClientIDMode="Static" />
                                    </div>
                                    <div class="col-12">
                                        <label for="txtToDate" class="form-label">To Date</label>
                                        <asp:TextBox ID="txtToDate" runat="server" class="form-control" ClientIDMode="Static" />
                                    </div>
                                    <div class="col-12 ">
                                          <label for="ddldepartment" class="form-label">Department</label>
                                   
                                     <asp:DropDownList ID="ddldepartment" runat="server" CssClass="form-select">
     <asp:ListItem Text="Select " Value="0"></asp:ListItem>
                                         <asp:ListItem Text="Sales " Value="1"></asp:ListItem>
    <asp:ListItem Text="Services" Value="2"></asp:ListItem>
                                           <asp:ListItem Text="Project" Value="3"></asp:ListItem>
                                           <asp:ListItem Text="Refresh" Value="4"></asp:ListItem>
   
</asp:DropDownList>
 </div>
                                    <div class="col-12 ">
                                        <label for="txtremarks" class="form-label">Remarks</label>
                                        <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" ClientIDMode="Static" />
                                    </div>
                                    <div class="col-12 ">
                                        <asp:Button ID="btnService" OnClick="btnService_Click" class="btn btn-primary w-100" Text="Submit" runat="server" style="background-color: #3f418d;"/>
                                    </div>
                                </div>
                            </section>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    </main>

    <script>
        $(function () {
            $("#<%= txtFromDate.ClientID %>").datepicker({
                dateFormat: "yy-mm-dd"
            });
            $("#<%= txtToDate.ClientID %>").datepicker({
                dateFormat: "yy-mm-dd"
            });
        });
    </script>
</asp:Content>