using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Error : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/html";

        Exception ex = (Exception)Application["ex"];

        if (ex != null)
            this.LabelMessage.Text = ex.Message;






    }
}
