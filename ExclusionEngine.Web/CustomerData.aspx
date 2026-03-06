<%@ Page Title="Customer Data" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CustomerData.aspx.cs" Inherits="ExclusionEngine.Web.CustomerData" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card">
        <div class="row">
            <h2>Customer Data</h2>
            <div class="top-actions">
                <asp:Button ID="BackToEntryButton" runat="server" Text="Back to Entry" PostBackUrl="~/Default.aspx" CssClass="btn secondary" CausesValidation="false" UseSubmitBehavior="false" />
            </div>
        </div>
        <asp:Label ID="MessageLabel" runat="server" />
        <div class="search-row">
            <label>Client</label>
            <asp:DropDownList ID="ClientFilterDropDown" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ClientFilterDropDown_SelectedIndexChanged" />
            <label>Search Last Name</label><asp:TextBox ID="SearchLastNameTextBox" runat="server" />
            <label>Search Address 1</label><asp:TextBox ID="SearchAddress1TextBox" runat="server" />
            <label>Page Size</label>
            <asp:DropDownList ID="PageSizeDropDown" runat="server" AutoPostBack="true" OnSelectedIndexChanged="PageSizeDropDown_SelectedIndexChanged">
                <asp:ListItem Text="20" Value="20" />
                <asp:ListItem Text="50" Value="50" />
                <asp:ListItem Text="100" Value="100" />
                <asp:ListItem Text="250" Value="250" />
            </asp:DropDownList>
            <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn" OnClick="SearchButton_Click" CausesValidation="false" UseSubmitBehavior="false" />
            <asp:Button ID="ClearSearchButton" runat="server" Text="Clear" CssClass="btn secondary" OnClick="ClearSearchButton_Click" CausesValidation="false" UseSubmitBehavior="false" />
        </div>

        <div class="grid-wrap">
        <asp:GridView ID="CustomerGrid" runat="server" CssClass="grid wide-grid" AutoGenerateColumns="false" DataKeyNames="EntryId"
            AllowPaging="true" AllowSorting="true" PageSize="20"
            OnPageIndexChanging="CustomerGrid_PageIndexChanging" OnSorting="CustomerGrid_Sorting" OnRowCommand="CustomerGrid_RowCommand">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="EditEntryButton" runat="server" Text="Edit" CommandName="EditEntry"
                            CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="DeleteEntryButton" runat="server" Text="Delete" CommandName="DeleteEntry"
                            CommandArgument="<%# ((GridViewRow)Container).RowIndex %>" CausesValidation="false"
                            OnClientClick="return confirm('Delete this customer entry?');" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField HeaderText="Client" DataField="ClientName" />
                <asp:BoundField HeaderText="Customer Number" DataField="CustomerNumber" />
                <asp:BoundField HeaderText="First Name" DataField="FirstName" />
                <asp:BoundField HeaderText="Last Name" DataField="LastName" SortExpression="LastName" />
                <asp:BoundField HeaderText="Address1" DataField="Address1" />
                <asp:BoundField HeaderText="Address2" DataField="Address2" />
                <asp:BoundField HeaderText="City" DataField="City" />
                <asp:BoundField HeaderText="State" DataField="State" SortExpression="State" />
                <asp:BoundField HeaderText="Zip" DataField="Zip" SortExpression="Zip" />
                <asp:BoundField HeaderText="Zip4" DataField="Zip4" />
                <asp:BoundField HeaderText="DPB" DataField="DeliveryPointBarcode" />
                <asp:BoundField HeaderText="Email" DataField="Email" />
                <asp:TemplateField HeaderText="Created At">
                    <ItemTemplate>
                        <asp:Literal ID="CreatedAtLiteral" runat="server" Text='<%# FormatCreatedAtForBrowser(Eval("CreatedAt")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </div>
    </div>
</asp:Content>
