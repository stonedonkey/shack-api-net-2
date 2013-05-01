using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using ICSharpCode.SharpZipLib.GZip;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


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
        client.Headers["X-Requested-With"] = "XMLHttpRequest";
        //client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        //client.Headers["Referer"] = "https://www.shacknews.com/login/login";
        

        try
        {
            NameValueCollection c = new NameValueCollection();
            c.Add("user-identifier", "latestchatty");
            c.Add("supplied-pass", "8675309");
            c.Add("get_fields%5B%5D", "result");
            c.Add("remember-login", "1");

            // get_fields%5B%5D=result&user-identifier=USERNAME&supplied-pass=PASSWORD&remember-login=1 

            client.Cookies = cc;

            string urlCookie = "https://www.shacknews.com/account/signin";
            
            Byte[] webResponse = client.UploadValues(urlCookie, "POST", c);
            String result = Encoding.ASCII.GetString(webResponse);

            if (result.Contains("{\"result\":{\"valid\":\"true\""))
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
