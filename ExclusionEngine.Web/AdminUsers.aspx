<%@ Page Title="Admin Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminUsers.aspx.cs" Inherits="ExclusionEngine.Web.AdminUsers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <h2>User Administration</h2>
        <asp:Label ID="AdminMessageLabel" runat="server" />

        <asp:HiddenField ID="EditingUserId" runat="server" />
        <div class="form-grid">
            <label>Username</label><asp:TextBox ID="AdminUsernameTextBox" runat="server" />
            <label>Email</label><asp:TextBox ID="AdminEmailTextBox" runat="server" TextMode="Email" />
            <label>Password</label><asp:TextBox ID="AdminPasswordTextBox" runat="server" TextMode="Password" />
            <label>Is Admin</label><asp:CheckBox ID="IsAdminCheckBox" runat="server" />
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
                <asp:BoundField HeaderText="Username" DataField="Username" />
                <asp:BoundField HeaderText="Email" DataField="Email" />
                <asp:CheckBoxField HeaderText="Admin" DataField="IsAdmin" />
                <asp:BoundField HeaderText="Clients" DataField="ClientCodes" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
