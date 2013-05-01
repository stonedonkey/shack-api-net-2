using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlAgilityPack;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;

public partial class _Default : System.Web.UI.Page
{
    private List<ShackPost> posts = new List<ShackPost>();
    private String title;
    private String totalPages;
    private String currentPage = "1";
    private String storyID = "";
    private Hashtable postsHash = new Hashtable();
    private int last_reply_id = 0;
    private String postBodies = "";
    private HtmlDocument postBodiesDocument = new HtmlDocument();
    private OutputFormats outputFormat = OutputFormats.XML;

    private List<idlist> id_list = new List<idlist>();
    WebClientExtended client = new WebClientExtended();
    private class idlist
    {
        public idlist(string id, int count)
        {
            this.id = id;
            this.count = count;
        }
        public string id { get; set; }
        public int count { get; set; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {

        Response.Expires = 0;

        if (string.IsNullOrEmpty(Request.QueryString["json"]))
            outputFormat = OutputFormats.XML;
        else
            outputFormat = OutputFormats.JSON;


            if (Request.QueryString["threadid"] == null)
                throw new Exception("Missing ThreadId");

        string threadid = Request.QueryString["threadid"];

        string url = String.Format("http://www.shacknews.com/chatty?id={0}", threadid);
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
            if (!shackHTML.Contains("shackapifix")) // if we lose session we have to reclaim it
            {
                HTTPManager.SetShackUserContext();
                client.Cookies = ShackUserContext.Current.CookieContainer;
                shackHTML = client.DownloadString(url);
            }
        }


        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);


        String rootPostId = doc.DocumentNode.SelectSingleNode("//div[@class='root']").GetAttributeValue("id", "");
        rootPostId = rootPostId.Replace("root_", "");


        // retreive all the post bodies
        url = String.Format("http://www.shacknews.com/frame_laryn.x?root={0}", rootPostId);
        using (WebClientExtended client = new WebClientExtended())
        {
            client.Encoding = Encoding.UTF8;
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
                    reader = new StreamReader(response,Encoding.UTF8);

                postBodies = reader.ReadToEnd();
            }
            if (!shackHTML.Contains("shackapifix")) // if we lose session we have to reclaim it
            {
                HTTPManager.SetShackUserContext();
                client.Cookies = ShackUserContext.Current.CookieContainer;
                postBodies = client.DownloadString(url);
            }
        }


        postBodiesDocument.LoadHtml(postBodies);


        //title = doc.DocumentNode.SelectSingleNode("//a[starts-with(@href ,'http://www.shacknews.com/onearticle.x/')]").InnerText;
        title = "LatestChatty";

        // get the number of pages for this story
        HtmlNodeCollection pages = doc.DocumentNode.SelectNodes("//a[starts-with(@href,'/laryn.x?story=')]");
        if (pages != null && pages.Count > 1)
            totalPages = pages[pages.Count - 2].InnerText.ToString();
        else
            totalPages = "1";

        // get the story
        storyID = doc.DocumentNode.SelectSingleNode("//input[@id='content_id']").GetAttributeValue("value","17");
        //storyID = "17";

        foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//li[starts-with(@id,'item_')]"))
        {
            ParsePost(post.InnerHtml);
        }


        if (posts[0] != null)
            posts[0].last_reply_id = last_reply_id.ToString();

        if (outputFormat == OutputFormats.XML)
        {
            ServePageAsXML();
        }
        else
        {
            ServePageAsJSON();
        }

    }
    protected void ParsePost(String node)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(node);

        ShackPost sp = new ShackPost();

        sp.body = "";// doc.DocumentNode.SelectSingleNode("//div[@class='postbody']").InnerHtml.Replace("\r", "&#13;").Replace("\n", "").Replace("<br>", "<br />");

        string preview = doc.DocumentNode.SelectSingleNode("//span[@class='oneline_body']").InnerHtml.Replace("\r\n", " ").Trim();
        preview = Regex.Replace(preview, "<span class=\"jt_spoiler\" onclick=\".*?\">(.*?)</span>", "_________");
        preview = Regex.Replace(preview, @"<(.|\n)*?>", "");
        preview = Regex.Replace(preview, "(\r\n|\r|\n|\n\r)", "");
        sp.preview = preview;

        //sp.Preview = doc.DocumentNode.SelectSingleNode("//span[@class='oneline_body']").InnerText.Replace("\r\n", " ").Trim();
        sp.date = "";// doc.DocumentNode.SelectSingleNode("//div[@class='postdate']").InnerText.Trim();
        sp.author = doc.DocumentNode.SelectSingleNode("//span[starts-with(@class ,'oneline_user')]").InnerText.Trim();

        string modmarker;
        int beginpos;
        int endpos;
        string post;
        String[] split;

        // first post is different.. horray for HTML!
        if (doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'fullpost ')]") != null)
        {
            modmarker = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'fullpost ')]").GetAttributeValue("class", "").ToString();
            modmarker = modmarker.Substring(15);
            modmarker = modmarker.Substring(0, modmarker.IndexOf(" "));
            sp.category = modmarker;

            // get last postid
            beginpos = node.LastIndexOf("return clickItem(") + 18;
            endpos = node.IndexOf(");", beginpos);
            post = node.Substring(beginpos, endpos - beginpos);
            split = Regex.Split(post, ", ");
            //sp.last_reply_id = split[1];


        }
        else
        {
            modmarker = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'oneline ')]").GetAttributeValue("class", "").ToString();
            modmarker = modmarker.Substring(modmarker.IndexOf("olmod_") + 6);
            modmarker = modmarker.Substring(0, modmarker.IndexOf(" "));
            sp.category = modmarker;
        }

        // get postid's
        beginpos = node.IndexOf("return clickItem(") + 18;
        endpos = node.IndexOf(");", beginpos);
        post = node.Substring(beginpos, endpos - beginpos);
        split = Regex.Split(post, ", ");

        sp.id = split[1];

        if (last_reply_id < int.Parse(sp.id))
            last_reply_id = int.Parse(sp.id);



        // all the posts that reply to this are also in this node string, so we count them for the reply count
        HtmlNodeCollection replies = doc.DocumentNode.SelectNodes("//li[starts-with(@id,'item_')]");
        if (replies != null)
            sp.reply_count = replies.Count.ToString();
        else
            sp.reply_count = "0";


        // get the replies to this root post and do stuff
        // create the list of participates for this thread  
        if (posts != null && posts.Count == 0) // only get partipants for first post
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
                sp.participants = participants;
        }



        // TODO: OK, so we built the XML where everything is generated from a flat list of objects, but JSON requires
        //       objects within your objects.
        //       TLDR: I'm making a different post object for XML vs JSON (psst: json's will be more correct)
        if (string.IsNullOrEmpty(Request.QueryString["json"]))
        {
            posts.Add(sp);
            GetFullPost(ref sp);
        }
        else
        {
            ShackPost holdpost = new ShackPost();
            if (id_list.Count > 0)
            {
                holdpost = FindShackPost(posts, id_list[0].id);

                for (int i = 0; i < id_list.Count; i++)
                {
                    id_list[i].count = id_list[i].count - 1;
                }
                id_list.RemoveAll(w => w.count == 0);


                if (holdpost.comments == null)
                    holdpost.comments = new List<ShackPost>();

                GetFullPost(ref sp);

                //if (sp.comments == null)
                 //   sp.comments = new List<ShackPost>();  // don't allow null posts to go in, empty to match squeegy

                holdpost.comments.Add(sp);

            }
            else
            {
                //if (sp.comments == null)
                 //   sp.comments = new List<ShackPost>();

                posts.Add(sp);
                GetFullPost(ref sp);
            }

            if (sp.reply_count != "0")
                id_list.Insert(0, new idlist(sp.id, int.Parse(sp.reply_count)));
        }



    }
    private ShackPost FindShackPost(List<ShackPost> p, string id)
    {
        ShackPost found;
        foreach (ShackPost sp in p)
        {
            if (sp.id == id)
                return sp;
            else
                if (sp.comments != null)
                {
                    found = FindShackPost(sp.comments, id);  // go go recursion!
                    if (found != null)
                        return found;
                }
        }

        return null;

    }
    private void GetFullPost(ref ShackPost post)
    {

        String id = post.id;
        String fullPostText = postBodiesDocument.DocumentNode.SelectSingleNode("id('item_" + id + "')/div/div[5]").InnerHtml.Replace("\r", "&#13;").Replace("\n", "").Replace("<br>", "<br />"); 
        String postDate = postBodiesDocument.DocumentNode.SelectSingleNode("id('item_" + id + "')/div/div[6]").InnerText.Trim();

        postDate = Helpers.FormatShackDate(postDate,outputFormat);

        if (post == null)
        {
            posts[posts.Count - 1].body = fullPostText.Replace("return doSpoiler(event);", "this.className = '';");
            posts[posts.Count - 1].date = postDate;
        }
        else
        {
            post.body = fullPostText.Replace("return doSpoiler(event);", "this.className = '';");
            post.date = postDate;
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

            writer.WriteAttributeString("story_name", this.title);
            writer.WriteAttributeString("last_page", this.totalPages);
            writer.WriteAttributeString("page", this.currentPage);
            writer.WriteAttributeString("story_id", this.storyID);

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
        json.page = this.currentPage;
        json.story_id = this.storyID;
        json.story_name = this.title;
        json.last_page = this.totalPages;

        string jsonPosts = js.Serialize(json);

        // TODO: I'm assuming squeegy is placing an array of posts in the <comments>  part of XML, if so that explains
        //       the blank array in his json output, for now I'm going to fake mimic this.
        jsonPosts = jsonPosts.Replace("null", "[]");

        Response.Write(jsonPosts);
    }
}