using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using System.Net;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Globalization;

public partial class search_default : System.Web.UI.Page
{
    private List<SearchResult> results = new List<SearchResult>();
    private string totalPages = "1";
    private string page = "1";
    private string searchTerms = "";
    private string totalResults = "0";
    private string author = "";
    private string parent_author = "";
    private OutputFormats outputFormat = OutputFormats.XML;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Request.QueryString["json"]))
            outputFormat = OutputFormats.XML;
        else
            outputFormat = OutputFormats.JSON;

        Response.Expires = 0;

        searchTerms = Request["SearchTerm"];
        string filterByUser = Request["Author"];
        string filterByParentAuthor = Request["ParentAuthor"];
        string searchType = Request["SearchType"];

        author = filterByUser;
        parent_author = filterByParentAuthor;


        int version = 1;
        int.TryParse(Request.QueryString["version"], out version);


        page = Request["page"];

        if (string.IsNullOrEmpty(page))
            page = "1";


        //searchTerms = "stonedonkey";

        string url = String.Format("http://www.shacknews.com/search?chatty=1&type=4&chatty_terms={0}&chatty_user={1}&chatty_author={2}&chatty_filter=all&page={3}&result_sort=postdate_desc", Server.UrlEncode(searchTerms), Server.UrlEncode(filterByUser), Server.UrlEncode(filterByParentAuthor), page);



        // NOTE: Can't use the HTTPManager class because we need to use the same client, we could feasibly replicate
        //       the functionality in this location, but for as much as it's used not for now.
        WebClientExtended client = new WebClientExtended();
        client.Method = "GET";
        client.Encoding = Encoding.UTF8;
        String shackHTML = client.DownloadString(url);

        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(shackHTML);

        //Response.Write(shackHTML);

        // try and get the number of pages for this story
        try
        {
            HtmlNodeCollection pages = doc.DocumentNode.SelectNodes("//div[@class='pagination']//a[starts-with(@href,'/search?')]");
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



        // try and retreive the total number of results
        //totalResults = (15 * int.Parse(totalPages)).ToString();
        try
        {
            string resultText = doc.DocumentNode.SelectSingleNode("//h2[@class='search-num-found']").InnerText.Replace(",", "");
            Match match = Regex.Match(resultText, @"([\d]+)");
            if (match.Success)
                totalResults = match.Groups[0].ToString();
            else
                totalResults = "0";
        }
        catch (Exception)
        {

            totalResults = "0";
        }



        try
        {
            foreach (HtmlNode post in doc.DocumentNode.SelectNodes("//ul[@class='results']//li"))
            {
                ParseResult(post.InnerHtml);

            }
        }
        catch (Exception ex)
        {
            string error = ex.Message;

        }

        if (string.IsNullOrEmpty(Request.QueryString["json"]))
        {
            if (version == 2)
                ServerPageAsXML2();
            else
                ServePageAsXML();
        }
        else
            ServePageAsJSON2();

    }



    private void ParseResult(string node)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(node);

        SearchResult sr = new SearchResult();
        sr.author = doc.DocumentNode.SelectSingleNode("//span[@class='chatty-author']").InnerText;

        if (sr.author.Length > 1)
            sr.author = sr.author.Substring(0, sr.author.Length - 1);


        sr.preview = doc.DocumentNode.SelectSingleNode("//a[starts-with(@href,'/chatty/')]").InnerText;
        sr.story_name = "LatestChatty";

        String datePosted = doc.DocumentNode.SelectSingleNode("//span[@class='postdate']").InnerText;
        datePosted = datePosted.Replace("Posted ", "");
        sr.date = Helpers.FormatShackDate(datePosted, outputFormat);
        
        //try
        //{
        //    datePosted = datePosted.Replace("Posted ", "");
        //    datePosted = datePosted.Replace(" UTC", "");
        //    //datePosted = datePosted.Replace(" PST", "");
        //    //datePosted = datePosted.Replace(" PDT", "");

        //    //DateTime form = DateTime.Parse(datePosted);
        //    //Jun 05, 2009 9:21am PDT
        //    DateTime form = DateTime.ParseExact(datePosted, "MMM dd, yyyy h:mmtt PDT", CultureInfo.InvariantCulture);

        //    if (string.IsNullOrEmpty(Request.QueryString["json"]))
        //        datePosted = string.Format("{0:MMM dd, yyyy h:mmtt UTC}", form);


        //    datePosted = Regex.Replace(datePosted, "AM", "am");
        //    datePosted = Regex.Replace(datePosted, "PM", "pm");
        //    sr.date = datePosted;

        //}
        //catch (Exception ex)
        //{
        //    string error = ex.Message;

        //}

        string storyText = doc.DocumentNode.SelectSingleNode("//a[starts-with(@href,'/chatty/')]").Attributes["href"].Value;
        Match match = Regex.Match(storyText, @"([\d]+)");
        if (match.Success)
            sr.id = match.Groups[0].ToString();
        else
            sr.id = "0";

        //string threadText = doc.DocumentNode.SelectSingleNode("//td[@class='thread']").InnerHtml;
        //match = Regex.Match(threadText, @"([\d]+)");
        //if (match.Success)
        //    sr.story_id = match.Groups[0].ToString();
        //else
        //    sr.story_id = "0";
        sr.story_id = "17";

        sr.comments = new List<ShackPost>();

        results.Add(sr);

    }
    private void ServePageAsXML()
    {
        Response.ContentType = "text/xml";
        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("results");

        writer.WriteAttributeString("page", page);
        writer.WriteAttributeString("last_page", totalPages);
        writer.WriteAttributeString("total_results", totalResults);
        writer.WriteAttributeString("search_term", Server.UrlEncode(searchTerms));

        foreach (var item in results)
        {
            writer.WriteStartElement("result");
            writer.WriteAttributeString("author", item.author);
            writer.WriteAttributeString("date", item.date);
            writer.WriteAttributeString("story_name", item.story_name);
            writer.WriteAttributeString("id", item.id);
            writer.WriteAttributeString("story_id", item.story_id);
            writer.WriteElementString("body", item.preview);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();

    }

    private void ServerPageAsXML2()
    {
        Response.ContentType = "text/xml";
        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();

        writer.WriteStartElement("comments");
        writer.WriteAttributeString("terms", Server.UrlEncode(searchTerms));
        writer.WriteAttributeString("author", author);
        writer.WriteAttributeString("parent_author", parent_author);

        foreach (var item in results)
        {
            writer.WriteStartElement("comment");
            writer.WriteAttributeString("preview", Server.HtmlEncode(item.preview));
            writer.WriteAttributeString("story_name", item.story_name);
            writer.WriteAttributeString("date", item.date);
            writer.WriteAttributeString("id", item.id);
            writer.WriteAttributeString("story_id", item.story_id);
            writer.WriteAttributeString("author", item.author);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();
    }

    private void ServePageAsJSON2()
    {
        Response.ContentType = "application/json";

        StringBuilder sb = new StringBuilder();

        JavaScriptSerializer js = new JavaScriptSerializer();

        JsonSearchResult json = new JsonSearchResult();
        json.comments = results;

        if (author.Length == 0)
            json.author = null;
        else
            json.author = Server.UrlEncode(author);

        if (parent_author == null || parent_author.Length == 0)
            json.parent_author = null;
        else
            json.parent_author = Server.UrlEncode(parent_author);

        if (searchTerms == null || searchTerms.Length == 0)
            json.terms = null;
        else
            json.terms = Server.UrlEncode(searchTerms);

        json.last_page = totalPages;

        string jsonPosts = js.Serialize(json);

        // TODO: I'm assuming squeegy is placing an array of posts in the <comments>  part of XML, if so that explains
        //       the blank array in his json output, for now I'm going to fake mimic this.
        //jsonPosts = jsonPosts.Replace("null", "[]");

        Response.Write(jsonPosts);
    }

}


