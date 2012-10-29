using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for URLRewriteModule
/// </summary>
/// 
namespace ShackApiNet.Modules
{

    public class UrlRewriteModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication applicationInstance = (HttpApplication)sender;
            string url = applicationInstance.Request.RawUrl;


            Match match;

            // TODO: Refactor this to something a little nicer ugly..

            // auth/auth.xml
            match = Regex.Match(url, "auth.xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/auth/Default.aspx");
                return;
            }

            // auth/auth.json
            match = Regex.Match(url, "auth.json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/auth/Default.aspx?json=true");
                return;
            }

            // image.xml
            match = Regex.Match(url, "images.xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/images/default.aspx");
                return;
            }

            // image.json
            match = Regex.Match(url, "images.json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/images/default.aspx?json=true");
                return;
            }

            // ~/thread/{threadid}.xml
            match = Regex.Match(url, "thread/([0-9].*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/thread/Default.aspx?threadid={0}", match.Groups[1].Value));
                return;
            }

            match = Regex.Match(url, "thread/([0-9].*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/thread/Default.aspx?threadid={0}&json=true", match.Groups[1].Value));
                return;
            }

            // ~/{storyid}.{page}.xml  
            match = Regex.Match(url, @"([0-9].*)\.([0-9].*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/Default.aspx?storyid={0}&page={1}", match.Groups[1].Value, match.Groups[2].Value));
                return;
            }

            // ~/{storyid}.{page}.json  
            match = Regex.Match(url, @"([0-9].*)\.([0-9].*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/Default.aspx?storyid={0}&page={1}&json=true", match.Groups[1].Value, match.Groups[2].Value));
                return;
            }

            // ~/users/{username}.xml
            match = Regex.Match(url, "users/(.*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/users/Default.aspx?username={0}", match.Groups[1].Value));
                return;
            }

            // ~/users/{username}.json
            match = Regex.Match(url, "users/(.*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/users/Default.aspx?username={0}&json=true", match.Groups[1].Value));
                return;
            }

            // ~/user/{username}.json
            match = Regex.Match(url, "user/(.*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/user/Default.aspx?username={0}&json=true", match.Groups[1].Value));
                return;
            }

            // ~/postcount/{username}.xml
            match = Regex.Match(url, "postcount/(.*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/postcount/Default.aspx?Author={0}", match.Groups[1].Value));
                return;
            }

            // ~/postcount/{username}.json
            match = Regex.Match(url, "postcount/(.*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/postcount/Default.aspx?Author={0}&json=true", match.Groups[1].Value));
                return;
            }


            // ~/stories.xml
            match = Regex.Match(url, "stories.xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/stories.aspx");
                return;
            }

            // ~/stories.json
            match = Regex.Match(url, "stories.json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/stories.aspx?json=true");
                return;
            }

            // ~/stories/{storyid}.xml
            match = Regex.Match(url, "stories/([0-9].*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/stories/default.aspx?storyid={0}", match.Groups[1].Value));
                return;
            }

            // ~/stories/{storyid}.json
            match = Regex.Match(url, "stories/([0-9].*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath(string.Format("~/stories/default.aspx?storyid={0}&json=true", match.Groups[1].Value));
                return;
            }

            // ~/search.xml or 
            match = Regex.Match(url, "search.xml(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string author = HttpContext.Current.Request.QueryString["author"];
                string parentAuthor = HttpContext.Current.Request.QueryString["parent_author"];
                string terms = HttpContext.Current.Request.QueryString["terms"];

                applicationInstance.Context.RewritePath(
                    String.Format("~/search/Default.aspx?SearchTerm={0}&Author={1}&ParentAuthor={2}&version=2",
                    HttpContext.Current.Server.UrlEncode(terms),
                    HttpContext.Current.Server.UrlEncode(author),
                    HttpContext.Current.Server.UrlEncode(parentAuthor)));
            }
            // ~/search.json or 
            match = Regex.Match(url, "search.json(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string author = HttpContext.Current.Request.QueryString["author"];
                string parentAuthor = HttpContext.Current.Request.QueryString["parent_author"];
                string terms = HttpContext.Current.Request.QueryString["terms"];
                string page = HttpContext.Current.Request.QueryString["page"];

                applicationInstance.Context.RewritePath(
                    String.Format("~/search/Default.aspx?SearchTerm={0}&Author={1}&ParentAuthor={2}&page={3}&version=2&json=true",
                    HttpContext.Current.Server.UrlEncode(terms),
                    HttpContext.Current.Server.UrlEncode(author),
                    HttpContext.Current.Server.UrlEncode(parentAuthor),
                    HttpContext.Current.Server.UrlEncode(page)));

                return;
            }

            // ~/messages.xml - send
            match = Regex.Match(url, "send/messages.xml(.*)", RegexOptions.IgnoreCase);
            if (match.Success
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["to"])
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["subject"])
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["body"]))
            {
                applicationInstance.Context.RewritePath(
                    String.Format("~/messages/send/default.aspx?to={0}&subject={1}&body={2}&version=2",
                    HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Form["to"]),
                    HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Form["subject"]),
                    HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Form["body"])));


                return;
            }
            // ~/messages.json - send
            match = Regex.Match(url, "send/messages.json(.*)", RegexOptions.IgnoreCase);
            if (match.Success
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["to"])
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["subject"])
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["body"]))
            {
                applicationInstance.Context.RewritePath(
                    String.Format("~/messages/send/default.aspx?to={0}&subject={1}&body={2}&version=2",
                    HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Form["to"]),
                    HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Form["subject"]),
                    HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Form["body"])));

                return;
            }



            // ~/messages/{messageid}.xml - mark read
            match = Regex.Match(url, "messages/([0-9].*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/messages/read/default.aspx?messageid=" + match.Groups[1].Value);
                return;
            }
            // ~/messages/{messageid}.json - mark read
            match = Regex.Match(url, "messages/([0-9].*).json", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/messages/read/default.aspx?messageid=" + match.Groups[1].Value);
                return;
            }

            // ~/messages/messages.xml  
            match = Regex.Match(url, "messages/(default.aspx|messages.xml)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/messages/default.aspx");
                return;
            }

            // ~/messages/messages.json  
            match = Regex.Match(url, "messages/(default.aspx|messages.json)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/messages/default.aspx?json=true");
                return;
            }


            // ~/messages.xml  
            match = Regex.Match(url, "messages.xml(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                applicationInstance.Context.RewritePath("~/messages/default.aspx?version=2");
                return;
            }

            // ~/messages.json  
            match = Regex.Match(url, "messages.json(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string page = HttpContext.Current.Request.QueryString["page"];

                applicationInstance.Context.RewritePath(
                    String.Format("~/messages/default.aspx?&page={0}&json=true&version=2",
                    HttpContext.Current.Server.UrlEncode(page)));

                return;
            }

            // ~/index.xml
            match = Regex.Match(url, "index.xml", RegexOptions.IgnoreCase);
            if (match.Success)
                applicationInstance.Context.RewritePath("~/default.aspx");

            // ~/index.json
            match = Regex.Match(url, "index.json", RegexOptions.IgnoreCase);
            if (match.Success)
                applicationInstance.Context.RewritePath("~/default.aspx?json=true");

            // ~/storyid.xml or 
            match = Regex.Match(url, "([0-9].*).xml", RegexOptions.IgnoreCase);
            if (match.Success)
                applicationInstance.Context.RewritePath(String.Format("~/Default.aspx?storyid={0}", match.Groups[1].Value));

            // ~/story.json
            match = Regex.Match(url, "([0-9].*).json$", RegexOptions.IgnoreCase);
            if (match.Success)
                applicationInstance.Context.RewritePath(String.Format("~/Default.aspx?storyid={0}&json=true", match.Groups[1].Value));

        }
    }
}