<%@ Page Title="Reset Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="ExclusionEngine.Web.ResetPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <h2>Reset Password</h2>
        <asp:Label ID="ResetMessage" runat="server" />
        <div class="form-grid">
            <label>New Password</label>
            <asp:TextBox ID="NewPasswordTextBox" runat="server" TextMode="Password" />
            <asp:Button ID="ResetPasswordButton" runat="server" Text="Reset Password" CssClass="btn" OnClick="ResetPasswordButton_Click" />
        </div>
    </div>
</asp:Content>
