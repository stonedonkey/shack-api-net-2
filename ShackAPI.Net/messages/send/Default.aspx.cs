using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

public partial class messages_send_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Expires = 0;

        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";
        client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 ( .NET CLR 3.5.30729; .NET4.0E)";

        int version = 1;
        int.TryParse(Request.QueryString["version"], out version);

        string username = "";
        string password = "";
        string subject = "";
        string to = "";
        string body = "";


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

        if (string.IsNullOrEmpty(Request["to"]) == false)
            to = Request["to"];
        else
        {
            Response.Write("error_message_to_missing");
            return;
        }

        if (string.IsNullOrEmpty(Request["subject"]) == false)
            subject = Request["subject"];
        else
        {
            Response.Write("error_message_subject_missing");
            return;
        }

        if (string.IsNullOrEmpty(Request["body"]) == false)
            body = Request["body"];
        else
        {
            Response.Write("error_message_body_missing");
            return;
        }

        // login to the shack news site using credentials
        try
        {
            NameValueCollection c = new NameValueCollection();
            c.Add("email", username);
            c.Add("password", password);
            c.Add("login", "login");
            client.Cookies = cc;
            string urlCookie = "http://www.shacknews.com/";
            Byte[] webResponse = client.UploadValues(urlCookie, "POST", c);
            String result = Encoding.UTF8.GetString(webResponse);

            if (!result.Contains("<li class=\"user light\">"))
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

        // submit shack message
        try
        {
            // first get the users id from the
            //http://www.shacknews.com/api/users/username.json
            //username

            //string userinfo = client.DownloadString(string.Format("http://www.shacknews.com/api/users/{0}.json",username));
            //Match match = Regex.Match(userinfo, "id\":\"(.*?)\"}");
            //string id = string.Empty;
            //if (match.Success)
            //{
            //    id = match.Groups[1].Value.ToString();
            //}

            string id = string.Empty;
            string shackHtml = client.DownloadString("http://www.shacknews.com/messages");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(shackHtml);

            id = doc.DocumentNode.SelectSingleNode("//input[@name='uid']").GetAttributeValue("value", "");

            client.Headers["X-Requested-With"] = "XMLHttpRequest";

            NameValueCollection post = new NameValueCollection();
            post.Add("message", body);
            post.Add("uid", id);
            post.Add("subject", subject);
            post.Add("to", to);
            string urlPost = "http://www.shacknews.com/messages/send";
            Byte[] postResponse = client.UploadValues(urlPost,"POST", post);
            string result = Encoding.UTF8.GetString(postResponse);

            if (version ==2)
                Response.Write("Message Sent!");

        }
        catch (Exception)
        {

            Response.Write("error_communication_send");
            return;
        }

        Response.Write("Message Sent!");

    }

     

}
