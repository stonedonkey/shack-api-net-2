// BasicAuthenticationModule.cs
//
// This HTTP Module is designed to support HTTP Basic authentication,
// without using the built-in IIS implementation.  The IIS implementation
// can only authenticate against the Active Directory store; but in
// many applications, one would rather authenticate against a separate
// database.
//
// The implementation was designed particularly for web services, but 
// should suffice for any web application.  For a non-service application,
// one obvious change would be to support a redirection on a failed login,
// to display a more friendly message to the user.
//
// The credential store in this version is a simple XML file (sample in
// users.xml).  In a real application, you would probably want to modify
// this to use a database or LDAP store.  An easy way to do this would be
// to derive from Rassoc.BasicAuthenticationModule and override the 
// AuthenticateUser function.
//
// Usage:
//
// (Assuming ASP.NET) 
// 1. Copy BasicAuthMod.dll to your ASP.NET application's bin directory.
// 2. Make the following changes to your web.config file (within <system.web>):
//     - change authentication line to: <authentication mode="None" /> 
//     - add an authorization section if you wish, such as
//         <authorization>
//           <deny users="?" />
//         </authorization>
//     - add the following lines:
//         <httpModules>
//           <add name="BasicAuthenticationModule" 
//                type="Rassoc.Samples.BasicAuthenticationModule,BasicAuthMod" />
//         </httpModules>   
// 3. Add the following to your web.config (within <configuration>):
//         <appSettings>
//           <add key="Rassoc.Samples.BasicAuthenticationModule_Realm" value="RassocBasicSample" />
//           <add key="Rassoc.Samples.BasicAuthenticationModule_UserFileVpath" value="~/users.xml" />
//         </appSettings>
//
//
// Greg Reinacker
// Reinacker & Associates, Inc.
// http://www.rassoc.com
// http://www.rassoc.com/gregr/weblog/
//

using System;
using System.Configuration;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Xml;

namespace ShackApiNet.Autentication
{
    public class BasicAuthenticationModule : IHttpModule
    {
        public BasicAuthenticationModule()
        {
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += new EventHandler(this.OnAuthenticateRequest);
            application.EndRequest += new EventHandler(this.OnEndRequest);
        }

        public void OnAuthenticateRequest(object source, EventArgs eventArgs)
        {
            HttpApplication app = (HttpApplication)source;

            string authStr = app.Request.Headers["Authorization"];

            if (authStr == null || authStr.Length == 0)
            {
                // No credentials; anonymous request
                return;
            }

            authStr = authStr.Trim();
            if (authStr.IndexOf("Basic", 0) != 0)
            {
                // Don't understand this header...we'll pass it along and 
                // assume someone else will handle it
                return;
            }

            string encodedCredentials = authStr.Substring(6);

            byte[] decodedBytes = Convert.FromBase64String(encodedCredentials);
            string s = new ASCIIEncoding().GetString(decodedBytes);

            string[] userPass = s.Split(new char[] { ':' });
            string username = userPass[0];
            string password = userPass[1];

          
            if (AuthenticateUser(app, username, password))
            {
                app.Context.User = new GenericPrincipal(new GenericIdentity(username, "Rassoc.Samples.Basic"),null);
            }
            else
            {
                // Invalid credentials; deny access
                DenyAccess(app);
                return;
            }
        }

        public void OnEndRequest(object source, EventArgs eventArgs)
        {
            // We add the WWW-Authenticate header here, so if an authorization 
            // fails elsewhere than in this module, we can still request authentication 
            // from the client.
            HttpApplication app = (HttpApplication)source;
            if (app.Response.StatusCode == 401)
            {
                string realm = ConfigurationManager.AppSettings["Rassoc.Samples.BasicAuthenticationModule_Realm"];
                string val = String.Format("Basic Realm=\"{0}\"", realm);
                app.Response.AppendHeader("WWW-Authenticate", val);
            }
        }

        private void DenyAccess(HttpApplication app)
        {

            app.Response.StatusCode = 401;
            app.Response.StatusDescription = "Access Denied";

            // Write to response stream as well, to give user visual 
            // indication of error during development
            app.Response.Write("401 Access Denied");

            app.CompleteRequest();
        }

        protected virtual bool AuthenticateUser(HttpApplication app, string username, string password)
        {
            // we always return true
            return true;
        }




    }
}
