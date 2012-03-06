using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using HtmlAgilityPack;
using System.Xml;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

public partial class postcount_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string filterByUser = Request["Author"];

        string url = String.Format("http://www.shacknews.com/search?chatty=1&type=4&chatty_term=&chatty_user={0}&chatty_author=&chatty_filter=all&start=999999", Server.UrlEncode(filterByUser));

        //int totalPages = 1;
        int totalPosts = 0;

        try
        {
            WebClientExtended client = new WebClientExtended();
            client.Method = "GET";
            client.Encoding = Encoding.UTF8;
            String shackHTML = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(shackHTML);


            // try and get the number of pages for this story
            try
            {
                string resultText = doc.DocumentNode.SelectSingleNode("//h2[@class='search-num-found']").InnerText.Replace(",", "");
                Match match = Regex.Match(resultText, @"([\d]+)");
                if (match.Success)
                    totalPosts = int.Parse(match.Groups[0].ToString());
                else
                    totalPosts = 0;
            }
            catch (Exception)
            {

                totalPosts = 0;
            }


            //totalPosts = ((totalPages - 1) * 15);

            //if (totalPages > 0)
            //{
            //    url = String.Format("http://www.shacknews.com/search?chatty=1&type=4&chatty_term=&chatty_user={0}&chatty_author=&chatty_filter=all&start={1}", Server.UrlEncode(filterByUser), totalPages);

            //    shackHTML = client.DownloadString(url);

            //    doc = new HtmlAgilityPack.HtmlDocument();
            //    doc.LoadHtml(shackHTML);

            //    //<li class="result chatty">
            //    int lastPageResults = doc.DocumentNode.SelectNodes("//li[@class='result chatty']").Count();

            //    totalPosts += lastPageResults;
            //}


        }
        catch
        {

        }



        if (string.IsNullOrEmpty(Request.QueryString["json"]))
            ServePageAsXML(totalPosts, filterByUser);
        else
            ServePageAsJSON(totalPosts, filterByUser);



    }
    private void ServePageAsXML(int postCount, string userName)
    {
        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("posts");
        writer.WriteAttributeString("user", userName);
        writer.WriteAttributeString("count", postCount.ToString());
        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();
        writer.Close();

    }
    private void ServePageAsJSON(int postCount, string userName)
    {
        Response.ContentType = "application/json";
        Encoding utf8 = new UTF8Encoding(false);

        PostCount pc = new PostCount();
        pc.user = userName;
        pc.count = postCount;

        JavaScriptSerializer js = new JavaScriptSerializer();
        string jsonPosts = js.Serialize(pc);
        Response.Write(jsonPosts);
    }

    private class PostCount
    {

        public string user { get; set; }
        public int count { get; set; }
    }

}
