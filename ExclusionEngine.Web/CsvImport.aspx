<%@ Page Title="CSV Import" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CsvImport.aspx.cs" Inherits="ExclusionEngine.Web.CsvImport" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="row">
            <h2>CSV Import</h2>
            <div class="top-actions">
                <asp:Button ID="BackButton" runat="server" Text="Back to Entry" PostBackUrl="~/Default.aspx" CssClass="btn secondary" CausesValidation="false" UseSubmitBehavior="false" />
            </div>
        </div>
        <asp:Label ID="MessageLabel" runat="server" />
        <div class="form-grid">
            <label>Client</label>
            <asp:DropDownList ID="ClientDropDown" runat="server" />
            <label>CSV File</label>
            <asp:FileUpload ID="CsvFileUpload" runat="server" accept=".csv" />
        </div>
        <p style="margin-top:10px;color:#555;">
            Expected columns (header row required):<br />
            <code>FirstName, LastName, Address1, Address2, City, State, Zip, CustomerNumber, Email, Phone, Notes</code><br />
            <strong>Address1, City, and State are required.</strong> All other columns are optional. Column names are case-insensitive.
            Addresses will be automatically run through CASS standardization.
        </p>
        <asp:Button ID="ImportButton" runat="server" Text="Import CSV" CssClass="btn" OnClick="ImportButton_Click" />
    </div>

    <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
        <div class="card">
            <h3>Import Results</h3>
            <asp:Label ID="SummaryLabel" runat="server" />
            <div class="grid-wrap">
            <asp:GridView ID="ResultsGrid" runat="server" CssClass="grid" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField HeaderText="Row #" DataField="Row" ItemStyle-CssClass="col-nowrap" />
                    <asp:BoundField HeaderText="Name" DataField="Name" ItemStyle-CssClass="col-nowrap" />
                    <asp:BoundField HeaderText="Address" DataField="Address" />
                    <asp:BoundField HeaderText="Status" DataField="Status" ItemStyle-CssClass="col-nowrap" />
                    <asp:BoundField HeaderText="Notes" DataField="ImportNotes" />
                </Columns>
            </asp:GridView>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
