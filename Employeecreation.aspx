<%@ Page Title="" Language="C#" MasterPageFile="/Main.Master" AutoEventWireup="true" CodeBehind="Employeecreation.aspx.cs" Inherits="Vivify.Employeecreation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <head>
        <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" />
        <style>
            .form-label {
                font-weight: bold;
                color: #333;
                margin-bottom: 0.25rem; /* Reduced bottom margin */
            }
           .form-select{
        width: 100%;
    padding: 5px;
   /* margin-bottom: 15px;*/
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
            .form-control:focus {
                border-color: #80bdff;
                box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
            }
            .required-field {
                color: OrangeRed;
                margin-top: 0.25rem;
                font-size: 0.875rem;
            }
            .btn-primary {
                background-color: #3f418d;
                border-color: #3f418d;
            }
            .btn-primary:hover {
                background-color: #0056b3;
                border-color: #0056b3;
            }
            .form-container {
                padding: 15px; /* Slightly less padding around the form */
                background-color: #f8f9fa;
                border-radius: 5px;
                box-shadow: 0 2px 10px darkblue;
            }
            .table-container {
                display: block;
                max-height: 400px;
                overflow-y: auto;
                box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
                background-color: white;
                border-radius: 5px;
            }
            .table th {
                position: sticky;
                top: 0;
                background-color: #3f418d;
                color: ghostwhite;
                z-index: 10;
                text-align: center;
            }
            .table th, .table td {
                padding: 8px;
                border: 1px solid #dee2e6;
            }
        </style>
            <aside id="sidebar" class="sidebar" style="box-shadow: 0 2px 10px darkblue;">

   <ul class="sidebar-nav" id="sidebar-nav">
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
    
            <a class="nav-link " href="AdminCustomer_creation.aspx">
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
        <meta name="viewport" content="width=device-width, initial-scale=1">
    </head>
    
    <main id="main" class="main" style="background-color: #cadcfc;">
        <div class="container">
            <div class="formarea">
                <section class="section dashboard">
                    <div class="row">
                        <div class="col">
                            <div class="card">
                                 <h5 class="card-title" style="text-align:center;background-color:#3f418d;color:white">Employee Creation</h5>
                                <section class="form-container section error-404 d-flex flex-column align-items-center justify-content-center" >
                                    <div class="row g-1 needs-validation"> <!-- Changed g-3 to g-1 for tighter spacing -->
                                        <div class="col-12 ">
                                            <label for="ddlBranch" class="form-label">Branch</label>
                                            <asp:DropDownList ID="ddlBranch" runat="server" CssClass="form-select">
                                            </asp:DropDownList>
                                        </div>

                                        <div class="col-12 ">
                                            <label for="txtcode" class="form-label">Employee Code</label>
                                            <asp:TextBox ID="txtcode" runat="server" ClientIDMode="Static" CssClass="form-control"></asp:TextBox>
                                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtcode" CssClass="required-field" ErrorMessage="Please enter employee code!" />
                                        </div>

                                        <div class="col-12 ">
                                            <label for="txtName" class="form-label">First Name</label>
                                            <asp:TextBox ID="txtName" runat="server" ClientIDMode="Static" CssClass="form-control"></asp:TextBox>
                                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" CssClass="required-field" ErrorMessage="Please enter a first name!" />
                                        </div>

                                        <div class="col-12 ">
                                            <label for="txtLname" class="form-label">Last Name</label>
                                            <asp:TextBox ID="txtLname" runat="server" ClientIDMode="Static" CssClass="form-control"></asp:TextBox>
                                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLname" CssClass="required-field" ErrorMessage="Please enter a last name!" />
                                        </div>

                                        <div class="col-12 ">
                                        <label for="txtMobno" class="form-label">Mobile Number</label>
                                        <asp:TextBox ID="txtMobno" runat="server" ClientIDMode="Static" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtMobno" CssClass="required-field" ErrorMessage="Please enter a mobile number!" />
    
                                     
                                        <asp:RegularExpressionValidator 
                                            runat="server" 
                                            ControlToValidate="txtMobno" 
                                            CssClass="required-field" 
                                            ErrorMessage="Please enter a valid 10-digit mobile number!" 
                                            ValidationExpression="^\d{10}$" />
                                    </div>


                                        <div class="col-12">
                                            <label for="txtOfcemail" class="form-label">Official Email</label>
                                            <asp:TextBox ID="txtOfcemail" runat="server" TextMode="Email" ClientIDMode="Static" CssClass="form-control"></asp:TextBox>
                                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtOfcemail" CssClass="required-field" ErrorMessage="Please enter an office email!" />
                                            <asp:RegularExpressionValidator 
                    runat="server" 
                    ControlToValidate="txtOfcemail" 
                    CssClass="required-field" 
                    ErrorMessage="Invalid email format!" 
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$">
                </asp:RegularExpressionValidator>
                                        </div>

                                        <div class="col-12 ">
                                            <label for="ddldesignation" class="form-label">Designation</label>
                                            <asp:DropDownList ID="ddldesignation" runat="server" CssClass="form-select">
                                                  <asp:ListItem Text="Select" Value="0"></asp:ListItem>
                                                <asp:ListItem Text="FSE" Value="1"></asp:ListItem>
                                                <asp:ListItem Text="FST" Value="2"></asp:ListItem>
                                            </asp:DropDownList>
                                        </div>

                                        <div class="col-12">
                                            <asp:Button ID="btn1" OnClick="btn1_Click" CssClass="btn btn-primary w-100" Text="Submit" runat="server" ClientIDMode="Static" style="background-color:#3f418d; margin-top:10px;" />
                                        </div>
                                    </div>
                                </section>
                            </div>
                        </div>
                    </div>
                </section>
            </div>
            
            <section class="scrollable-container mt-4">
                <div class="table-responsive">
                    <div class="table-container">
                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="mydatagrid table">
                            <Columns>
                                <asp:TemplateField HeaderText="S.NO">
                                    <ItemTemplate>
                                        <%# Container.DataItemIndex + 1 %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="EmployeeId" HeaderText="EmployeeId" ReadOnly="True" />
                                <asp:BoundField DataField="EmployeeCode" HeaderText="EmployeeCode" />
                                <asp:BoundField DataField="FirstName" HeaderText="FirstName" />
                                <asp:BoundField DataField="MobileNumber" HeaderText="MobileNumber" />
                                <asp:BoundField DataField="OfficialMail" HeaderText="OfficialMail" />
                                <asp:BoundField DataField="PersonalMail" HeaderText="PersonalMail" />
                                <asp:BoundField DataField="Designation" HeaderText="Designation" />
                                <asp:BoundField DataField="BranchName" HeaderText="BranchName" />
                                <asp:BoundField DataField="DepartmentName" HeaderText="DepartmentName" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </section>
        </div>
    </main>
</asp:Content>