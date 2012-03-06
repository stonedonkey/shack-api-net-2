<%@ Application Language="C#" %>
<script runat="server">

    void Application_BeginRequest(object sender, EventArgs e)
    {
        var app = (HttpApplication)sender;
        if (app.Context.Request.Url.LocalPath.EndsWith("/"))
        {
            app.Context.RewritePath(
                     string.Concat(app.Context.Request.Url.LocalPath, "default.aspx"));
        }
    }


    void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup

    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    {

      ////get reference to the source of the exception chain
      Exception ex = Server.GetLastError().GetBaseException();


      //System.Diagnostics.EventLog log = new System.Diagnostics.EventLog();
      //log.Log = "Application";
      //log.Source = "shack-api-net";
      //log.WriteEntry(ex.Message,System.Diagnostics.EventLogEntryType.Error );
      
      

      //Application["ex"] = ex;
      
      //Response.Redirect("~/error.aspx");

      //Server.ClearError();      
      

    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }

</script>
