<%@ Page Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Refreshment.aspx.cs" Inherits="Vivify.Refreshment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main id="main" class="main">
        <style>
            .form-label {
                font-weight: bold;
                color: #333;
                margin-bottom: 0.5rem;
            }
            .form-control, .form-select {
                width: 100%;
                padding: 5px;
                margin-bottom: 15px;
                border-radius: 4px;
                border: 2px solid darkblue;
            }
            .custom-button {
                background-color: #3f418d;
                color: white;
                border: none;
                padding: 10px 20px;
                cursor: pointer;
                border-radius: 4px;
                transition: background-color 0.3s, transform 0.2s;
                margin-top: 10px;
            }
            .main {
                margin: 0;
                padding: 0;
                background-color:#cadcfc;
            }
        </style>

        <div class="formarea">
            <section class="section dashboard">
                <div class="row">
                    <div class="col">
                        <div class="card">
                            <h5 class="card-title" style="text-align:center; background-color:#3f418d; color:white">Assign Refreshment</h5>
                            <section class="form-container section error-404 d-flex flex-column "  style=" box-shadow: 0 2px 10px #1f2b60;">
                                <!-- Form Fields -->
                                <label for="txtEmployeeName">Employee Name:</label>
                                <asp:TextBox ID="txtEmployeeName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>

                                <label for="txtServiceType">Service Type:</label>
                                <asp:TextBox ID="txtServiceType" runat="server" Text="Refresh" CssClass="form-control" ReadOnly="true"></asp:TextBox>

                                <label for="txtDepartment">Department:</label>
                                <asp:TextBox ID="txtDepartment" runat="server" Text="Refresh" CssClass="form-control" ReadOnly="true"></asp:TextBox>

                                <label for="txtLocalRefreshmentFromDate">From Date:</label>
                                <asp:TextBox ID="txtLocalRefreshmentFromDate" runat="server" CssClass="form-control" placeholder="MM/DD/YYYY" TextMode="Date"></asp:TextBox>

                                <label for="txtLocalRefreshmentToDate">To Date:</label>
                                <asp:TextBox ID="txtLocalRefreshmentToDate" runat="server" CssClass="form-control" placeholder="MM/DD/YYYY" TextMode="Date"></asp:TextBox>

                                <label for="txtLocalRefreshmentAmount">Refreshment Amount:</label>
                                <asp:TextBox ID="txtLocalRefreshmentAmount" runat="server" CssClass="form-control" placeholder="Enter amount"></asp:TextBox>

                                <label for="fileUploadRefBill">Upload Approval:</label>
                                <asp:FileUpload ID="fileUploadRefBill" runat="server" CssClass="form-control" />

                                <asp:Label ID="lblValidationMessage" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                                <asp:Label ID="lblSuccessMessage" runat="server" ForeColor="Green" Visible="false"></asp:Label>

                                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="custom-button" OnClick="btnSave_Click" />
                            </section>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    </main>
</asp:Content>