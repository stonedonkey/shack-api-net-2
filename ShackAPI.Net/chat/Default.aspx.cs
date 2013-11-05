using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;

public partial class chat_Default : System.Web.UI.Page
{
    private List<ShackPost> posts = new List<ShackPost>();

    protected void Page_Load(object sender, EventArgs e)
    {

        Response.Expires = 0;

        string url = String.Format("http://www.shacknews.com/chatty");
        String shackHTML;
        using (WebClientExtended client = new WebClientExtended())
        {
            client.Method = "GET";
            if (ShackUserContext.Current.CookieContainer == null)
                HTTPManager.SetShackUserContext();

            client.Cookies = ShackUserContext.Current.CookieContainer;
            // lets try and do some gzippy stuff here
            //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
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
            if (!shackHTML.Contains("latestchatty")) // if we lose session we have to reclaim it
            {
                HTTPManager.SetShackUserContext();
                client.Cookies = ShackUserContext.Current.CookieContainer;
                shackHTML = client.DownloadString(url);
            }
        }

        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);
        foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//div[starts-with(@id,'root_')]"))
        {
            ParsePost(post.InnerHtml);
        }

        ServePageAsXML();

    }
    private void ParsePost(string node)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(node);

        ShackPost post = new ShackPost();

        post.body = doc.DocumentNode.SelectSingleNode("//div[@class='postbody']").InnerHtml;
        post.author = doc.DocumentNode.SelectSingleNode("//span[@class='author']//a").InnerText;
        post.date = doc.DocumentNode.SelectSingleNode("//div[@class='postdate']").InnerText;

        string preview = doc.DocumentNode.SelectSingleNode("//div[@class='postbody']").InnerHtml.Replace("\r\n", " ").Trim();
        preview = Regex.Replace(preview, "<span class=\"jt_spoiler\" onclick=\".*?\">(.*?)</span>", "_________");
        preview = Regex.Replace(preview, @"<(.|\n)*?>", "");
        preview = Regex.Replace(preview, "(\r\n|\r|\n|\n\r)", "");
        post.preview = preview;

        string modmarker = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'fullpost ')]").GetAttributeValue("class", "").ToString();
        modmarker = modmarker.Substring(18);
        modmarker = modmarker.Substring(0, modmarker.IndexOf(" "));
        post.category = modmarker;

        // get postid's
        int beginpos = node.IndexOf("return clickItem(") + 18;
        int endpos = node.IndexOf(");", beginpos);
        String p = node.Substring(beginpos, endpos - beginpos);
        String[] split = Regex.Split(p, ", ");
        post.id = split[1];

        // all the posts that reply to this are also in this node string, so we count them for the reply count
        HtmlNodeCollection replies = doc.DocumentNode.SelectNodes("//li[starts-with(@id,'item_')]");
        if (replies != null)
            post.reply_count = replies.Count.ToString();
        else
            post.reply_count = "0";

        // get the replies to this root post and do stuff
        // create the list of participates for this thread  
        if (posts != null) // only get partipants for first post
        {
            List<Participants> participants = new List<Participants>();
            HtmlNodeCollection usernames = doc.DocumentNode.SelectNodes("//span[starts-with(@class,'oneline_user')]");
            foreach (var item in usernames)
            {
                string username = item.InnerText.Trim();

                Participants part;
                part = participants.Find(w => w.username == username);
                if (part == null)
                {
                    part = new Participants();
                    part.username = username;
                    part.post_count = 1;
                    participants.Add(part);
                }
                else
                    part.post_count++;
            }

            if (participants.Count > 0)
                post.participants = participants;
        }

        // TODO: now we need to loop through the replies and keep building out this reply
        // but we don't need all the information in the root,just the basic reply infos as we
        // do when we render a single thread
        replies = doc.DocumentNode.SelectNodes("//div[@class='capcontainer']/ul/li");
        if (replies != null)
            foreach (var reply in replies)
            {
                ParseReplies(ref post, reply.InnerHtml);
            }

        posts.Add(post);

    }
    private void ParseReplies(ref ShackPost post, string n)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(n);

        ShackPost r = new ShackPost();

        r.body = doc.DocumentNode.SelectSingleNode("//span[@class='oneline_body']").InnerHtml;
        r.author = doc.DocumentNode.SelectSingleNode("//span[starts-with(@class,'oneline_user')]").InnerText;

        if (post.comments == null)
            post.comments = new List<ShackPost>();

        post.comments.Add(r);

        HtmlNodeCollection replies = doc.DocumentNode.SelectNodes("/ul/li");
        if (replies != null)
            foreach (var reply in replies)
            {
                ParseReplies(ref r, reply.InnerHtml);
            }



    }

    private void ServePageAsXML()
    {

        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        using (XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8))
        {
            writer.Formatting = System.Xml.Formatting.Indented;

            writer.WriteStartDocument();
            writer.WriteStartElement("comments");

            writer.WriteAttributeString("story_name", "LatestChatty");
            writer.WriteAttributeString("last_page", "1");
            writer.WriteAttributeString("page", "1");
            writer.WriteAttributeString("story_id", "17");

            List<int> threading = new List<int>();

            foreach (var item in posts)
            {
                writer.WriteStartElement("comment");
                writer.WriteAttributeString("reply_count", item.reply_count);
                writer.WriteAttributeString("date", item.date);
                writer.WriteAttributeString("category", item.category);
                writer.WriteAttributeString("author", item.author);
                writer.WriteAttributeString("id", item.id);

                if (item.last_reply_id != null && item.last_reply_id.Length > 0)
                    writer.WriteAttributeString("last_reply_id", item.last_reply_id); // no last reply on thread view

                writer.WriteAttributeString("preview", item.preview);  // squeegy's api does the actual truncate off a clean body, we don't
                writer.WriteElementString("body", item.body);


                if (item.participants != null && item.participants.Count > 0)
                {
                    writer.WriteStartElement("participants");
                    foreach (var part in item.participants)
                    {
                        writer.WriteStartElement("participant");
                        writer.WriteAttributeString("posts", part.post_count.ToString());
                        writer.WriteString(part.username);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                // add a blank comment tag if no comments
                if (int.Parse(item.reply_count) == 0)
                {
                    writer.WriteStartElement("comments");
                    writer.WriteFullEndElement();
                    writer.WriteEndElement();
                }

                // close the tags that have since had the proper number of replies go by
                for (int i = 0; i < threading.Count; i++)
                {
                    threading[i]--;
                    if (threading[i] == 0)
                    {
                        writer.WriteFullEndElement();
                        writer.WriteEndElement();
                    }
                }


                if (int.Parse(item.reply_count) > 0)
                {
                    writer.WriteStartElement("comments");
                    threading.Insert(0, int.Parse(item.reply_count));
                }
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
        }


    }

    private void ServePageAsJSON()
    {
        Response.ContentType = "application/json";

        StringBuilder sb = new StringBuilder();

        JavaScriptSerializer js = new JavaScriptSerializer();

        JsonComments json = new JsonComments();
        json.comments = posts;
        json.page = "1";
        json.story_id = "17";
        json.story_name = "LatestChatty";
        json.last_page = "1";

        string jsonPosts = js.Serialize(json);

        // TODO: I'm assuming squeegy is placing an array of posts in the <comments>  part of XML, if so that explains
        //       the blank array in his json output, for now I'm going to fake mimic this.
        jsonPosts = jsonPosts.Replace("null", "[]");

        Response.Write(jsonPosts);
    }
}