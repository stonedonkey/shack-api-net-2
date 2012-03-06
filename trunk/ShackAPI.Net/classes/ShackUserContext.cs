using System.Web;
using System.Net;
public sealed class ShackUserContext
{
    private ShackUserContext() { }
    private static ShackUserContext Instance = new ShackUserContext();
    private CookieContainer _cookieContainer;

    public static ShackUserContext Current
    {
        get
        {
            //ShackUserContext instance;
            if (HttpContext.Current.Cache["ShackUserContext"] == null)
            {
                Instance = new ShackUserContext();
                HttpContext.Current.Cache["ShackUserContext"] = Instance;
            }
            else
            {
                Instance = (ShackUserContext)HttpContext.Current.Cache["ShackUserContext"];

            }
            return Instance;
        }

    }

    #region Member Variables


    public CookieContainer CookieContainer
    {
        get { return _cookieContainer; }
        set
        {
            _cookieContainer = value;
           // this.UpdateSession();
        }


    #endregion
    }
    private void UpdateSession()
    {
       // HttpContext.Current.Cache["ShackUserContext"] = this;
    }
}

//private WebClientExtended _client;

//public WebClientExtended Client
//{
//    get
//    {
//        return _client;

//    }
//    set
//    {
//        _client = value;
//        this.UpdateSession(); // update the http session
//    }
//}

//#endregion



