﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Vivify.ChangePassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Change Password</title>
    <style>
        body {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            background-color: #f2f2f2; /* Background color for the page */
            margin: 0; /* Remove default margin */
        }
        .container {
            background-color: #ffffff; /* White background for the panel */
            padding: 20px;
            border-radius: 5px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
            width: 400px; /* Set a smaller width for the container */
            border: 2px solid #2e57ad; /* Border color for the panel */
        }
        h2 {
            text-align: center;
            color: #2e57ad; /* Title color */
        }
        .container{
             background-color: #0292ba;
        }
        .container input {
            width: 90%; /* Full width for inputs */
            padding: 10px;
            margin: 10px 10px; /* Space between fields */
            border: 1px solid #ccc; /* Default border color */
            border-radius: 5px;
            background-color: #e7f3fe; /* Light blue background for input fields */
        }
        .container input:focus {
            border-color: #1e3a7c; /* Change border color on focus */
            outline: none;
            background-color: #d0e8ff; /* Slightly darker blue on focus */
        }
        .message {
            color: red;
            text-align: center; /* Center the error message */
            margin-top: 10px;
        }
        .button {
            width: 100px; /* Full width for button */
            padding: 10px;
            margin-top: 15px; /* Space above the button */
            border-radius: 5px;
            font-size: 16px; /* Font size */
            background-color: #3f418d; /* Button background color */
            color: white; /* Set text color to white */
           /* border: none;*/ /* Remove border */
            cursor: pointer; /* Pointer cursor on hover */
            align-content:center;
           /* transition: background-color 0.3s ease;*/ /* Smooth transition for hover effect */
        }
        .button:hover {
            background-color: #3f418d;  /*shade of the background color on hover */
        }

        .password-container {
            position: relative;
        }
        .toggle-password {
            position: absolute;
            right: 10px;
            top: 10px;
            cursor: pointer;
            color: #2e57ad; /* Icon color */
        }

        body {
            background: url(assets/img/airport3.jpeg) no-repeat center center;
            background-size: cover;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2 style="color:black">Change Password</h2>
            <asp:Label ID="lblCurrentPassword" runat="server" Text="Current Password:" AssociatedControlID="txtCurrentPassword" />
            <asp:TextBox ID="txtCurrentPassword" runat="server" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvCurrentPassword" runat="server" ControlToValidate="txtCurrentPassword" 
                ErrorMessage="Current Password is required." CssClass="message" Display="Dynamic" />

            <asp:Label ID="lblNewPassword" runat="server" Text="New Password:" AssociatedControlID="txtNewPassword" />
            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" ControlToValidate="txtNewPassword" 
                ErrorMessage="New Password is required." CssClass="message" Display="Dynamic" />

            <asp:Label ID="lblConfirmPassword" runat="server" Text="Confirm New Password:" AssociatedControlID="txtConfirmPassword" />
            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" />
            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" 
                ErrorMessage="Confirm Password is required." CssClass="message" Display="Dynamic" />

            <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" OnClick="btnChangePassword_Click" CssClass="button"  style=" background-color: #3f418d; width:97%; "/>
            <asp:Label ID="lblMessage" runat="server" CssClass="message" />
        </div>
    </form>
</body>
</html>

<script>
    function togglePassword(inputId, icon) {
        var input = document.getElementById(inputId);
        if (input.type === "password") {
            input.type = "text";
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            input.type = "password";
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    }
</script>
