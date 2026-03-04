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
            <asp:Button ID="SaveClientButton" runat="server" Text="Save Client" CssClass="btn" OnClick="SaveClientButton_Click" />
            <asp:Button ID="CancelClientEditButton" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="CancelClientEditButton_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="card">
        <h3>Clients</h3>
        <asp:GridView ID="ClientsGrid" runat="server" AutoGenerateColumns="false" DataKeyNames="ClientId" OnRowCommand="ClientsGrid_RowCommand" CssClass="grid">
            <Columns>
                <asp:ButtonField Text="Edit" ButtonType="Button" CommandName="EditClient" CausesValidation="false" />
                <asp:ButtonField Text="Delete" ButtonType="Button" CommandName="DeleteClient" CausesValidation="false" />
                <asp:BoundField HeaderText="Client Code" DataField="ClientCode" />
                <asp:BoundField HeaderText="Client Name" DataField="ClientName" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
