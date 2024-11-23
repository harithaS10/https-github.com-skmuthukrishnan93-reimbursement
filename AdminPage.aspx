<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="AdminPage.aspx.cs" Inherits="Vivify.AdminPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style>
        .mydatagrid th, .mydatagrid td {
            border: 1px solid black;
            padding: 8px;
            box-shadow: #1f2b60;
        }
        .header {
            background-color: #3f418d;
            font-weight: bold;
            color: ghostwhite;
            position: sticky;
            top: 0;
            z-index: 10;
            text-align:center;
        }
        .rows {
            background-color: #ffffff;
        }
        .pager {
            text-align: right;
        }
        .scrollable-container {
            max-height: 400px;
             box-shadow: 0 2px 10px darkblue;
            overflow-x: auto;
            overflow-y: auto;
            border: 1px solid #ccc;
        }
        .sidebar {
            width: 300px;
            float: left;
            background: #f8f9fa;
            padding: 20px;
            box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
        }
        .content {
            margin-left: 30px; /* Adjust margin to fit sidebar */
            padding: 20px;
             background-color:#cadcfc; 
        }
        .custom-button {
            background-color: #3f418d;
            color: white;
            margin-left: 30px; 
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
            border: none;
            padding: 10px 20px;
            cursor: pointer;
            text-align: center;
            font-size: 16px;
            border-radius: 5px;
        }
        .custom-button:hover {
            background-color:#3f418d;
        }
            .toggle-sidebar-btn {
     font-size: 40px;
     color:midnightblue;/* Increase the size of the icon */
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
.footer {
        background-color:rgb(249, 243, 243); /* Footer background color */
        text-align: center; /* Center footer text */
        padding:10px ; /* Padding for footer */
        color: ghostwhite; /* Footer text color */
       /* margin-top:50px;
        margin-bottom:50px;*/
     /*  margin-bottom:0px;*/
    }

    
    .footer a {
        color: midnightblue; /* Link color in footer */
        text-decoration: none; 
       /* Remove underline from links */
    }
    
    .footer a:hover {
        text-decoration: underline; /* Underline on hover */
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
          <div class="filter-section" style="margin-bottom: 20px; text-align: center;">
               <asp:Label ID="BranchName" runat="server" Text="Branch Name:"></asp:Label>
    <asp:DropDownList ID="ddlBranch" runat="server" AutoPostBack="false" CssClass="custom-dropdown">
        <asp:ListItem Text="Select Branch" Value="0" />
    </asp:DropDownList>
    <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" CssClass="custom-button" />
</div>
          <section class="section dashboard">
                <div class="row">
                    <div class="col">
                        <div class="card">
                            <h5 class="card-title" style="text-align:center;background-color:#3f418d;color:white">Verification</h5>
                            <section class="scrollable-container">
                               <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false"
                                    CellPadding="4" CellSpacing="0" GridLines="None"
                                    Width="100%" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                                    RowStyle-CssClass="rows" HeaderStyle-CssClass="header"
                                    style="border: 1.5px solid midnightblue; border-collapse: collapse; font-size:14px; line-height:20px; box-shadow:0 4px 15px rgba(0, 0, 0, 0.2);"
                                    OnRowCommand="GridView1_RowCommand">
                                    <Columns>
                                        <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                                        <asp:BoundField DataField="CustomerName" HeaderText="Customer Name" />
                                        <asp:BoundField DataField="Advance" HeaderText="Advance" />
                                        <asp:BoundField DataField="Total" HeaderText="Total" />
                                        <asp:BoundField DataField="FormattedFromDate" HeaderText="From Date" />
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:Button ID="btnVerify" runat="server" Text="Click here to Verify"
                                                    CommandName="Verify" CommandArgument='<%# Eval("ServiceId") %>'
                                                    CssClass="btn btn-primary custom-button" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </section>
                        </div>
                    </div>
                </div>
            </section>

            <asp:Panel ID="verificationForm" runat="server" Visible="false" style="max-width: 800px; margin: auto;">
                <h3>Verify and Update Expenses</h3>
                <label>Conveyance Total:</label>
                <asp:TextBox ID="txtConveyanceTotal" runat="server" ReadOnly="true" />
                <asp:CheckBox ID="chkConveyanceClaimable" runat="server" Text="Claimable" />
                <asp:TextBox ID="txtConveyanceTotalEditable" runat="server" />
                <br />

                <label>Food Total:</label>
                <asp:TextBox ID="txtFoodTotal" runat="server" ReadOnly="true" />
                <asp:CheckBox ID="chkFoodClaimable" runat="server" Text="Claimable" />
                <asp:TextBox ID="txtFoodTotalEditable" runat="server" />
                <br />

                <label>Miscellaneous Total:</label>
                <asp:TextBox ID="txtMiscellaneousTotal" runat="server" ReadOnly="true" />
                <asp:CheckBox ID="chkMiscellaneousClaimable" runat="server" Text="Claimable" />
                <asp:TextBox ID="txtMiscellaneousTotalEditable" runat="server" />
                <br />

                <label>Others Total:</label>
                <asp:TextBox ID="txtOthersTotal" runat="server" ReadOnly="true" />
                <asp:CheckBox ID="chkOthersClaimable" runat="server" Text="Claimable" />
                <asp:TextBox ID="txtOthersTotalEditable" runat="server" />
                <br />

                <label>Lodging Total:</label>
                <asp:TextBox ID="txtLodgingTotal" runat="server" ReadOnly="true" />
                <asp:CheckBox ID="chkLodgingClaimable" runat="server" Text="Claimable" />
                <asp:TextBox ID="txtLodgingTotalEditable" runat="server" />
                <br />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" />
                <br />
                <asp:Label ID="lblError" runat="server" ForeColor="Red" />
            </asp:Panel>
        </main>
    </div>
</asp:Content>
