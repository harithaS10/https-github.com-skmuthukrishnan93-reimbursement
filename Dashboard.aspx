<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Vivify.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main id="main" class="main">
        <style>
            .mydatagrid th, .mydatagrid td {
                border: 1.5px solid black;
                padding: 12px; /* Increase padding for more height */
                box-shadow: #1f2b60;
            }

            .header {
                background-color: #3f418d; /* Color for the GridView header */
                font-weight: bold;
                color: ghostwhite; /* Text color for the header */
                position: sticky; /* Make header sticky */
                top: 0; /* Stick to the top */
                z-index: 10; /* Ensure it is above other content */
                text-align:center;
            }

            .rows {
                background-color: #ffffff; /* Color for the rows */
            }

            .pager {
                text-align: right; /* Align pager text to the right */
            }

            .scrollable-container {
                max-height: 390px; /* Set a maximum height */
                overflow: auto; /* Enable scrolling if needed */
                border: 1px solid #1f2b60;
                box-shadow: 0 2px 10px darkblue;
                margin: 0 auto; /* Center the container */
                width: 90%; /* Set width as needed */
            }

           .custom-button {
    background-color: #3f418d; /* Button color */
    color: white; 
    align-items: center; /* Button text color */
    margin-left: 30px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
    border: none; /* Remove default border */
}

/* Prevent color change on hover */
.custom-button:hover {
    background-color: #3f418d; /* Maintain the same background color */
    color: white; /* Maintain  same text color */
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2); /* Maintain box shadow */
    cursor: pointer; /* Optional: Change cursor to pointer */
}


            .main {
                margin: 0; /* Remove all margins */
                padding: 0; /* Remove padding */
                background-color:#cadcfc; 
                /* Fill background color */
                height: 85vh; /* Full height of the viewport */
                display: flex; /* Enable flexbox */
                justify-content: center; /* Center content horizontally */
                align-items: center; /* Center content vertically */
                overflow: hidden; /* Prevent scrolling */
            }

            .custom-grid {
                box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
            }
         
        </style>

        <section class="scrollable-container">
            <div class="custom-grid">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CellPadding="4" CellSpacing="0" GridLines="None"
                    Width="100%" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                    RowStyle-CssClass="rows" HeaderStyle-CssClass="header"
                    style="border: 1.5px solid red; border-collapse: collapse; font-size:14px; line-height:20px; box-shadow:0 4px 15px rgba(0, 0, 0, 0.2);"
                    OnRowCommand="GridView1_RowCommand" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"> 
                    <Columns>
                        <asp:TemplateField HeaderText="S.No">
                            <ItemTemplate>
                                <%# Container.DataItemIndex + 1 %> <!-- Correct context for DataItemIndex -->
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer Name" />
                        <asp:BoundField DataField="FromDate" HeaderText="Date" HeaderStyle-Width="100px"
                            ItemStyle-Width="100px" DataFormatString="{0:dd-MMM-yyyy}" HtmlEncode="false" />
                        <asp:BoundField DataField="Advance" HeaderText="Advance" />
                        <asp:BoundField DataField="ServiceType" HeaderText="Service Type" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:HiddenField ID="hdnServiceId" runat="server" Value='<%# Eval("ServiceId") %>' />
                                <asp:Button ID="btnReimburse" runat="server" Text="Proceed to Reimbursement"
                                    CommandName="Reimburse" CommandArgument='<%# Container.DataItemIndex %>'
                                    CssClass="btn btn-primary custom-button" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main><!-- End #main -->
</asp:Content>
