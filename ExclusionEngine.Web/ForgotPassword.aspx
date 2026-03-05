<%@ Page Title="Forgot Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="ExclusionEngine.Web.ForgotPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <h2>Forgot Password</h2>
        <asp:Label ID="ForgotPasswordMessage" runat="server" />
        <div class="form-grid">
            <label>Email</label>
            <asp:TextBox ID="EmailTextBox" runat="server" TextMode="Email" />
            <asp:Button ID="RequestResetButton" runat="server" Text="Send Reset Link" CssClass="btn" OnClick="RequestResetButton_Click" />
        </div>
    </div>
</asp:Content>
