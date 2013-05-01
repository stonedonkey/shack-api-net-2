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

public partial class user_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        string userName = Request["username"];


        string url = string.Format("http://www.shacknews.com/api/users/{0}.json", Server.HtmlEncode(userName)); 
        //http://www.shacknews.com/api/users/cgee.json

        try
        {

            String shackHTML;
            using (WebClientExtended client = new WebClientExtended())
            {
                client.Method = "GET";

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
            }

            Response.ContentType = "application/json";
            Response.Write(shackHTML);
        }
        catch
        {
            Response.Write(String.Format("{{\"error\":\"{0}\"}}", "error getting user"));
        }

    }
}
