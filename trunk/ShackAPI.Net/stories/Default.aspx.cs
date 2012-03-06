using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Xml;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.IO;
using System.IO.Compression;

public partial class stories_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Request.QueryString["storyid"]))
            throw new Exception("Missing StoryID");

        string storyID = Request.QueryString["storyid"];


        if (string.IsNullOrEmpty(Request.QueryString["json"]))
            ServePageAsXML(storyID);
        else
            ServePageAsJSON(storyID);
    }

    private void ServePageAsJSON(string storyID)
    {
        Response.ContentType = "application/json";

        StringBuilder sb = new StringBuilder();

        JavaScriptSerializer js = new JavaScriptSerializer();

        string url = String.Format("http://www.shacknews.com/article/{0}/somestory", storyID);
        string shackHTML = string.Empty;

        using (WebClientExtended client = new WebClientExtended())
        {
            client.Method = "GET";
            if (ShackUserContext.Current.CookieContainer == null)
                HTTPManager.SetShackUserContext();

            client.Cookies = ShackUserContext.Current.CookieContainer;
            // lets try and do some gzippy stuff here
            client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            using (Stream response = client.OpenRead(url))
            {
                string contentEncoding = client.ResponseHeaders["Content-Encoding"];
                StreamReader reader;
                if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.Contains("gzip"))
                    reader = new StreamReader(new GZipStream(response, CompressionMode.Decompress), Encoding.UTF8);
                else
                    reader = new StreamReader(response, Encoding.UTF8);

                shackHTML = reader.ReadToEnd();
            }
        }

        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);

        ShackStory store = new ShackStory();


        store.name = doc.DocumentNode.SelectSingleNode("//div[@id='article']//h1").InnerText;

        store.date = doc.DocumentNode.SelectSingleNode("//span[@class='vitals']").InnerText.Trim();
        store.date = store.date.Substring(store.date.IndexOf(", ") + 2);

        store.body = doc.DocumentNode.SelectSingleNode("//div[@id='article_body']").InnerHtml.Trim().Replace("\r", "").Replace("\n", "").Replace("&", "&amp;").Replace("<br>", "<br />").Replace("<p><p>", "<p>");
        store.body = store.body.Replace("<script type=\"text/javascript\">", "<script type=\"text/javascript\"><![CDATA[");
        store.body = store.body.Replace("</script>", "]]></script>");

        store.preview = doc.DocumentNode.SelectSingleNode("//div[@id='article_body']").InnerText.Trim();

        try
        {
            store.comment_count = doc.DocumentNode.SelectSingleNode("//div[@id='commenttools']//a[2]").InnerText.Replace(" Comments", "");
        }
        catch
        {
            store.comment_count = "0";
        }

        store.id = storyID;


        string jsonPosts = js.Serialize(store);
        Response.Write(jsonPosts);
    }

    private void ServePageAsXML(string storyID)
    {
        try
        {

            string url = String.Format("http://www.shacknews.com/article/{0}/somestory", storyID);

            //String shackHTML = client.DownloadString(url);
            string shackHTML = string.Empty;


            using (WebClientExtended client = new WebClientExtended())
            {
                client.Method = "GET";
                if (ShackUserContext.Current.CookieContainer == null)
                    HTTPManager.SetShackUserContext();

                client.Cookies = ShackUserContext.Current.CookieContainer;
                // lets try and do some gzippy stuff here
                client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                using (Stream response = client.OpenRead(url))
                {
                    string contentEncoding = client.ResponseHeaders["Content-Encoding"];
                    StreamReader reader;
                    if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.Contains("gzip"))
                        reader = new StreamReader(new GZipStream(response, CompressionMode.Decompress), Encoding.UTF8);
                    else
                        reader = new StreamReader(response, Encoding.UTF8);

                    shackHTML = reader.ReadToEnd();
                }
            }

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(shackHTML);

            ShackStory store = new ShackStory();

            store.name = doc.DocumentNode.SelectSingleNode("//div[@id='article']//h1").InnerText;

            store.date = doc.DocumentNode.SelectSingleNode("//span[@class='vitals']").InnerText.Trim();
            store.date = store.date.Substring(store.date.IndexOf(", ") + 2);

            store.body = doc.DocumentNode.SelectSingleNode("//div[@id='article_body']").InnerHtml.Trim().Replace("\r", "").Replace("\n", "").Replace("&", "&amp;").Replace("<br>", "<br />").Replace("<p><p>", "<p>");
            store.body = store.body.Replace("<script type=\"text/javascript\">", "<script type=\"text/javascript\"><![CDATA[");
            store.body = store.body.Replace("</script>", "]]></script>");

            store.preview = doc.DocumentNode.SelectSingleNode("//div[@id='article_body']").InnerText.Trim();

            try
            {
                store.comment_count = doc.DocumentNode.SelectSingleNode("//div[@id='commenttools']//a[2]").InnerText.Replace(" Comments", "");
            }
            catch
            {
                store.comment_count = "0";
            }

            Response.ContentType = "text/xml";

            Encoding utf8 = new UTF8Encoding(false);

            XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
            writer.Formatting = System.Xml.Formatting.Indented;

            writer.WriteStartDocument();
            writer.WriteStartElement("story");

            writer.WriteStartElement("comment-count");
            writer.WriteAttributeString("type", "integer");
            writer.WriteValue(store.comment_count);
            writer.WriteEndElement();


            writer.WriteStartElement("date");
            writer.WriteValue(store.date);
            writer.WriteEndElement();

            writer.WriteStartElement("name");
            writer.WriteValue(store.name);
            writer.WriteEndElement();

            writer.WriteStartElement("body");
            writer.WriteValue(store.body);
            writer.WriteEndElement();

            writer.WriteStartElement("id");
            writer.WriteAttributeString("type", "integer");
            writer.WriteValue(storyID);
            writer.WriteEndElement();

            writer.WriteStartElement("preview");
            writer.WriteValue(store.preview);
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();

        }
        catch (Exception)
        {


            throw new Exception("Error parsing this story id");

          
        }


    }



}
