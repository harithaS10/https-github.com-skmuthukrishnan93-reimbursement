<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Reportform.aspx.cs" Inherits="Vivify.Reportform" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }

        h1 {
            color: #333;
            text-align: center;
            margin-bottom: 20px;
        }

        .HeaderStyle {
            width: 100%;
        }

        .grid-container {
            max-height: 500px;
            overflow-y: auto;
            border: 1px solid #ddd;
            border-radius: 8px;
            background-color: white;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }

        .gridview {
            width: 100%;
            border-spacing: 0;
            margin: 0;
        }

        .gridview th, .gridview td {
            border: 1px solid #ddd;
            padding: 12px;
            text-align: center;
        }

        .gridview th {
            background-color:darkblue;
            color: white;
            position: sticky;
            top: 0;
            z-index: 10;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .gridview tr:nth-child(even) {
            background-color: #f9f9f9;
        }

        .gridview tr:hover {
            background-color: #f1f1f1;
        }

        .border-right {
            border-right: 4px solid #ddd;
        }

        #btnGenerate {
            display: block;
            margin: 20px auto;
            padding: 10px 20px;
            font-size: 16px;
            background-color: #3f418d;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            transition: background-color 0.3s;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }

        #btnGenerate:hover {
            background-color: #3f418d;
        }

        #header {
            background-color: #3f418d;
        }

        .sidebar {
            width: 300px;
            float: left;
            background: #f8f9fa;
            padding: 20px;
            box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
        }

        .sidebar {
            background-color: #3f418d;
            padding: 20px;
        }

        .sidebar-nav .nav-link {
            display: flex;
            align-items: center;
            padding: 12px 20px;
            border-radius: 5px;
            color: #222b65;
            background-color: white;
        }

        .sidebar-nav .nav-link.active {
            background-color: #222b65;
            color: white;
        }

        .sidebar-nav .nav-item {
            margin-bottom: 10px;
        }

        /* Ensure the Date column does not wrap and is displayed in a single row */
        .date-column {
            white-space: nowrap;  /* Prevents text from wrapping */
            width: 800px;         /* Set a fixed width for the Date column */
        }

        /* Increase the width of Particulars column */
        .particulars-column {
            white-space: nowrap;  /* Prevents wrapping of text */
            width: 250px;         /* Increased width for the Particulars column */
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
                <a class="nav-link" href="Employeecreation.aspx">
                    <i class="bi bi-person-circle"></i><span>Employee Creation</span>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="AdminCustomer_Creation.aspx">
                    <i class="bi bi-person-workspace"></i><span>Customer Creation</span>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="AdminService_Assign.aspx">
                    <i class="bi bi-diagram-3"></i><span>Service Assignment</span>
                </a>
            </li>
            <li class="nav-item">
    <a class="nav-link" href="Reportform.aspx">
        <i class="bi bi-filetype-exe"></i><span>Report</span>
    </a>
</li>
            <li class="nav-item">
                <a class="nav-link" href="CombinedReport.aspx">
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
                            <h5 class="card-title" style="text-align:center;background-color:#3f418d;color:white">Report Form</h5>
                            <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                            <section class="scrollable-container">
                                <div>
                                    <asp:Label ID="lblFromDate" runat="server" Text="From Date:" ></asp:Label>
                                    <asp:TextBox ID="txtFromDate" runat="server" TextMode="date"></asp:TextBox>
                                    <asp:Label ID="lblToDate" runat="server" Text="To Date:"></asp:Label>
                                    <asp:TextBox ID="txtToDate" runat="server" TextMode="date"></asp:TextBox>
                                    <asp:Label ID="BranchName" runat="server" Text="Branch Name:"></asp:Label>
                                    <asp:DropDownList ID="ddlBranch" runat="server"></asp:DropDownList>
                                    <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click"  style="background-color:darkblue; color:white;"/>
                                </div>
                                <div class="grid-container">
                                    <asp:GridView ID="gvReport" runat="server" AutoGenerateColumns="false" CssClass="gridview">
                                        <HeaderStyle BackColor="#4CAF50" ForeColor="White" />
                                        <Columns>
                                           
                                            <asp:BoundField DataField="Eng_Name" HeaderText="Eng Name" ItemStyle-Width="150px" />
                                            <asp:BoundField DataField="Tour_Local" HeaderText="Tour/Local" />

                                            
                                            <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" 
                                                HeaderStyle-Width="200px" ItemStyle-Width="200px" ItemStyle-CssClass="date-column" />

                                            
                                            <asp:BoundField DataField="Particulars" HeaderText="Particulars" 
                                                ItemStyle-CssClass="particulars-column" ItemStyle-Width="250px" />

                                            <asp:BoundField DataField="Distance" HeaderText="Distance" />
                                            <asp:BoundField DataField="Transport" HeaderText="Transport" />
                                            <asp:BoundField DataField="Conveyance" HeaderText="Conveyance" />
                                           
                                            <asp:BoundField DataField="Food" HeaderText="Food" />
                                            <asp:BoundField DataField="Others" HeaderText="Others" />
                                             <asp:BoundField DataField="Lodging" HeaderText="Lodging" />
                                            <asp:BoundField DataField="Miscellaneous" HeaderText="Misc" />
                                            <asp:BoundField DataField="Total" HeaderText="Total" />
                                            <asp:BoundField DataField="SO_Number" HeaderText="SO Number" />
                                            <asp:BoundField DataField="Department" HeaderText="Department" />
                                            <asp:BoundField DataField="Nature_of_Work" HeaderText="Nature of Work" />
                                            <asp:BoundField DataField="SMO" HeaderText="SMO/SO/WBS" />
                                            <asp:BoundField DataField="Document_Reference" HeaderText="Document Reference" />
                                            <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                                <asp:Button ID="btnGenerate" runat="server" Text="Generate Excel" OnClick="btnGenerate_Click" style="background-color:darkblue; color:white;" />
                            </section>
                        </div>
                    </div>
                </div>
            </section>
        </main>
    </div>
</asp:Content>
