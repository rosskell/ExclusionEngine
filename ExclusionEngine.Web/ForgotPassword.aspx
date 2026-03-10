<%@ Page Title="Forgot Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="ExclusionEngine.Web.ForgotPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="row">
            <h2>Forgot Password</h2>
            <div class="top-actions">
                <asp:Button ID="BackToLoginButton" runat="server" Text="Back to Login" PostBackUrl="~/Login.aspx" CssClass="btn secondary" CausesValidation="false" UseSubmitBehavior="false" />
            </div>
        </div>
        <asp:Label ID="ForgotPasswordMessage" runat="server" />
        <div class="form-grid">
            <label>Email</label>
            <asp:TextBox ID="EmailTextBox" runat="server" TextMode="Email" />
            <asp:Button ID="RequestResetButton" runat="server" Text="Send Reset Link" CssClass="btn" OnClick="RequestResetButton_Click" />
        </div>
    </div>
</asp:Content>
