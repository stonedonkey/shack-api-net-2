using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using System.Web.Script.Serialization;

public partial class messages_Default : System.Web.UI.Page
{
    private List<ShackMessage> results = new List<ShackMessage>();
    private string totalPages = "1";
    private string page = "1";
    private string totalResults = "0";
    private string _Username;

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Expires = 0;

        int version = 1;
        int.TryParse(Request.QueryString["version"], out version);

        _Username = "";
        string password = "";

        string headers = Context.Request.Headers["Authorization"];
        if (headers.Length > 7)
        {
            string ticket = headers.Substring(6);
            string clearTicket = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ticket));

            password = clearTicket.Substring(clearTicket.LastIndexOf(":") + 1);
            _Username = clearTicket.Substring(0, clearTicket.LastIndexOf(":"));

            if (_Username.Contains("\\") && !_Username.EndsWith("\\"))
                _Username = _Username.Substring(_Username.LastIndexOf("\\") + 1);


        }

        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";
        client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 ( .NET CLR 3.5.30729; .NET4.0E)";
        client.Headers["X-Requested-With"] = "XMLHttpRequest";

        try
        {


            NameValueCollection c = new NameValueCollection();
            //c.Add("email", _Username);
            //c.Add("password", password);
            //c.Add("login", "login");
            c.Add("user-identifier", _Username);
            c.Add("supplied-pass", password);
            c.Add("get_fields[]", "result");
            c.Add("remember-login", "1");

            client.Cookies = cc;
            string urlCookie = "https://www.shacknews.com/account/signin";
            Byte[] webResponse = client.UploadValues(urlCookie, "POST", c);
            String result = Encoding.UTF8.GetString(webResponse);

            if (!result.Contains("{\"result\":{\"valid\":\"true\""))
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


        string shackPage = "0";
        if (string.IsNullOrEmpty(Request["page"]) == false)
        {
            page = Request["page"];
            shackPage = Request["page"]; //(int.Parse(page) - 1).ToString(); // shack uses 0 based paging, so page is always -1
        }

        String shackHTML = "";
        String URL = "";

        if (Request["box"] == "archive")
            URL = string.Format("http://www.shacknews.com/messages/sent/?page={0}", shackPage);  // archive
        else if (Request["box"] == "outbox")
            URL = string.Format("http://www.shacknews.com/messages/sent/?page={0}", shackPage);   // outbox
        else
            URL = string.Format("http://www.shacknews.com/messages/inbox?page={0}", shackPage); // inbox
        try
        {
            client.Method = "GET";
            client.Headers.Remove("X-Requested-With");
            // load the HTML from the shack
            shackHTML = client.DownloadString(URL);
        }
        catch (Exception)
        {
            Response.Write("error_communication_inbox");
            return;
        }


        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);

        // try and retreive the total number of results
        try
        {
            string resultText = doc.DocumentNode.SelectSingleNode("//div[@class='showing-column']").InnerText;
            Match match = Regex.Match(resultText, @"of ([\d]+)");
            if (match.Success)
                totalResults = Regex.Replace(match.Groups[0].ToString(), "of ", "");
            else
                totalResults = "0";
        }
        catch (Exception)
        {

            totalResults = "0";
        }

        // try and get the number of pages for this story
        // Shacknews doesn't page properly, when showing 50 of 50, for instance it says there's two pages
        // this is not correct
        try
        {
            totalPages = Math.Ceiling(double.Parse(totalResults) / 50).ToString();
        }
        catch (Exception)
        {
            totalPages = "1";
        }


        // cleanup 
        client.Dispose();
        client = null;

        try
        {
            foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//ul[@id='messages']//li"))
            {
                ShackMessage msg = new ShackMessage();
                msg = ParseResult(post.InnerHtml, post.GetAttributeValue("class", "message").ToString());

                if (msg.from != null && msg.from.Length > 0)
                    results.Add(msg);

            }
        }
        catch (Exception ex)
        {
            string error = ex.Message;
        }
        if (string.IsNullOrEmpty(Request.QueryString["json"]))
        {
            if (version == 2)
                ServePageAsXML2();
            else
                ServePageAsXML();
        }
        else
            ServePageAsJSON2();

    }


    private ShackMessage ParseResult(string node, string read)
    {

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(node);

        ShackMessage msg = new ShackMessage();
        msg.from = doc.DocumentNode.SelectSingleNode("//a[@class='username']").InnerText.Trim();
        msg.subject = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'subject-column')]//a").InnerText.Trim();
        msg.date = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'date-column')]//a").InnerText.Trim();

        msg.body = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'message-body')]").InnerHtml.Trim();

        msg.id = doc.DocumentNode.SelectSingleNode("//input[@class='mid']").GetAttributeValue("value", "0");
        msg.unread = Regex.Replace(read, "message", "");

        if (msg.unread.Equals(""))
            msg.unread = "unread";

        return msg;

    }
    private void ServePageAsXML()
    {

        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("messages");

        writer.WriteAttributeString("page", page);
        writer.WriteAttributeString("last_page", totalPages);
        writer.WriteAttributeString("total_results", totalResults);

        foreach (var item in results)
        {
            writer.WriteStartElement("message");
            writer.WriteAttributeString("author", item.from);
            writer.WriteAttributeString("subject", item.subject);
            writer.WriteAttributeString("date", item.date);
            writer.WriteAttributeString("id", item.id);
            writer.WriteAttributeString("status", item.unread);
            writer.WriteElementString("body", item.body);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();
    }

    private void ServePageAsXML2()
    {
        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("messages");
        writer.WriteAttributeString("user", _Username);

        foreach (var item in results)
        {
            Boolean isUnRead = false;
            if (item.unread == "unread")
                isUnRead = true;
            else
                isUnRead = false;

            //TimeZone tz = TimeZone.CurrentTimeZone;
            //DateTime datePosted = DateTime.ParseExact(item.date, "MMM dd, yyyy h:mmtt CST", CultureInfo.InvariantCulture);
            //if (tz.IsDaylightSavingTime(DateTime.Now) == true)
            //    datePosted = datePosted.AddHours(-1);

            //String dateout = String.Format("{0:ddd MMM dd hh:mm:00 -0700 yyyy}", datePosted);

            String dateout = Helpers.FormatShackDate(item.date, OutputFormats.XML);


            writer.WriteStartElement("message");
            writer.WriteAttributeString("from", item.from);
            writer.WriteAttributeString("subject", item.subject);
            writer.WriteAttributeString("date", dateout);
            writer.WriteAttributeString("id", item.id);
            writer.WriteAttributeString("unread", isUnRead.ToString().ToLower());
            writer.WriteString(item.body);
            //writer.WriteElementString("body", item.Text);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();

    }
    private void ServePageAsJSON2()
    {

        Response.ContentType = "application/json";

        TimeZone tz = TimeZone.CurrentTimeZone;

        foreach (var item in results)
        {
            //"March 1, 2011, 1:15 pm"
            DateTime datePosted = DateTime.ParseExact(item.date, "MMMM d, yyyy, h:mm tt", CultureInfo.InvariantCulture);

            if (tz.IsDaylightSavingTime(DateTime.Now) == true)
                datePosted = datePosted.AddHours(-1);

            //String dateout = String.Format("{0:yyyy/MM/dd hh:mm:00 -0700}", datePosted);

            item.date = Helpers.FormatShackDate(item.date, OutputFormats.JSON);

            if (item.unread == "unread")
                item.unread = "true";
            else
                item.unread = "false";
        }

        StringBuilder sb = new StringBuilder();

        JavaScriptSerializer js = new JavaScriptSerializer();

        JsonShackMessage json = new JsonShackMessage();
        json.messages = results;

        json.user = _Username;
        json.last_page = totalPages;

        string jsonPosts = js.Serialize(json);



        Response.Write(jsonPosts);
    }
}

