using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

namespace ExclusionEngine.Web
{
    public partial class CustomerData : System.Web.UI.Page
    {
        private int UserId => Convert.ToInt32(Session["UserId"]);

        private string CurrentSortExpression
        {
            get => ViewState["SortExpression"] as string ?? "CreatedAt";
            set => ViewState["SortExpression"] = value;
        }

        private string CurrentSortDirection
        {
            get => ViewState["SortDirection"] as string ?? "DESC";
            set => ViewState["SortDirection"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindClientFilter();
                BindGrid();
            }
        }

        public string FormatCreatedAtForBrowser(object createdAt)
        {
            if (createdAt == null || createdAt == DBNull.Value)
            {
                return string.Empty;
            }

            var utc = Convert.ToDateTime(createdAt);
            if (utc.Kind == DateTimeKind.Unspecified)
            {
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            }
            else
            {
                utc = utc.ToUniversalTime();
            }

            var isoUtc = utc.ToString("o");
            return "<span class=\"created-at-local\" data-utc-created=\"" + HttpUtility.HtmlAttributeEncode(isoUtc) + "\">" + HttpUtility.HtmlEncode(utc.ToString("yyyy-MM-dd HH:mm") + " UTC") + "</span>";
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            CustomerGrid.PageIndex = 0;
            BindGrid();
        }

        protected void ClearSearchButton_Click(object sender, EventArgs e)
        {
            SearchLastNameTextBox.Text = string.Empty;
            SearchAddress1TextBox.Text = string.Empty;
            ClientFilterDropDown.SelectedValue = "0";
            CustomerGrid.PageIndex = 0;
            BindGrid();
        }

        protected void ClientFilterDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            CustomerGrid.PageIndex = 0;
            BindGrid();
        }

        protected void PageSizeDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            CustomerGrid.PageSize = GetPageSize();
            CustomerGrid.PageIndex = 0;
            BindGrid();
        }

        protected void CustomerGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            CustomerGrid.PageSize = GetPageSize();
            CustomerGrid.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void CustomerGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (CurrentSortExpression.Equals(e.SortExpression, StringComparison.OrdinalIgnoreCase))
            {
                CurrentSortDirection = CurrentSortDirection == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                CurrentSortExpression = e.SortExpression;
                CurrentSortDirection = "ASC";
            }

            CustomerGrid.PageIndex = 0;
            BindGrid();
        }

        protected void CustomerGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var rowIndex = Convert.ToInt32(e.CommandArgument);
            var entryId = Convert.ToInt32(CustomerGrid.DataKeys[rowIndex].Value);

            if (e.CommandName == "EditEntry")
            {
                Response.Redirect("~/Default.aspx?editId=" + entryId);
                return;
            }

            if (e.CommandName == "DeleteEntry")
            {
                try
                {
                    Repository.DeleteEntry(UserId, entryId);
                    MessageLabel.Text = "<span class='success'>Customer entry deleted.</span>";
                    BindGrid();
                }
                catch (Exception ex)
                {
                    MessageLabel.Text = $"<span class='error'>{HttpUtility.HtmlEncode(ex.Message)}</span>";
                }
            }
        }

        private void BindClientFilter()
        {
            var clients = Repository.GetClientsForUser(UserId);
            ClientFilterDropDown.DataSource = clients;
            ClientFilterDropDown.DataTextField = "ClientDisplay";
            ClientFilterDropDown.DataValueField = "ClientId";
            ClientFilterDropDown.DataBind();
            ClientFilterDropDown.Items.Insert(0, new ListItem("All Clients", "0"));
            ClientFilterDropDown.Visible = clients.Count > 1;
        }

        private void BindGrid()
        {
            var selectedClientId = 0;
            int.TryParse(ClientFilterDropDown.SelectedValue, out selectedClientId);
            var dt = Repository.GetCustomerDataForUser(
                UserId,
                selectedClientId > 0 ? (int?)selectedClientId : null,
                SearchLastNameTextBox.Text.Trim(),
                SearchAddress1TextBox.Text.Trim());

            var dv = dt.DefaultView;
            dv.Sort = CurrentSortExpression + " " + CurrentSortDirection;
            CustomerGrid.PageSize = GetPageSize();
            CustomerGrid.DataSource = dv;
            CustomerGrid.DataBind();
        }

        private int GetPageSize()
        {
            if (int.TryParse(PageSizeDropDown.SelectedValue, out var pageSize) && pageSize > 0)
            {
                return pageSize;
            }

            return 20;
        }
    }
}
