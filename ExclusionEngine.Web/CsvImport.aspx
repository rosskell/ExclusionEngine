<%@ Page Title="CSV Import" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CsvImport.aspx.cs" Inherits="ExclusionEngine.Web.CsvImport" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ==================== STEP 1: UPLOAD ==================== --%>
    <asp:Panel ID="UploadPanel" runat="server">
        <div class="card">
            <div class="row">
                <h2>CSV Import</h2>
                <div class="top-actions">
                    <asp:Button ID="BackToEntryButton" runat="server" Text="Back to Entry" PostBackUrl="~/Default.aspx" CssClass="btn secondary" CausesValidation="false" UseSubmitBehavior="false" />
                </div>
            </div>
            <asp:Label ID="MessageLabel" runat="server" />
            <div class="form-grid">
                <label>Client</label>
                <asp:DropDownList ID="ClientDropDown" runat="server" />
                <label>CSV File</label>
                <asp:FileUpload ID="CsvFileUpload" runat="server" accept=".csv" />
            </div>
            <p style="margin-top:12px;font-size:13px;color:#555;">
                A header row is required. Column names are case-insensitive and common variations are accepted.<br />
                <strong>Required:</strong> Address1, City, State &nbsp;|&nbsp;
                <strong>Optional:</strong> CustomerNumber, FirstName, LastName, Address2, Zip, Email, Phone, Notes
            </p>
            <asp:Button ID="PreviewButton" runat="server" Text="Preview CSV" CssClass="btn" OnClick="PreviewButton_Click" />
        </div>
    </asp:Panel>

    <%-- ==================== STEP 2: PREVIEW & EDIT ==================== --%>
    <asp:Panel ID="PreviewPanel" runat="server" Visible="false">
        <div class="card">
            <div class="row">
                <h2>Preview &amp; Edit</h2>
                <div class="top-actions">
                    <asp:Button ID="ImportSelectedTopButton" runat="server" Text="Import Selected" CssClass="btn"
                        OnClick="ImportSelectedButton_Click" OnClientClick="return confirmImportSelected();" />
                    <asp:Button ID="StartOverButton" runat="server" Text="Start Over" CssClass="btn secondary"
                        OnClick="StartOverButton_Click" CausesValidation="false" UseSubmitBehavior="false" />
                </div>
            </div>
            <asp:Label ID="PreviewMessageLabel" runat="server" />
            <p style="font-size:13px;color:#555;margin:4px 0 12px;">
                Edit any field before importing. Uncheck a row to skip it. All addresses will be run through CASS standardization on import.
            </p>
            <div class="grid-wrap csv-preview-wrap">
                <asp:GridView ID="PreviewGrid" runat="server" CssClass="grid csv-preview-grid"
                    AutoGenerateColumns="false" DataKeyNames="RowNum">
                    <Columns>
                        <asp:TemplateField ItemStyle-CssClass="col-select" HeaderStyle-CssClass="col-select">
                            <HeaderTemplate>
                                <input type="checkbox" onclick="toggleSelectAll(this)" checked="checked" title="Select / deselect all" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SelectRowCheckBox" runat="server" Checked="true" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Row" ItemStyle-CssClass="csv-col-rownum">
                            <ItemTemplate>
                                <asp:Label ID="RowNumLabel" runat="server" Text='<%# Eval("RowNum") %>'
                                    style="font-size:12px;color:#94a3b8;white-space:nowrap;" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Cust #" ItemStyle-CssClass="csv-col-sm">
                            <ItemTemplate>
                                <asp:TextBox ID="CustomerNumberBox" runat="server" Text='<%# Eval("CustomerNumber") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="First Name" ItemStyle-CssClass="csv-col-name">
                            <ItemTemplate>
                                <asp:TextBox ID="FirstNameBox" runat="server" Text='<%# Eval("FirstName") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Last Name" ItemStyle-CssClass="csv-col-name">
                            <ItemTemplate>
                                <asp:TextBox ID="LastNameBox" runat="server" Text='<%# Eval("LastName") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Address 1" ItemStyle-CssClass="csv-col-addr">
                            <ItemTemplate>
                                <asp:TextBox ID="Address1Box" runat="server" Text='<%# Eval("Address1") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Address 2" ItemStyle-CssClass="csv-col-name">
                            <ItemTemplate>
                                <asp:TextBox ID="Address2Box" runat="server" Text='<%# Eval("Address2") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="City" ItemStyle-CssClass="csv-col-name">
                            <ItemTemplate>
                                <asp:TextBox ID="CityBox" runat="server" Text='<%# Eval("City") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="St" ItemStyle-CssClass="csv-col-state">
                            <ItemTemplate>
                                <asp:TextBox ID="StateBox" runat="server" Text='<%# Eval("State") %>' MaxLength="2" CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Zip" ItemStyle-CssClass="csv-col-sm">
                            <ItemTemplate>
                                <asp:TextBox ID="ZipBox" runat="server" Text='<%# Eval("Zip") %>' MaxLength="10" CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Email" ItemStyle-CssClass="csv-col-addr">
                            <ItemTemplate>
                                <asp:TextBox ID="EmailBox" runat="server" Text='<%# Eval("Email") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Phone" ItemStyle-CssClass="csv-col-sm">
                            <ItemTemplate>
                                <asp:TextBox ID="PhoneBox" runat="server" Text='<%# Eval("Phone") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="csv-col-addr">
                            <ItemTemplate>
                                <asp:TextBox ID="NotesBox" runat="server" Text='<%# Eval("Notes") %>' CssClass="csv-cell" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="action-row" style="margin-top:12px;">
                <asp:Button ID="ImportSelectedBottomButton" runat="server" Text="Import Selected" CssClass="btn"
                    OnClick="ImportSelectedButton_Click" OnClientClick="return confirmImportSelected();" />
                <asp:Button ID="StartOverBottomButton" runat="server" Text="Start Over" CssClass="btn secondary"
                    OnClick="StartOverButton_Click" CausesValidation="false" UseSubmitBehavior="false" />
            </div>
        </div>
    </asp:Panel>

    <%-- ==================== STEP 3: RESULTS ==================== --%>
    <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
        <div class="card">
            <div class="row">
                <h2>Import Results</h2>
                <div class="top-actions">
                    <asp:Button ID="ImportMoreButton" runat="server" Text="Import More" CssClass="btn secondary"
                        OnClick="ImportMoreButton_Click" CausesValidation="false" UseSubmitBehavior="false" />
                    <asp:Button ID="BackToEntryFromResultsButton" runat="server" Text="Back to Entry"
                        PostBackUrl="~/Default.aspx" CssClass="btn secondary" CausesValidation="false" UseSubmitBehavior="false" />
                </div>
            </div>
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

    <script type="text/javascript">
    function confirmImportSelected() {
        var count = document.querySelectorAll('.csv-preview-grid tbody input[type=checkbox]:checked').length;
        if (count === 0) {
            alert('Please select at least one row to import.');
            return false;
        }
        return confirm('Import ' + count + ' selected row' + (count === 1 ? '' : 's') + '?');
    }
    </script>

</asp:Content>
