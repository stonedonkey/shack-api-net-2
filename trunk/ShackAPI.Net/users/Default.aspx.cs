using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Xml;
using System.Text;
using HtmlAgilityPack;
using System.Web.Script.Serialization;

public partial class users_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        if (string.IsNullOrEmpty(Request.QueryString["username"]))
            throw new Exception("Missing User Name");

        if (string.IsNullOrEmpty(Request.QueryString["json"]))
            ServePageAsXML();
        else
            ServePageAsJSON();

    }
    private void ServePageAsJSON()
    {
        string username = Request.QueryString["username"];
        string url = String.Format("http://www.shacknews.com/profile/{0}", username);

        System.Net.WebClient client = new WebClient();
        string shackHTML = client.DownloadString(url); 

        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);

        Response.ContentType = "application/json";

        Encoding utf8 = new UTF8Encoding(false);


        JsonUser user = new JsonUser();
        foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//tr"))
        {
            HtmlNode th = post.SelectSingleNode("th");
            if (th != null)
            {


                if (user.sex == null )
                    user.sex = GetPageValue("Sex", th, null, post, "sex");

                if (user.location == null)
                    user.location = GetPageValue("Location", th, null, post, "location");

                if (user.steam == null)
                    user.steam = GetPageValue("Steam", th, null, post, "steam");

                if (user.xbox_live == null)
                    user.xbox_live = GetPageValue("Xbox Live", th, null, post, "xbox-live");

                if (user.xfire == null)
                    user.xfire = GetPageValue("XFire", th, null, post, "xfire");

                if (user.psn == null)
                    user.psn = GetPageValue("PlayStation Network", th, null, post, "psn");

                if (user.age == null)
                    user.age = GetPageValue("age", th, null, post, "age");

                if (user.join_date == null)
                    user.join_date = GetPageValue("Registered", th, null, post, "join-date");

                if (user.wii == null)
                    user.wii = GetPageValue("Wii", th, null, post, "wii");

                if (user.homepage == null)
                    user.homepage = GetPageValue("Homepage", th, null, post, "homepage");

            }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();
        string jsonPosts = js.Serialize(user);
        Response.Write(jsonPosts);


    }
    private void ServePageAsXML()
    {
        string username = Request.QueryString["username"];
        string url = String.Format("http://www.shacknews.com/profile/{0}", username);

        System.Net.WebClient client = new WebClient();
        String shackHTML = client.DownloadString(url);
        //string shackHTML = HTTPManager.GetURLWithGzip(url);

        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);

        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("user");

        writer.WriteStartElement("name");
        writer.WriteValue(doc.DocumentNode.SelectSingleNode("//div[@class='wholec profilemain']").SelectSingleNode("h3").InnerText);
        writer.WriteEndElement();

        foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//tr"))
        {
            HtmlNode th = post.SelectSingleNode("th");
            if (th != null)
            {
                GetPageValue("Sex", th, writer, post, "sex");
                GetPageValue("Location", th, writer, post, "location");
                GetPageValue("Steam", th, writer, post, "steam");
                GetPageValue("Xbox Live", th, writer, post, "xbox-live");
                GetPageValue("XFire", th, writer, post, "xfire");
                GetPageValue("PlayStation Network", th, writer, post, "psn");
                GetPageValue("age", th, writer, post, "age");
                GetPageValue("Registered", th, writer, post, "join-date");
                GetPageValue("Wii", th, writer, post, "wii");
                GetPageValue("Homepage", th, writer, post, "homepage");

            }
        }


        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();
        writer.Close();
    }
    private string GetPageValue(string findname, HtmlNode th, XmlTextWriter writer, HtmlNode post, string tagName)
    {

        if (string.IsNullOrEmpty(Request.QueryString["json"]))
        {
            if (th.InnerText.ToLower() == findname.ToLower())
            {
                HtmlNode td = post.SelectSingleNode("td");
                if (td != null)
                {
                    writer.WriteStartElement(tagName);
                    // TODO: this is pretty much the worst thing ever...
                    writer.WriteValue(Server.HtmlDecode(Server.HtmlEncode(td.InnerText.Trim()).Replace("&#194;", "")).Replace("&nbsp;", " "));
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteStartElement(tagName);
                    writer.WriteEndElement();
                }
            }
        }
        else
        {
            if (th.InnerText.ToLower() == findname.ToLower())
            {
                HtmlNode td = post.SelectSingleNode("td");
                if (td != null)
                {

                    return Server.HtmlDecode(Server.HtmlEncode(td.InnerText.Trim()).Replace("&#194;", "")).Replace("&nbsp;", " ");
                }
                else
                {
                    return tagName;
                }
            }
        }

        return null;
    }
}
