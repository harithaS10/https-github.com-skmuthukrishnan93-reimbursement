<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Vivify.Login" %>

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8">
    <meta content="width=device-width, initial-scale=1.0" name="viewport">
    <title>Vivify | Login</title>
    <meta content="" name="description">
    <meta content="" name="keywords">

    <!-- Google Fonts -->
    <link href="https://fonts.gstatic.com" rel="preconnect">
    <link href="https://fonts.googleapis.com/css?family=Open+Sans:300,400,600|Nunito:300,400,600|Poppins:300,400,500,600" rel="stylesheet">

    <!-- Vendor CSS Files -->
    <link href="assets/vendor/bootstrap/css/bootstrap.min.css" rel="stylesheet">
    <link href="assets/vendor/bootstrap-icons/bootstrap-icons.css" rel="stylesheet">
    <link href="assets/css/style.css" rel="stylesheet">

    <style>
        /* Basic reset and body styles */
        html, body {
            height: 100%;
            margin: 0;
            padding: 0;
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }

        body {
            background: url(assets/img/airport3.jpeg) no-repeat center center;
            background-size: cover;
        }

        .home_banner_area {
            display: flex;
            justify-content: center;
            align-items: center;
            flex: 1; /* Take up available space */
            flex-direction: column;
            padding: 0; /* Remove any default padding */
            margin: 0;  /* Remove default margin */
        }

        .logo_area img {
            max-height: 100px;
            width: 100%;
            max-width: 400px;
        }

        /* Reduced height for formarea and less space between fields */
        .formarea {
            width: 90%;
            max-width: 400px;
            background-color: #0292ba;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            padding: 15px; /* Reduced padding to shrink form area */
            display: flex;
            flex-direction: column;
            align-items: center;
            margin-top: 5%; /* Reduced top margin */
        }

        /* Removed space between username and password fields */
        .form-container {
            width: 100%;
            display: flex;
            flex-direction: column;
        }

        .form-container .col-12 {
            margin-bottom: 5px; /* Reduced space between fields */
        }

        .footer {
            text-align: center;
            padding: 10px;
            background: rgba(255, 255, 255, 0.8);
            position: relative;
            bottom: 0;
            width: 100%;
            margin-top: 100px; /* Reduced margin-top for footer */
        }

        .form-label {
            color: black;
            font-weight: bold;
        }

        .form-control {
            height: 40px;
            padding: 10px;
        }

        @media (max-width: 576px) {
            .formarea {
                margin: 5% auto;
                padding: 12px; /* Reduced padding for mobile */
                width: 95%;
            }

            .logo_area img {
                max-height: 80px;
            }

            .footer {
                font-size: 0.85rem;
            }
        }

        @media (max-width: 768px) {
            .formarea {
                margin-top: 20px;
            }

            .form-control {
                height: 45px; /* Adjusted input height slightly */
            }
        }

    </style>
</head>

<body>
    <main>
        <div class="home_banner_area">
            <div class="logo_area">
                <a class="navbar-brand logo_h" href="#"><img src="assets/img/lgimg.png" alt="Logo"></a>
            </div>

            <div class="formarea">
                <div class="form-container">
                    <h5 class="text-center pb-3" style="padding-top:4px; font-weight:bolder; color:black">Login to Your Account </h5>

                    <form class="row g-3" runat="server">
                        <div class="col-12">
                            <label for="txtUsername" class="form-label">Username</label>
                            <asp:TextBox ID="txtUsername" runat="server" class="form-control"></asp:TextBox>
                            <asp:RequiredFieldValidator runat="server" ID="reqName" ControlToValidate="txtUsername" ForeColor="OrangeRed" ErrorMessage="Please enter your username" />
                            <asp:RegularExpressionValidator 
                                runat="server" 
                                ControlToValidate="txtUsername" 
                                CssClass="required-field" 
                                ErrorMessage="Invalid email format!" 
                                ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$">
                            </asp:RegularExpressionValidator>
                               <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="false" />
                        </div>

                     

                        <div class="col-12">
                            <label for="txtPassword" class="form-label">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" class="form-control"></asp:TextBox>
                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" ControlToValidate="txtPassword" ForeColor="OrangeRed" ErrorMessage="Please enter your password" />
                        </div>

                        <div class="col-12">
                            <asp:Button ID="btnLogin1" runat="server" class="btn btn-primary w-100" OnClick="btnLogin1_Click" Text="Login" style="background-color:#3f418d;" />
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </main>

    <footer class="footer">
        <p>&copy; 2024 Vivify Technocrats. All rights reserved.</p>
    </footer>

    <script src="assets/vendor/bootstrap/js/bootstrap.bundle.min.js"></script>
    <script src="assets/js/main.js"></script>
</body>

</html>
