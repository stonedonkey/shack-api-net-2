using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Text;
using System.Net;

public partial class messages_read_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Expires = 0;

        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";
        client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 ( .NET CLR 3.5.30729; .NET4.0E)";
        client.Headers["X-Requested-With"] = "XMLHttpRequest";

        string username = "";
        string password = "";
        string messageid = "";

        //if (string.IsNullOrEmpty(Request["username"]) == false)
        //    username = Request["username"];
        //else
        //{
        //    Response.Write("error_username_missing");
        //    return;
        //}

        //if (string.IsNullOrEmpty(Request["password"]) == false)
        //    password = Request["password"];
        //else
        //{
        //    Response.Write("error_password_missing");
        //    return;
        //}

        // try and pull credentials off the auth header
        string headers = Context.Request.Headers["Authorization"];
        if (headers.Length > 7)
        {
            string ticket = headers.Substring(6);
            string clearTicket = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ticket));

            password = clearTicket.Substring(clearTicket.LastIndexOf(":") + 1);
            username = clearTicket.Substring(0, clearTicket.LastIndexOf(":"));

            if (username.Contains("\\") && !username.EndsWith("\\"))
                username = username.Substring(username.LastIndexOf("\\") + 1);
        }


        if (string.IsNullOrEmpty(Request["messageid"]) == false)
            messageid = Request["messageid"];
        else
        {
            Response.Write("messageid_missing");
            return;
        }


        // login to the shack news site using credentials
        try
        {
            NameValueCollection c = new NameValueCollection();
            //c.Add("email", _Username);
            //c.Add("password", password);
            //c.Add("login", "login");
            c.Add("user-identifier", username);
            c.Add("supplied-pass", password);
            c.Add("get_fields[]", "result");
            c.Add("remember-login", "1");

            client.Cookies = cc;
            string urlCookie = "http://www.shacknews.com/account/signin";
            Byte[] webResponse = client.UploadValues(urlCookie, "POST", c);
            String result = Encoding.UTF8.GetString(webResponse);

            if (!result.Contains("{\"status\":\"OK\""))
            {
                Response.Write("error_login_failed");
                return;
            }
        }
        catch (Exception)
        {
            Response.Write("error_communication_authentication");
            return;
        }

        String URL = "http://www.shacknews.com/messages/read";

        try
        {
            NameValueCollection m = new NameValueCollection();
            m.Add("mid", messageid);

            client.Headers.Remove("X-Requested-With");
            // load the HTML from the shack
            Byte[] postResponse = client.UploadValues(URL, "POST", m);
            string result = Encoding.UTF8.GetString(postResponse);
            Response.Write("ok");

        }
        catch (Exception)
        {
            Response.Write("error_communication_inbox");
            return;
        }

    }
}
