<%@ Page Title="Client Admin" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ClientAdmin.aspx.cs" Inherits="ExclusionEngine.Web.ClientAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="row">
            <h2>Client Admin</h2>
            <asp:Button ID="BackButton" runat="server" Text="Back to Dashboard" CssClass="btn secondary" OnClick="BackButton_Click" CausesValidation="false" />
        </div>
        <asp:Label ID="ClientMessageLabel" runat="server" />
        <asp:HiddenField ID="EditingClientId" runat="server" />
        <div class="form-grid">
            <label>Client Code</label><asp:TextBox ID="ClientCodeTextBox" runat="server" />
            <label>Client Name</label><asp:TextBox ID="ClientNameTextBox" runat="server" />
            <label>Active</label><asp:CheckBox ID="IsActiveCheckBox" runat="server" Checked="true" />
            <asp:Button ID="SaveClientButton" runat="server" Text="Save Client" CssClass="btn" OnClick="SaveClientButton_Click" />
            <asp:Button ID="CancelClientEditButton" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="CancelClientEditButton_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="card">
        <h3>Clients</h3>
        <div class="action-row">
            <label>Client Code</label>
            <asp:TextBox ID="SearchClientCodeTextBox" runat="server" CssClass="search-input" />
            <label>Client Name</label>
            <asp:TextBox ID="SearchClientNameTextBox" runat="server" CssClass="search-input" />
            <label>Status</label>
            <asp:DropDownList ID="ClientStatusFilterDropDown" runat="server">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Active" Value="1" />
                <asp:ListItem Text="Inactive" Value="0" />
            </asp:DropDownList>
            <asp:Button ID="SearchClientsButton" runat="server" Text="Search" CssClass="btn" OnClick="SearchClientsButton_Click" CausesValidation="false" />
            <asp:Button ID="ClearClientSearchButton" runat="server" Text="Clear" CssClass="btn secondary" OnClick="ClearClientSearchButton_Click" CausesValidation="false" />
        </div>
        <div class="grid-wrap">
            <asp:GridView ID="ClientsGrid" runat="server" AutoGenerateColumns="false" DataKeyNames="ClientId" OnRowCommand="ClientsGrid_RowCommand" CssClass="grid">
                <Columns>
                    <asp:TemplateField HeaderText="Actions" ItemStyle-CssClass="col-actions">
                        <ItemTemplate>
                            <asp:Button ID="EditClientButton" runat="server" Text="Edit" CommandName="EditClient"
                                CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false" />
                            <asp:Button ID="DeactivateClientButton" runat="server" Text="Deactivate" CommandName="DeleteClient"
                                CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false"
                                OnClientClick="return confirm('Deactivate this client? Existing customer records will be kept.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Client Code" DataField="ClientCode" />
                    <asp:BoundField HeaderText="Client Name" DataField="ClientName" />
                    <asp:CheckBoxField HeaderText="Active" DataField="IsActive" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
