<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Refreshdash.aspx.cs" Inherits="Vivify.Refreshdash" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main id="main" class="main">
        <style>
            .mydatagrid th, .mydatagrid td {
                border: 1.5px solid black;
                padding: 12px;
               
            }
            
            .mydatagrid td {
                background-color: white;
            }

            .mydatagrid th {
                background-color: #3f418d;
                color: white;
                position: sticky;
                top: 0;
                z-index: 10;
                text-align: center;
            }
            
            .rows:hover {
                background-color: #f1f1f1;
            }

            .pager {
                text-align: right;
            }

            .scrollable-container {
                max-height: 400px;
                overflow: auto;
                border: 1px solid #1f2b60;
                box-shadow: 0 2px 10px darkblue;
                margin: 0 auto;
                width: 60%;
                max-width:500px;
            }

            .btnReimburse {
                background-color: #3f418d;
                color: white;
                border: none;
                padding: 8px 16px;
                cursor: pointer;
                border-radius: 4px;
                transition: background-color 0.3s, transform 0.2s;
            }

            .custom-button {
                background-color: #3f418d;
                color: white;
                border: none;
                padding: 8px 16px;
                cursor: pointer;
                border-radius: 4px;
                transition: background-color 0.3s, transform 0.2s;
            }

            .main {
                margin: 0;
                padding: 0;
                background-color: #cadcfc;
                height: 85vh;
                display: flex;
                justify-content: center;
                align-items: center;
                overflow: hidden;
            }

            .custom-grid {
                box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
                border-radius: 8px;
                overflow: hidden;
            }
        </style>

        <section class="scrollable-container">
            <div class="custom-grid">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" OnRowCommand="GridView1_RowCommand" CssClass="mydatagrid">
                    <Columns>
                      
                        <asp:TemplateField HeaderText="S.No">
                            <ItemTemplate>
                                <%# Container.DataItemIndex + 1 %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        
                  <asp:TemplateField HeaderText="Employee ID">
    <ItemTemplate>
        <asp:Label ID="lblEmployeeId" runat="server" Text='<%# Eval("EmployeeId") %>'></asp:Label>
    </ItemTemplate>
</asp:TemplateField>


                      
                        <asp:TemplateField HeaderText="First Name">
                            <ItemTemplate>
                                <asp:Label ID="lblFirstName" runat="server" Text='<%# Eval("FirstName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                       
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnReimburse" runat="server"
                                            CommandName="Reimburse"
                                            CommandArgument="<%# Container.DataItemIndex %>"
                                            Text="Assign Refreshment"
                                            CssClass="btnReimburse custom-button" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>