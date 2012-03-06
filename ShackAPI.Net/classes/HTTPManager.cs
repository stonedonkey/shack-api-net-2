using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using ICSharpCode.SharpZipLib.GZip;
using System.IO;
using System.Collections.Specialized;
using System.Text;


/// <summary>
/// Summary description for HTTPManagement
/// </summary>
public class HTTPManager
{
    public static string GetURLWithGzip(string url)
    {
        System.Net.WebClient client = new WebClient();
        client.Headers.Add("Accept-Encoding", "gzip,deflate");
        Byte[] b = client.DownloadData(url);

        GZipInputStream gz = new GZipInputStream(new MemoryStream(b));

        Byte[] unzipBytes = new Byte[2048];
        int sizeRead;

        MemoryStream outputStream = new MemoryStream();
        while (true)
        {
            sizeRead = gz.Read(unzipBytes, 0, 2048);
            if (sizeRead > 0)
                outputStream.Write(unzipBytes, 0, 2048);
            else
                break;
        }
        return System.Text.Encoding.UTF8.GetString(outputStream.ToArray());

    }
    public static void SetShackUserContext()
    {
        WebClientExtended client = new WebClientExtended();
        CookieContainer cc = new CookieContainer();
        client.Method = "POST";
        client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.2.13) Gecko/20101203 Firefox/3.6.13 ( .NET CLR 3.5.30729; .NET4.0E)";
        //client.Headers["x-requested-with"] = "XMLHttpRequest";
        //client.Headers["Referer"] = "http://www.shacknews.com";

        try
        {
            NameValueCollection c = new NameValueCollection();
            c.Add("email", "latestchatty");
            c.Add("password", "8675309");
            c.Add("login", "login");
            
            client.Cookies = cc;

            string urlCookie = "http://www.shacknews.com/";
            Byte[] webResponse = client.UploadValues(urlCookie, "POST", c);
            String result = Encoding.ASCII.GetString(webResponse);

            if (result.Contains("/user/latestchatty/posts"))
            {
                ShackUserContext.Current.CookieContainer = client.Cookies;
                return;               
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message.ToString());
        }
    }
}
