using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Xml;
using System.IO;

public partial class post_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";
        client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 ( .NET CLR 3.5.30729; .NET4.0E)";

        int version = 1;
        int.TryParse(Request.QueryString["version"], out version);

        string username = "";
        string password = "";
        string parent_id = "";
        string body = "";
        string contentTypeID = "17";

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


        if (!string.IsNullOrEmpty(Request["content_type_id"]))
        {
            contentTypeID = Request["content_type_id"];
        }

        if (string.IsNullOrEmpty(Request["parent_id"]) == false)
            parent_id = Request["parent_id"];
        else
        {
            parent_id = "0";
        }

        if (string.IsNullOrEmpty(Request["body"]) == false)
            body = Request["body"];
        else
        {
            Response.Write("error_post_body_missing");
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

        int tid = 0;
        int.TryParse(parent_id, out tid);

        // do a check for what type of post they are respoding too, if posted by shacknews we're going
        // to force the contentTypeID to 2
        if (tid > 0)
            try
            {
                //25577895
                JavaScriptSerializer js = new JavaScriptSerializer();
                WebClient wc = new WebClient();
                string json = wc.DownloadString(ConfigurationManager.AppSettings["siteURL"] + "thread/" + tid + ".xml");

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(new StringReader(json));

                string starter = xmlDoc.SelectSingleNode("comments/comment").Attributes["author"].Value;

                if (starter.ToLower().Equals("shacknews"))
                    contentTypeID = "2";

            }
            catch(Exception ex)
            {
                string fail = ex.Message;
                // we'll try to post anyways, it should work most times than not.
            }


        // submit shack message
        try
        {

            NameValueCollection post = new NameValueCollection();

            if (tid <= 0)
                post.Add("parent_id", ""); // empty for new post
            else
                post.Add("parent_id", parent_id);

            post.Add("content_type_id", contentTypeID); // 17 is latest chatty
            post.Add("content_id", contentTypeID); // posts to main chatty
            post.Add("page", ""); // don't really need this
            post.Add("parent_url", "/chatty"); // don't really need this either
            post.Add("body", body);

            string urlPost = "http://www.shacknews.com/post_chatty.x";
            Byte[] postResponse = client.UploadValues(urlPost, "POST", post);
            string result = Encoding.UTF8.GetString(postResponse);

            if (result != null && result.Contains("You have been banned from posting"))
                Response.Write("error_account_banned");
            else if (result != null && result.Contains("Please wait a few minutes before trying to post again."))
                Response.Write("error_post_rate_limiter");
            else if (result != null && result.Contains("Please post something at least 5 characters long."))
                Response.Write("error_post_at_least_5_characters");
            else
                Response.Write("Message Sent!");

        }
        catch (Exception)
        {

            Response.Write("error_communication_send");
            return;
        }
    }
}
