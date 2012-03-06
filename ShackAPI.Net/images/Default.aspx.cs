using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using HtmlAgilityPack;
using System.Xml;
using System.IO;
using System.Drawing;

public partial class images_Default : System.Web.UI.Page
{
    private OutputFormats outputFormat = OutputFormats.XML;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(Request.QueryString["json"]))
            outputFormat = OutputFormats.XML;
        else
            outputFormat = OutputFormats.JSON;

        if (String.IsNullOrEmpty((Request["username"])))
            throw new Exception("Username not found");

        if (String.IsNullOrEmpty((Request["password"])))
            throw new Exception("Password not found");

        if (String.IsNullOrEmpty((Request["filename"])))
            throw new Exception("Filename not found");

        if (String.IsNullOrEmpty((Request["image"])))
            throw new Exception("Image not found");

        String username =Request["username"];
        String password = Request["password"];
        String filename = Request["filename"];
        String imageString = Request["image"];




        Byte[] bitmapData = new Byte[imageString.Length];
        bitmapData = Convert.FromBase64String(FixBase64ForImage(imageString));
        Stream streamBitmap = new System.IO.MemoryStream(bitmapData);

        String result;

        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";

        NameValueCollection c = new NameValueCollection();
        c.Add("user_name", "latestchatty");
        c.Add("user_password", "8675309");

        client.Cookies = cc;

        string loginUrl = "http://www.shackpics.com/users.x?act=login_go";
        Byte[] webResponse = client.UploadValues(loginUrl, "POST", c);
        result = Encoding.ASCII.GetString(webResponse);

        if (result.Contains("successfully")) // login worked
        {
            NameValueCollection imgC = new NameValueCollection();
            imgC.Add("type", "direct");

            String postUrl = "http://www.shackpics.com/upload.x";
            Uri url = new Uri(postUrl);

            WebResponse wr = Upload.PostFile(url, imgC, streamBitmap, filename, null, "userfile[]", client.Cookies, null);

            StreamReader reader = new StreamReader(wr.GetResponseStream());
            string str = "";
            while (reader.EndOfStream == false)
            {
                str = str + reader.ReadLine();
            }

            result = str;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);

            string filePath = "";
            if (doc.DocumentNode.SelectSingleNode("//input[@id='link11']") != null)
            {
                filePath = doc.DocumentNode.SelectSingleNode("//input[@id='link11']").GetAttributeValue("value", "").ToString();
            }
            else
                filePath = "";


            if (outputFormat == OutputFormats.JSON)
            {
                Response.ContentType = "application/json";
                if (filePath.Length > 0)
                    Response.Write(String.Format("{{\"success\":\"{0}\"}}", filePath));
                else
                    Response.Write(String.Format("{{\"error\":\"{0}\"}}", "error uploading file"));
            }
            else
            {
                Response.ContentType = "text/xml";
                Encoding utf8 = new UTF8Encoding(false);
                using (XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8))
                {
                    writer.Formatting = System.Xml.Formatting.Indented;
                    writer.WriteStartDocument();
                    if (filePath.Length > 0)
                        writer.WriteElementString("success", filePath);
                    else
                        writer.WriteElementString("error", "error uploading file");

                    writer.Flush();
                    writer.Close();

                }
            }
        }
        else
        {
            if (outputFormat == OutputFormats.JSON)
            {
                Response.ContentType = "application/json";
                Response.Write(String.Format("{{\"error\":\"{0}\"}}", "error logging in"));
            }
            else
            {
                Response.ContentType = "text/xml";
                Encoding utf8 = new UTF8Encoding(false);
                using (XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, utf8))
                {
                    writer.Formatting = System.Xml.Formatting.Indented;
                    writer.WriteStartDocument();

                    writer.WriteElementString("error", "error logging in");

                    writer.Flush();
                    writer.Close();

                }
            }
        }

    }


    public Bitmap ConvertBase64ToImage(string ImageText)
    {
        if (ImageText.Length > 0)
        {
            Byte[] bitmapData = new Byte[ImageText.Length];
            bitmapData = Convert.FromBase64String(FixBase64ForImage(ImageText));

            System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);

            Bitmap bitImage = new Bitmap((Bitmap)System.Drawing.Image.FromStream(streamBitmap));

            return bitImage;
        }
        else
            return null;
    }
    private string FixBase64ForImage(string Image)
    {
        System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);

        sbText.Replace("\r\n", String.Empty);

        sbText.Replace(" ", String.Empty);

        return sbText.ToString();
    }



}
public class CustomWebClient : WebClient
{
    private CookieContainer _cookies;

    public CustomWebClient(CookieContainer cookies)
    {
        _cookies = cookies;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
        request.CookieContainer = _cookies;
        return request;
    }
}