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
            <label>Company Name</label><asp:TextBox ID="AdminCompanyNameTextBox" runat="server" />
            <div class="check-row"><asp:CheckBox ID="IsAdminCheckBox" runat="server" Text="Is Admin" AutoPostBack="true" OnCheckedChanged="IsAdminCheckBox_CheckedChanged" /></div>
            <div class="check-row"><asp:CheckBox ID="IsDisabledCheckBox" runat="server" Text="Is Disabled" /></div>
            <label>Allowed Clients</label>
            <asp:CheckBoxList ID="ClientCheckBoxList" runat="server" CssClass="client-check-list" RepeatLayout="UnorderedList" />

            <asp:Button ID="SaveUserButton" runat="server" Text="Save User" CssClass="btn" OnClick="SaveUserButton_Click" />
            <asp:Button ID="CancelUserEditButton" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="CancelUserEditButton_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="card">
        <h3>Users</h3>
        <div class="action-row">
            <label>Username</label>
            <asp:TextBox ID="SearchUsernameTextBox" runat="server" CssClass="search-input" />
            <label>Company</label>
            <asp:TextBox ID="SearchCompanyTextBox" runat="server" CssClass="search-input" />
            <label>Status</label>
            <asp:DropDownList ID="StatusFilterDropDown" runat="server">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Enabled" Value="0" />
                <asp:ListItem Text="Disabled" Value="1" />
            </asp:DropDownList>
            <asp:Button ID="SearchUsersButton" runat="server" Text="Search" CssClass="btn" OnClick="SearchUsersButton_Click" CausesValidation="false" />
        </div>
        <div class="grid-wrap">
            <asp:GridView ID="UsersGrid" runat="server" AutoGenerateColumns="false" DataKeyNames="UserId" OnRowCommand="UsersGrid_RowCommand" CssClass="grid">
                <Columns>
                    <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="col-actions">
                        <ItemTemplate>
                            <asp:Button ID="EditUserButton" runat="server" Text="Edit" CommandName="EditUser"
                                CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false" />
                            <asp:Button ID="ToggleDisableButton" runat="server"
                                Text='<%# Convert.ToBoolean(Eval("IsDisabled")) ? "Enable" : "Disable" %>'
                                CommandName="ToggleDisable"
                                CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false" />
                            <asp:Button ID="DeleteUserButton" runat="server" Text="Delete" CommandName="DeleteUser"
                                CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false"
                                OnClientClick="return confirm('Delete this user?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Username" DataField="Username" />
                    <asp:BoundField HeaderText="Email" DataField="Email" />
                    <asp:BoundField HeaderText="Company" DataField="CompanyName" />
                    <asp:CheckBoxField HeaderText="Admin" DataField="IsAdmin" />
                    <asp:CheckBoxField HeaderText="Disabled" DataField="IsDisabled" />
                    <asp:BoundField HeaderText="Clients" DataField="ClientCodes" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
