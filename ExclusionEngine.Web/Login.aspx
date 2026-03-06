<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ExclusionEngine.Web.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <h2>Client Login</h2>
        <asp:Label ID="ErrorMessage" runat="server" CssClass="error" />
        <div class="form-grid">
            <label>Username</label>
            <asp:TextBox ID="UsernameTextBox" runat="server" />
            <label>Password</label>
            <asp:TextBox ID="PasswordTextBox" runat="server" TextMode="Password" />
            <asp:Button ID="LoginButton" runat="server" Text="Sign in" OnClick="LoginButton_Click" CssClass="btn" />
        </div>
        <p><asp:HyperLink ID="ForgotPasswordLink" runat="server" NavigateUrl="~/ForgotPassword.aspx">Forgot password?</asp:HyperLink></p>
    </div>
</asp:Content>
