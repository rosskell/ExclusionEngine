<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ExclusionEngine.Web._Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="row">
            <div>
                <h2>Customer Entry</h2>
                <asp:Button ID="UserAdminButton" runat="server" Text="User Admin" PostBackUrl="~/AdminUsers.aspx" CssClass="btn secondary" Visible="false" CausesValidation="false" UseSubmitBehavior="false" />
                <asp:Button ID="ClientAdminButton" runat="server" Text="Client Admin" PostBackUrl="~/ClientAdmin.aspx" CssClass="btn secondary" Visible="false" CausesValidation="false" UseSubmitBehavior="false" />
            </div>
            <asp:Button ID="LogoutButton" runat="server" Text="Log out" OnClick="LogoutButton_Click" CssClass="btn secondary" CausesValidation="false" UseSubmitBehavior="false" />
        </div>
        <asp:Label ID="MessageLabel" runat="server" />
        <div class="form-grid">
            <label>Client</label>
            <asp:DropDownList ID="ClientDropDown" runat="server" />
            <label>Customer Number</label><asp:TextBox ID="CustomerNumberTextBox" runat="server" />
            <label>First Name</label><asp:TextBox ID="FirstNameTextBox" runat="server" />
            <label>Last Name</label><asp:TextBox ID="LastNameTextBox" runat="server" />
            <label>Address 1</label><asp:TextBox ID="Address1TextBox" runat="server" />
            <label>Address 2</label><asp:TextBox ID="Address2TextBox" runat="server" />
            <label>City</label><asp:TextBox ID="CityTextBox" runat="server" />
            <label>State</label><asp:TextBox ID="StateTextBox" runat="server" MaxLength="2" />
            <label>Zip</label><asp:TextBox ID="ZipTextBox" runat="server" MaxLength="10" />
            <label>Email</label><asp:TextBox ID="EmailTextBox" runat="server" TextMode="Email" />
            <asp:HiddenField ID="ConfirmedStandardized" runat="server" Value="false" ClientIDMode="Static" />
            <asp:HiddenField ID="UseOriginalAddress" runat="server" Value="false" ClientIDMode="Static" />
            <asp:HiddenField ID="EditingEntryId" runat="server" Value="" />
            <asp:Button ID="ValidateAddressButton" runat="server" Text="Validate + Save" CssClass="btn" OnClick="ValidateAddressButton_Click" ClientIDMode="Static" />
            <asp:Button ID="CancelEditButton" runat="server" Text="Cancel Edit" CssClass="btn secondary" OnClick="CancelEditButton_Click" Visible="false" CausesValidation="false" UseSubmitBehavior="false" />
        </div>
    </div>

    <div id="confirmModal" class="modal hidden">
        <div class="modal-content">
            <h3>Confirm Address Changes</h3>
            <p><strong>Entered:</strong> <span id="enteredAddress"></span></p>
            <p><strong>Standardized (CASS):</strong> <span id="cassAddress"></span></p>
            <button type="button" class="btn" onclick="acceptCassChanges()">Accept Standardized & Save</button>
            <button type="button" class="btn secondary" onclick="keepOriginalAndSave()">Keep Original & Save</button>
        </div>
    </div>

    <asp:Literal ID="ModalScriptLiteral" runat="server" />

    <div class="card">
        <h3>Recent Entries (Authorized Clients Only)</h3>
        <asp:GridView ID="RecentGrid" runat="server" CssClass="grid" AutoGenerateColumns="false" DataKeyNames="EntryId" OnRowCommand="RecentGrid_RowCommand">
            <Columns>
                <asp:ButtonField Text="Edit" ButtonType="Button" CommandName="EditEntry" CausesValidation="false" />
                <asp:ButtonField Text="Delete" ButtonType="Button" CommandName="DeleteEntry" CausesValidation="false" />
                <asp:BoundField HeaderText="Client" DataField="ClientName" />
                <asp:BoundField HeaderText="Customer #" DataField="CustomerNumber" />
                <asp:BoundField HeaderText="Name" DataField="FullName" />
                <asp:BoundField HeaderText="Address" DataField="FormattedAddress" />
                <asp:BoundField HeaderText="Email" DataField="Email" />
                <asp:BoundField HeaderText="Created" DataField="CreatedAt" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
