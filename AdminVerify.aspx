<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="AdminVerify.aspx.cs" Inherits="Vivify.AdminVerify" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main id="main" class="main container d-flex flex-column align-items-center">

        <style>
            /* Inline CSS for better readability */
            .table {
                border-collapse: collapse; /* Collapses border spacing */
                width: 100%; /* Full width */
            }

            .table th, .table td {
                border: 1px solid #dee2e6; /* Border color */
                padding: 8px; /* Padding for cells */
                text-align: left; /* Align text */
            }

            .table th {
                background-color: #3f418d; /* Header background color */
                color: white; /* Header text color */
            }

            .table tr:nth-child(even) {
                background-color: #f2f2f2; /* Zebra striping */
            }

            .amount-container {
                display: flex;
                align-items: center; /* Center items vertically */
                margin-bottom: 10px; /* Space between items */
            }

            .button-container {
                margin-top: 20px; /* Spacing for buttons */
            }


            .total-amount {
                font-weight: bold; /* Bold text for totals */
                text-align: right; /* Right-align total amounts */
            }
             .form-control {
     width: 100px;
     padding: 5px;
    /* margin-bottom: 15px;*/
     border-radius: 4px;
     border: 2px solid darkblue;
     text-align:center;
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
            .custom-card {
                width: 100%; /* Full width of the viewport */
                max-width: 1200px; /* Maximum width */
                margin-top: 20px; /* Space above card */
            }
            .sidebar {
     background-color:#3f418d;
    /* Sidebar background */
    padding: 20px; /* Add some padding */
}

.sidebar-nav .nav-link {
    display: flex; /* Align icon and text */
    align-items: center; /* Center vertically */
    padding: 12px 20px; /* Increased padding */
    border-radius: 5px; /* Rounded corners */
    color: #222b65; /* Text color */
   /* transition: background 0.3s, color 0.3s;*/
     background-color:white;/* Smooth transitions */
}

 /*.sidebar-nav .nav-link:hover {*/
       /* background-color:white; *//* Highlight background on hover */
        /*color: red;*/ /* Change text color on hover */
        /*transition: background 0.3s, color 0.3s, border 0.3s; 
        border : 2px solid #222b65;
        box-shadow: 0 2px 10px #1f2b60;
    }*/

.sidebar-nav .nav-link.active {
    background-color: #222b65; /* Active link background */
    color: white; /* Active link text color */
}

.sidebar-nav .nav-item {
    margin-bottom: 10px; /* Space between items */
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
  <a class="nav-link " href="ServiceAssignment.aspx">
      <i class="bi bi-diagram-3"></i><span>Service Assignment</span>
  </a>
</li>

                                 <li class="nav-item">
  <a class="nav-link " href="CombinedReport.aspx">
      <i class="bi bi-folder-fill"></i><span>Combined Report</span>
  </a>

</li>
       
                                 <li class="nav-item">
  <a class="nav-link " href="Reportform.aspx">
      <i class="bi bi-filetype-exe"></i><span>Report</span>
  </a>

</li>
                <li class="nav-item">
  <a class="nav-link " href="DocView.aspx">
      <i class="bi bi-file-earmark-pdf-fill"></i><span> Attachment</span>
  </a>

</li>
            
   </ul>
        </aside>
        <div class="card custom-card">
            <h5 class="card-title text-center" style="background-color: #3f418d; color: ghostwhite;">Admin Verification</h5>
            <section class="section error-404 d-flex flex-column align-items-center justify-content-center" style="box-shadow: 0 2px 10px #1f2b60; ">

                <div class="scrollable-container">
                    <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="ExpenseId" CssClass="table">
                        <Columns>
                            <asp:BoundField DataField="SourceTable" HeaderText="Expense Category" />
                            <asp:BoundField DataField="ExpenseType" HeaderText="Expense Type" />
                             <asp:BoundField DataField="Id" HeaderText="Id" Visible="true" /> 
                            <asp:BoundField DataField="Date" HeaderText="Date" />
                            <asp:TemplateField HeaderText="From Time">
                                <ItemTemplate>
                                    <asp:Label ID="lblFromTime" runat="server" Text='<%# Eval("FromTime") != DBNull.Value ? Convert.ToDateTime(Eval("FromTime")).ToString("HH:mm") : string.Empty %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="To Time">
                                <ItemTemplate>
                                    <asp:Label ID="lblToTime" runat="server" Text='<%# Eval("ToTime") != DBNull.Value ? Convert.ToDateTime(Eval("ToTime")).ToString("HH:mm") : string.Empty %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Particulars" HeaderText="Particulars" />
                            <asp:BoundField DataField="Distance" HeaderText="Distance(Km)" />
                            <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                            <asp:TemplateField HeaderText="Amounts">
                                <ItemTemplate>
                                    <div class="amount-container">
                                        <asp:CheckBox ID="chkConveyanceClaimable" runat="server" AutoPostBack="true" 
                                                      OnCheckedChanged="chkClaimable_CheckedChanged" 
                                                      Visible='<%# !string.IsNullOrEmpty(Eval("ConveyanceAmount")?.ToString()) %>' />
                                        <asp:TextBox ID="txtConveyanceAmount" runat="server" 
                                                     Text='<%# Eval("ConveyanceAmount") %>' 
                                                     ReadOnly="true" CssClass="form-control mx-2" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("ConveyanceAmount")?.ToString()) %>' />
                                        <asp:Button ID="btnEditConveyance" runat="server" Text="Edit" 
                                                     CommandName="EditRow" CommandArgument='<%# Container.DataItemIndex %>' 
                                                     CssClass="btn btn-secondary" 
                                                     OnClick="btnEditRow_Click" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("ConveyanceAmount")?.ToString()) %>' />
                                    </div>
                                    <div class="amount-container">
                                        <asp:CheckBox ID="chkFoodClaimable" runat="server" AutoPostBack="true" 
                                                      OnCheckedChanged="chkClaimable_CheckedChanged" 
                                                      Visible='<%# !string.IsNullOrEmpty(Eval("FoodAmount")?.ToString()) %>' />
                                        <asp:TextBox ID="txtFoodAmount" runat="server" 
                                                     Text='<%# Eval("FoodAmount") %>' 
                                                     ReadOnly="true" CssClass="form-control mx-2" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("FoodAmount")?.ToString()) %>' />
                                        <asp:Button ID="btnEditFood" runat="server" Text="Edit" 
                                                     CommandName="EditRow" CommandArgument='<%# Container.DataItemIndex %>' 
                                                     CssClass="btn btn-secondary" 
                                                     OnClick="btnEditRow_Click" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("FoodAmount")?.ToString()) %>' />
                                    </div>
                                    <div class="amount-container">
                                        <asp:CheckBox ID="chkOthersClaimable" runat="server" AutoPostBack="true" 
                                                      OnCheckedChanged="chkClaimable_CheckedChanged" 
                                                      Visible='<%# !string.IsNullOrEmpty(Eval("OtherAmount")?.ToString()) %>' />
                                        <asp:TextBox ID="txtOthersAmount" runat="server" 
                                                     Text='<%# Eval("OtherAmount") %>' 
                                                     ReadOnly="true" CssClass="form-control mx-2" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("OtherAmount")?.ToString()) %>' />
                                        <asp:Button ID="btnEditOthers" runat="server" Text="Edit" 
                                                     CommandName="EditRow" CommandArgument='<%# Container.DataItemIndex %>' 
                                                     CssClass="btn btn-secondary" 
                                                     OnClick="btnEditRow_Click" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("OtherAmount")?.ToString()) %>' />
                                    </div>
                                    <div class="amount-container">
                                        <asp:CheckBox ID="chkMiscellaneousClaimable" runat="server" AutoPostBack="true" 
                                                      OnCheckedChanged="chkClaimable_CheckedChanged" 
                                                      Visible='<%# !string.IsNullOrEmpty(Eval("MiscellaneousAmount")?.ToString()) %>' />
                                        <asp:TextBox ID="txtMiscellaneousAmount" runat="server" 
                                                     Text='<%# Eval("MiscellaneousAmount") %>' 
                                                     ReadOnly="true" CssClass="form-control mx-2" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("MiscellaneousAmount")?.ToString()) %>' />
                                        <asp:Button ID="btnEditMiscellaneous" runat="server" Text="Edit" 
                                                     CommandName="EditRow" CommandArgument='<%# Container.DataItemIndex %>' 
                                                     CssClass="btn btn-secondary" 
                                                     OnClick="btnEditRow_Click" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("MiscellaneousAmount")?.ToString()) %>' />
                                    </div>
                                    <div class="amount-container">
                                        <asp:CheckBox ID="chkLodgingClaimable" runat="server" AutoPostBack="true" 
                                                      OnCheckedChanged="chkClaimable_CheckedChanged" 
                                                      Visible='<%# !string.IsNullOrEmpty(Eval("LodgingAmount")?.ToString()) %>' />
                                        <asp:TextBox ID="txtLodgingAmount" runat="server" 
                                                     Text='<%# Eval("LodgingAmount") %>' 
                                                     ReadOnly="true" CssClass="form-control mx-2" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("LodgingAmount")?.ToString()) %>' />
                                        <asp:Button ID="btnEditLodging" runat="server" Text="Edit" 
                                                     CommandName="EditRow" CommandArgument='<%# Container.DataItemIndex %>' 
                                                     CssClass="btn btn-secondary" 
                                                     OnClick="btnEditRow_Click" 
                                                     Visible='<%# !string.IsNullOrEmpty(Eval("LodgingAmount")?.ToString()) %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <asp:Label ID="lblTotalClaimed" runat="server" Text="Total Claimed: " />
                <asp:TextBox ID="txtTotalClaimedAmount" runat="server" ReadOnly="true" CssClass="form-control total-amount" style="align-content:center;"/>
                <br />
                <asp:Label ID="lblTotalNonClaimed" runat="server" Text="Total Non-Claimed: " />
                <asp:TextBox ID="txtTotalNonClaimedAmount" runat="server" ReadOnly="true" CssClass="form-control total-amount" style="align-content:center;" />
                <br />
                <div class="button-container d-flex">
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" CssClass="btn btn-primary" />
                </div>
                <br />
               <div>
<asp:Button ID="btnExportToExcel" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click" CssClass="btn btn-success" />

 </div>
                <div class="footer">
                    <asp:Label ID="lblMessage" runat="server" ForeColor="Green" Visible="false"></asp:Label>
                </div>

             <div class="back ">
    <asp:Button ID="btnBack" runat="server" Text="Back" OnClick="btnBack_Click" CssClass="btn btn-primary" />
</div>

            </section>
        </div>
    </main>
</asp:Content>