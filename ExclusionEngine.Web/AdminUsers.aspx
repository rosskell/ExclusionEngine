<%@ Page Title="User Admin" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminUsers.aspx.cs" Inherits="ExclusionEngine.Web.AdminUsers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="row">
            <h2>User Admin</h2>
            <asp:Button ID="BackToDefaultButton" runat="server" Text="Back to Dashboard" CssClass="btn secondary" OnClick="BackToDefaultButton_Click" CausesValidation="false" />
        </div>
        <asp:Label ID="AdminMessageLabel" runat="server" />

        <asp:HiddenField ID="EditingUserId" runat="server" />
        <div class="form-grid">
            <label>Username</label><asp:TextBox ID="AdminUsernameTextBox" runat="server" />
            <label>Email</label><asp:TextBox ID="AdminEmailTextBox" runat="server" TextMode="Email" />
            <label>Password</label><asp:TextBox ID="AdminPasswordTextBox" runat="server" TextMode="Password" />
            <label>Is Admin</label><asp:CheckBox ID="IsAdminCheckBox" runat="server" AutoPostBack="true" OnCheckedChanged="IsAdminCheckBox_CheckedChanged" />
            <label>Is Disabled</label><asp:CheckBox ID="IsDisabledCheckBox" runat="server" />
            <label>Allowed Clients</label>
            <asp:CheckBoxList ID="ClientCheckBoxList" runat="server" RepeatColumns="2" />

            <asp:Button ID="SaveUserButton" runat="server" Text="Save User" CssClass="btn" OnClick="SaveUserButton_Click" />
            <asp:Button ID="CancelUserEditButton" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="CancelUserEditButton_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="card">
        <h3>Users</h3>
        <asp:GridView ID="UsersGrid" runat="server" AutoGenerateColumns="false" DataKeyNames="UserId" OnRowCommand="UsersGrid_RowCommand" CssClass="grid">
            <Columns>
                <asp:ButtonField Text="Edit" ButtonType="Button" CommandName="EditUser" CausesValidation="false" />
                <asp:ButtonField Text="Disable/Enable" ButtonType="Button" CommandName="ToggleDisable" CausesValidation="false" />
                <asp:ButtonField Text="Delete" ButtonType="Button" CommandName="DeleteUser" CausesValidation="false" />
                <asp:BoundField HeaderText="Username" DataField="Username" />
                <asp:BoundField HeaderText="Email" DataField="Email" />
                <asp:CheckBoxField HeaderText="Admin" DataField="IsAdmin" />
                <asp:CheckBoxField HeaderText="Disabled" DataField="IsDisabled" />
                <asp:BoundField HeaderText="Clients" DataField="ClientCodes" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
