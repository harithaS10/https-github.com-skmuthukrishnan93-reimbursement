<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" Codebehind="AdminCustomer_Creation.aspx.cs" Inherits="Vivify.AdminCustomer_Creation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main id="main" class="main">
        <style>
            /* General Styling */
            .mydatagrid th, .mydatagrid td {
                border: 1.5px solid black;
                padding: 12px; /* Increase padding for more height */
                box-shadow: #1f2b60;
               
            }
            .mydatagrid th{
                background-color:#3f418d;
                color:ghostwhite;
            }
            /* Form Container Styling */
            .form-container {
                padding: 20px;
                background-color: #f8f9fa;
                border-radius: 5px;
                box-shadow: 0 2px 10px darkblue;
                width: 100%;
                max-width: 600px;
                margin: 0 auto;
            }

            /* Form controls (input, select, button) */
            .form-select, .form-control, .btn-primary {
                width: 100%;
                padding: 12px;
                margin-bottom: 15px;
                border-color:darkblue;
            }
            .form-control{
                margin:0px;
            }
            .main {
                margin: 0;
                padding: 0;
                background-color: #cadcfc;
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
            }

            .scrollable-container {
                max-height: 390px;
                overflow: auto;
                border: 1px solid black;
                box-shadow: 0 2px 10px #1f2b60;
                margin: 25px auto;
                width: 100%;
                max-width: 900px;
            }

            .table {
                width: 100%;
                table-layout: fixed;
                border-collapse: collapse;
            }

            .table th, .table td {
                padding: 10px;
            }

 .scrollable-container {
     max-height: 390px; /* Set a maximum height */
     overflow: auto; /* Enable scrolling if needed */
     border: 1px solid #1f2b60;
     box-shadow: 0 2px 10px darkblue;
     margin: 0 auto; /* Center the container */
     width: 90%; /* Set width as needed */
 }




            /* Mobile and Tablet Adjustments */
            @media (max-width: 768px) {
                .form-container {
                    padding: 15px;
                }

                .main {
                    padding: 20px;
                }

                .table th, .table td {
                    padding: 8px;
                }

                .btn-primary {
                    font-size: 16px; /* Increase button font size for better readability */
                }
            }

            @media (max-width: 600px) {
                /* Hide unnecessary columns in the table */
                .table th:nth-child(4), .table td:nth-child(4) {
                    display: none; /* Hide BranchId column on small screens */
                }

                .btn-primary {
                    font-size: 16px; /* Increase button font size for better readability */
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
       
        <section class="section dashboard">
            <div class="row">
                <div class="col">
                    <div class="card">
                        <h5 class="card-title" style="text-align:center;background-color: #3f418d;color:ghostwhite">Customer Creation</h5>
                        <section class="form-container section error-404 d-flex flex-column">
                            <div class="row g-3 needs-validation">
                                <div class="col-12  mb-0">
                                    <label for="ddlBranch" class="form-label">Branch</label>
                                    <asp:DropDownList ID="ddlBranch" runat="server" class="form-select">
                                       
                                    </asp:DropDownList>
                                </div>

                                <div class="col-12 mb-0 ">
                                    <label for="txtCustomerName" class="form-label">Customer Name</label>
                                    <asp:TextBox id="txtCustomerName" runat="server" class="form-control"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" id="RequiredFieldValidator1" ForeColor="OrangeRed" controltovalidate="txtCustomerName" errormessage="Please enter a customer name!" />
                                </div>

                                <div class="col-12 mb-0 ">
                                    <label for="txtAddress" class="form-label">Address</label>
                                    <asp:TextBox id="txtAddress" runat="server" TextMode="MultiLine" class="form-control"></asp:TextBox>
                                    <asp:RequiredFieldValidator runat="server" id="RequiredFieldValidator2" ForeColor="OrangeRed" controltovalidate="txtAddress" errormessage="Please enter an address!" />
                                </div>

                                <div class="col-12  mb-0" style=" color:#1f2b60">
                                    <asp:Button ID="btnCustomerCreate" class="btn btn-primary w-100" OnClick="btnCustomerCreate_Click" Text="Create Customer" runat="server" style="background-color: #3f418d;" />
                                </div>
                            </div>
                        </section>
                    </div><!-- End Card -->
                </div><!-- End Column -->
            </div><!-- End Row -->
        </section><!-- End Dashboard Section -->

        <section class="scrollable-container">
            <div class="table-responsive">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="mydatagrid table">
                    <Columns>
                        <asp:BoundField DataField="CustomerId" HeaderText="CustomerId" ReadOnly="True" />
                        <asp:BoundField DataField="CustomerName" HeaderText="CustomerName" />
                        <asp:BoundField DataField="Address1" HeaderText="Address1" />
                        <asp:BoundField DataField="BranchId" HeaderText="BranchId" />
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
