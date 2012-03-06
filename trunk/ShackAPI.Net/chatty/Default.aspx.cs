using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text;

public partial class chatty_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        

        HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create("http://www.shacknews.com/latestchatty.x");
        loHttp.ContentType = "application/x-www-form-urlencoded";
        loHttp.Method = "GET";
        loHttp.AllowAutoRedirect = false;

        HttpWebResponse loWebResponse = (HttpWebResponse)loHttp.GetResponse();
        string url = loWebResponse.Headers["Location"];

        Match match = Regex.Match(url, @"story=([0-9]*)");

        string id = "";
        if (match.Success)
            id = match.Groups[1].ToString();

        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("story");
        writer.WriteAttributeString("current_chatty", id);

        writer.WriteEndDocument();

        writer.Flush();

        loWebResponse.Close();
        loWebResponse = null;

    }
}
