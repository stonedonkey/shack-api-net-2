using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlAgilityPack;
using System.Web.Script.Serialization;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.GZip;
using System.Collections.Specialized;
using System.Globalization;
using System.IO.Compression;

namespace ShackAPI
{

    public partial class _Default : System.Web.UI.Page
    {
        private List<ShackPost> posts = new List<ShackPost>();
        private String title;
        private String totalPages;
        private String currentPage = "1";
        private String storyID = "";
        private int last_reply_id = 0;
        private OutputFormats outputFormat = OutputFormats.XML;

        protected double pageStart = DateTime.Now.TimeOfDay.TotalMilliseconds;
        protected double pageScraped;
        protected double pageEnd;
        protected bool usedGzip = false;
        protected bool loadedSession = false;

        protected void Page_Load(object sender, EventArgs e)
        {

            Response.Expires = 0;

            if (string.IsNullOrEmpty(Request.QueryString["json"]))
                outputFormat = OutputFormats.XML;
            else
                outputFormat = OutputFormats.JSON;

            string url;// = @"http://www.shacknews.com/latestchatty.x";

            if (string.IsNullOrEmpty(Request.QueryString["page"]) == false)
                currentPage = Request.QueryString["page"];

            if (Request.QueryString["page"] != null && Request.QueryString["storyid"] == "17")
                url = "http://www.shacknews.com/chatty/?page=" + Request.QueryString["page"];
            else if (Request.QueryString["page"] != null && Request.QueryString["storyid"] != null)
                url = "http://www.shacknews.com/laryn.x?story=" + Request.QueryString["storyid"] + "&page=" + Request.QueryString["page"];
            else if (Request.QueryString["storyid"] != null)
                url = "http://www.shacknews.com/article/" + Request.QueryString["storyid"] + "/api-get-story";
            else
                url = "http://www.shacknews.com/chatty";

            String shackHTML;
            using (WebClientExtended client = new WebClientExtended())
            {

                client.Method = "GET";
                if (ShackUserContext.Current.CookieContainer == null)
                {
                    loadedSession = true;
                    HTTPManager.SetShackUserContext();
                }

                client.Cookies = ShackUserContext.Current.CookieContainer;

                // lets try and do some gzippy stuff here
                //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                using (Stream response = client.OpenRead(url))
                {
                    string contentEncoding = client.ResponseHeaders["Content-Encoding"];

                    StreamReader reader;
                    if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.Contains("gzip"))
                    {
                        usedGzip = true;
                        reader = new StreamReader(new GZipStream(response, CompressionMode.Decompress), Encoding.UTF8);
                    }
                    else
                    {
                        reader = new StreamReader(response, Encoding.UTF8);
                    }
                    // end Gzip Compression code

                    shackHTML = reader.ReadToEnd();
                }
                if (!shackHTML.Contains("/user/shackapifix/posts")) // if we lose session we have to reclaim it
                {
                    HTTPManager.SetShackUserContext();
                    client.Cookies = ShackUserContext.Current.CookieContainer;
                    shackHTML = client.DownloadString(url);
                }
            }


            pageScraped = DateTime.Now.TimeOfDay.TotalMilliseconds;

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(shackHTML);

            //title = doc.DocumentNode.SelectSingleNode("//a[starts-with(@href ,'http://www.shacknews.com/onearticle.x/')]").InnerText;
            title = "LatestChatty";
            try
            {
                // get the number of pages for this story
                HtmlNodeCollection pages = doc.DocumentNode.SelectNodes("//div[@class='pagenavigation']//a[starts-with(@href,'/chatty')]");

                if (pages != null && pages.Count > 1)
                    if (pages[pages.Count - 1].InnerText.Contains("Next"))
                        totalPages = pages[pages.Count - 2].InnerText.ToString();
                    else
                        totalPages = pages[pages.Count - 1].InnerText.ToString();
                else
                    totalPages = "1";
            }
            catch (Exception)
            {

                totalPages = "1";
            }

            // get the story
            //storyID = doc.DocumentNode.SelectSingleNode("//input[@id='p_group']").GetAttributeValue("value", "");
            if (Request.QueryString["storyID"] != "17")
                if (string.IsNullOrEmpty(Request.QueryString["storyID"]))
                    storyID = "17";
                else
                    storyID = Request.QueryString["storyID"];
            else
                storyID = "17";

            if (doc.DocumentNode.SelectNodes("//div[starts-with(@class,'root')]") != null)
                foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//div[starts-with(@class,'root')]"))
                    ParsePost(post.InnerHtml);

            pageEnd = DateTime.Now.TimeOfDay.TotalMilliseconds;

            if (outputFormat == OutputFormats.XML)
                ServePageAsXML();
            else
                ServePageAsJSON();


        }
        protected void ParsePost(String node)
        {
            node = EscapeUnicode(node);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(node);

            ShackPost sp = new ShackPost();

            sp.body = doc.DocumentNode.SelectSingleNode("//div[@class='postbody']").InnerHtml.Replace("\r", "&#13;").Replace("\n", "").Replace("<br>", "<br />").Replace("return doSpoiler(event);", "this.className = '';");

            string preview = doc.DocumentNode.SelectSingleNode("//span[@class='oneline_body']").InnerHtml.Replace("\r\n", " ").Trim();
            preview = Regex.Replace(preview, "(\r\n|\r|\n|\n\r)", " ");
            preview = Regex.Replace(preview, "<span class=\"jt_spoiler\" onclick=\".*?\">(.*?)</span>", "_________");
            preview = Regex.Replace(preview, @"<(.|\n)*?>", "");
            sp.preview = preview;

            sp.date = doc.DocumentNode.SelectSingleNode("//div[@class='postdate']").InnerText.Trim();

            sp.date = Helpers.FormatShackDate(sp.date, outputFormat);

            sp.author = doc.DocumentNode.SelectSingleNode("//span[@class='user']").InnerText.Trim();

            // get post categeroy
            string modmarker = doc.DocumentNode.SelectSingleNode("//div[starts-with(@class,'fullpost ')]").GetAttributeValue("class", "").ToString();
            modmarker = modmarker.Substring(modmarker.IndexOf("mod_") + 4);
            modmarker = modmarker.Substring(0, modmarker.IndexOf(" "));
            sp.category = modmarker;

            // get the replies to this root post and do stuff
            List<Participants> participants = new List<Participants>();
            HtmlNodeCollection replies = doc.DocumentNode.SelectNodes("//div[starts-with(@class,'oneline')]");
            sp.reply_count = (replies.Count - 1).ToString();

            // need to determine the max replyid for last_reply_id // might be away to just do this with a query.. in selectnodes not sure.
            foreach (var item in replies)
            {
                Match match = Regex.Match(item.InnerHtml, @"\?id=([\d]*)");

                if (match.Success)
                {
                    int id = int.Parse(match.Groups[1].ToString());
                    if (last_reply_id < id)
                        last_reply_id = id;

                }

            }

            // create the list of participates for this thread
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

            // use the last post to determine the postid and the lastpost id.. this is pretty ugly
            HtmlNode lastNode = replies[replies.Count - 1];
            String lastpost = lastNode.InnerHtml;

            int beginpos = lastpost.IndexOf("return clickItem(") + 18;
            int endpos = lastpost.IndexOf(");", beginpos);

            string post = lastpost.Substring(beginpos, endpos - beginpos);
            String[] split = Regex.Split(post, ", ");

            sp.id = split[0];
            sp.last_reply_id = last_reply_id.ToString();
            last_reply_id = 0; // reset value for next run through

            // create empty list and stuff in comments property, get rid of the null -> [] string manip
            sp.comments = new List<ShackPost>();

            // end last post parsing

            posts.Add(sp);

        }

        private void ServePageAsXML()
        {

            Response.ContentType = "text/xml";

            Encoding utf8 = new UTF8Encoding(false);

            XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
            writer.Formatting = System.Xml.Formatting.Indented;

            writer.WriteStartDocument();

            if (!string.IsNullOrEmpty(Request.QueryString["debug"]))
            {
                writer.WriteComment("Paged Scraped: " + (pageScraped - pageStart).ToString() + " ms");
                writer.WriteComment("Total Parse: " + (pageEnd - pageScraped).ToString() + " ms");
                writer.WriteComment("Total Render: " + (pageEnd - pageStart).ToString() + " ms");
                writer.WriteComment("Used Gzip: " + usedGzip.ToString());
                writer.WriteComment("Loaded Session: " + loadedSession.ToString());
            }
            writer.WriteStartElement("comments");

            writer.WriteAttributeString("story_name", this.title);
            writer.WriteAttributeString("last_page", this.totalPages);
            writer.WriteAttributeString("page", this.currentPage);
            writer.WriteAttributeString("story_id", this.storyID);

            foreach (var item in posts)
            {
                writer.WriteStartElement("comment");
                writer.WriteAttributeString("reply_count", item.reply_count);
                writer.WriteAttributeString("date", item.date);
                writer.WriteAttributeString("category", item.category);
                writer.WriteAttributeString("author", item.author);
                writer.WriteAttributeString("last_reply_id", item.last_reply_id);
                writer.WriteAttributeString("id", item.id);
                writer.WriteAttributeString("preview", item.preview);
                writer.WriteElementString("body", item.body);

                writer.WriteStartElement("participants");
                foreach (var part in item.participants)
                {
                    writer.WriteStartElement("participant");
                    writer.WriteAttributeString("posts", part.post_count.ToString());
                    writer.WriteString(part.username);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();


                writer.WriteStartElement("comments");
                writer.WriteFullEndElement();
                writer.WriteEndElement();

            }

            writer.WriteEndElement();

            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
            writer = null;

        }
        private string EscapeUnicode(string result)
        {
            Regex rx = new Regex(@"\\[uU]([0-9A-F]{4})");
            result = rx.Replace(result, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            return result;

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
            //jsonPosts = jsonPosts.Replace("null", "[]");

            Response.Write(jsonPosts);
        }


    }
}