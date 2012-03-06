﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Web.Script.Serialization;
using System.IO;
using System.IO.Compression;

public partial class Stories : System.Web.UI.Page
{
    private List<ShackStory> posts = new List<ShackStory>();
    protected void Page_Load(object sender, EventArgs e)
    {

        string url = @"http://www.shacknews.com/news";

        String shackHTML;
        using (WebClientExtended client = new WebClientExtended())
        {
            client.Method = "GET";

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

        //try
        //{
        //    // get the number of pages for this story
        if (doc.DocumentNode.SelectNodes("//div[starts-with(@class,'story')]") != null)
            foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//div[starts-with(@class,'story')]"))
                ParseStory(post.InnerHtml);

        //}
        //catch (Exception ex)
        //{
        //    // fail boat right now...
        //    string test = ex.InnerException.ToString();
        //}

        if (string.IsNullOrEmpty(Request.QueryString["json"]))
            ServePageAsXML();
        else
            ServePageAsJSON();


    }
    private void ParseStory(String node)
    {

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(node);

        ShackStory store = new ShackStory();
        store.name = doc.DocumentNode.SelectSingleNode("//h1|//h2").InnerText;
        // get date of story
        try
        {
            string date = doc.DocumentNode.SelectSingleNode("//span[@class='byline']").InnerText;
            date = date.Substring(date.IndexOf(",", 0) + 1).Trim();
            store.date = date;
        }
        catch
        {
            // something changed in the formatting of the date
        }

        // reply count
        Match match = Regex.Match(node, @"see all (\d*) comment", RegexOptions.IgnoreCase);
        if (match.Success)
            store.comment_count = match.Groups[1].ToString();
        else
            store.comment_count = "0";

        try
        {
            string url = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'small-bubble')]//a").Attributes["href"].Value;
            store.url = "http://www.shacknews.com" + doc.DocumentNode.SelectSingleNode("//div[contains(@class,'small-bubble')]//a").Attributes["href"].Value;

            match = Regex.Match(node, @"article/(\d*)/", RegexOptions.IgnoreCase);
            if (match.Success)
                store.id = match.Groups[1].ToString();
            else
                store.id = "0";

        }
        catch { } // failed getting url

        store.body = doc.DocumentNode.SelectSingleNode("//div[@class='summary']").InnerHtml.Replace("\r", "").Replace("\n", "").Replace("\t","").Replace("&", "&amp;").Replace("<br>", "<br />").Replace("<p><p>", "<p>");
        store.body = store.body.Replace("<script type=\"text/javascript\">", "<script type=\"text/javascript\"><![CDATA[");
        store.body = store.body.Replace("</script>", "]]></script>");

        store.preview = doc.DocumentNode.SelectSingleNode("//div[@class='summary']").InnerText.Trim();

        posts.Add(store);


    }
    private void ServePageAsJSON()
    {
        Response.ContentType = "application/json";

        StringBuilder sb = new StringBuilder();

        JavaScriptSerializer js = new JavaScriptSerializer();

        string result = js.Serialize(posts);
        Response.Write(result);


    }

    private void ServePageAsXML()
    {

        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;


        writer.WriteStartDocument();

        writer.WriteStartElement("stories");
        writer.WriteAttributeString("type", "array");

        foreach (var item in posts)
        {

            writer.WriteStartElement("story");

            writer.WriteStartElement("name");
            writer.WriteValue(item.name);
            writer.WriteFullEndElement();

            writer.WriteStartElement("body");
            writer.WriteValue(item.body);
            writer.WriteFullEndElement();

            writer.WriteStartElement("id");
            writer.WriteAttributeString("type", "integer");
            writer.WriteValue(item.id);
            writer.WriteFullEndElement();

            writer.WriteStartElement("comment-count");
            writer.WriteAttributeString("type", "integer");
            writer.WriteValue(item.comment_count);
            writer.WriteFullEndElement();

            writer.WriteStartElement("preview");
            writer.WriteValue(item.preview);
            writer.WriteFullEndElement();

            writer.WriteStartElement("url");
            writer.WriteValue(item.url);
            writer.WriteFullEndElement();

            writer.WriteStartElement("date");
            writer.WriteValue(item.date);
            writer.WriteFullEndElement();

            writer.WriteFullEndElement();

            //writer.WriteElementString("comment-count", item.CommentCount);
            //writer.WriteAttributeString("type", "integer");
            //writer.WriteFullEndElement();

        }

        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();

        writer.Close();



    }
    public static string EscapeXml(string s)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement element = doc.CreateElement("temp");
        element.InnerText = s;
        return element.InnerXml;
    }
}
