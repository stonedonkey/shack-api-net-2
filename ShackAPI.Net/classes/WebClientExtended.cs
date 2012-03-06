
//THUG LIFE! from : http://channel9.msdn.com/forums/TechOff/162017-Using-WebClient-to-enter-Form-based-Auth-system-How/
using System.Net;
using System;
public class WebClientExtended : WebClient
{
    private CookieContainer myContainer;
    private HttpWebRequest myRequest;
    private string myMethod;

    public string login { get; set; }
    public string password { get; set; }


    public string Method
    {
        get { return myMethod; }
        set { myMethod = value; }
    }

    public CookieContainer Cookies
    {
        get
        {
            if (myContainer == null)
            {
                myContainer = new CookieContainer();
            }

          

            return myContainer;
        }
        set
        {
            myContainer = value;
        }
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        myRequest = (HttpWebRequest)base.GetWebRequest(address);
        myRequest.Method = this.Method;
        myRequest.CookieContainer = Cookies;

        CredentialCache cache = new CredentialCache();
        if ((login != null && login.Length > 0) && password != null && password.Length > 0)
            cache.Add(address, "Basic", new NetworkCredential(login, password));
        else
            cache.Add(address, "Basic", new NetworkCredential("latestchatty", "8675309"));

        myRequest.Credentials = cache;


        return myRequest;
    }

    protected override WebResponse GetWebResponse(WebRequest request)
    {
        return myRequest.GetResponse();
    }

    protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
    {
        return myRequest.EndGetResponse(result);
    }
}