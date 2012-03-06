using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Xml;

public partial class Auth : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        Response.Expires = 0;

        int version = 1;
        int.TryParse(Request.QueryString["version"], out version);

        String userName = "";
        string password = "";

        string headers = Context.Request.Headers["Authorization"];
        if (headers.Length > 7)
        {
            string ticket = headers.Substring(6);
            string clearTicket = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ticket));

            password = clearTicket.Substring(clearTicket.LastIndexOf(":") + 1);
            userName = clearTicket.Substring(0, clearTicket.LastIndexOf(":"));

            if (userName.Contains("\\") && !userName.EndsWith("\\"))
                userName = userName.Substring(userName.LastIndexOf("\\") + 1);


        }

        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";

        try
        {
            NameValueCollection c = new NameValueCollection();
            c.Add("username", userName);
            c.Add("password", password);
            c.Add("uri", "/");
            client.Cookies = cc;
            string urlCookie = "http://www.shacknews.com/login.x";
            Byte[] webResponse = client.UploadValues(urlCookie, "POST", c);
            String result = Encoding.UTF8.GetString(webResponse);

            if (result.Contains("ERROR: Login failed - Username or password is incorrect"))
            {
                //throw New HttpException(401, "HTTP Basic: Access denied.");
                //throw new HttpException(401, "HTTP Basic: Access denied.");

                //Response.Write("error_login_failed");
                Response.Clear();
                Response.StatusCode = 401;
                Response.End();
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(Request.QueryString["json"]))
                    ServePageAsXML();
                else
                    ServePageAsJSON();
            }

        }
        catch (Exception)
        {
            //throw new HttpException(401, "HTTP Basic: Access denied.");
            Response.Clear();
            Response.StatusCode = 401;
            Response.End();
            //Response.Write("error_communication_authentication");
            return;

        }

    }

    private void ServePageAsJSON()
    {
        Response.ContentType = "application/json";
        Response.Write("{\"authentication\":{\"success\":true}}");
    }
    private void ServePageAsXML()
    {
        Response.ContentType = "text/xml";

        Encoding utf8 = new UTF8Encoding(false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8);
        writer.Formatting = System.Xml.Formatting.Indented;

        writer.WriteStartDocument();
        writer.WriteStartElement("autentication");

        writer.WriteStartElement("success");
        writer.WriteStartElement("true");
        writer.WriteEndElement();
        
        writer.WriteEndElement();
        writer.WriteEndDocument();

        writer.Flush();
        writer.Close();

    }
}
