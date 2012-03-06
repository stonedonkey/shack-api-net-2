<%@ Page Language="C#" AutoEventWireup="true" Inherits="readme_Default" Codebehind="Default.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>shack-api-net readme</title>
  <link href="../css/main.css" rel="stylesheet" type="text/css" />
</head>
<body>
  <form id="form1" runat="server">
  <div>
    <div id="content">
      <h1>
        shack-api-net</h1>
      
       
        shakck-api-net is a .NET version of <a target="_blank" href="http://shackchatty.com/readme">http://shackchatty.com/readme</a> created by SqueegyTBS for ShackNews.com.  The
        API mimics the output his API written with Ruby.<br /><br />
        
        <h2>Chatty and Thread API</h2>
        
        <div class="url">
        GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %> <br />
        GET:  <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>index.xml
        </div>
        
        Retrieve a list of page 1 root posts in the latest chatty. This does not get the children of those posts, only the root posts. 
        
        <div class="url">
          GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>[story_id].xml
        </div>
        Retrieve a list of page 1 root posts in the news story with a specific story_id. This does not get the children of those posts, only the root posts. 
        
        <div class="url">
         GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>[story_id].[page_number].xml
        </div>
        Retrieve a list of root posts in the news story with a specific story_id on page page_number. This does not get the children of those posts, only the root posts. 
    
        <div class="url">
         GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>thread/[root_post_id].xml
        </div>
        Get the entire tree of posts for a thread with has a root post with a specific root_post_id. This gets all children of those posts with full content for the entire tree. 
        <br />
        
        <h2>Stories</h2>
        <div class="url">
        GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>stories.xml
        </div>
        Returns a listing of the current stories showing on the front page of ShackNews.com
        
        
        <div class="url">
         GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>stories/[story_id].xml
        </div>
        Returns all the information relating to a specific story as well as all the content for that story.
        
        
        <h2>Users</h2>
        <div class="url">
        GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>users/[username].xml
        </div>
        Returns public data for a single user.
        
        
        <h2>Search API</h2>
        <div class="url">
        GET:  <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Search/?[parameter_list]<BR />
        POST: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Search/
        </div>
        Executes a search of the ShackNews comments only.  The request can either be a GET or POST with the following encoded variables.
        <ul>
          <li><b>SearchTerm</b> - the string your searching the comments for.</li>
          <li><b>Author</b> - filter search by a specific author.</li>
          <li><b>ParentAuthor</b> - filter search results by a Parent Author.</li>
          <li><b>SearchType</b> - the type of search your wish to execute (note these seem unreliable even on the actual site).  [ Default: all]
            <ul>  
            <li><b>all</b> = search all comments</li>
            <li><b>i</b> = informative comments</li>
            <li><b>n</b> = nws comments</li>
            </ul>
            </li>
            <li><b>Page</b> - the page of search results you wish to retrive. [ Default: 1 ]</li>
        </ul>
        A sample GET query might look like:<br /><br />
        <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Search/?SearchTerm=duke&Author=GeorgeB3DR
        
        
        <h2>ShackMessage API - Extended API</h2>
        
        <b>You must autenticate your request with HTTP Basic Autentication using the crentials of the user making the request.</b>
                
        <div class="url">
        GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Messages/?[parameter_list]<br />
        POST: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Messages/
        </div>
        Executes a query of a users shack messages.  Because the user must first autenticate with ShackNews, the API must handle
        and pass their login information.<br /><br />
        The request can either be a GET or POST with the following encoded variables.<br />
      <ul>
        <li><b>username</b> - the users shack username.</li>
        <li><b>password</b> - the users shack password.</li>
        <li><b>box</b> - the mail box within Shack Messages [ Default: inbox ]
          <ul>
            <li><b>inbox</b> = users current shack messages </li>
            <li><b>outbox</b> = users outgoing shack messages</li>
            <li><b>archive</b> = users archived shack messages</li>
          </ul>
        </li>
        <li><b>page</b> - the page of results you wish to view [ Default: 1 ]</li>
      </ul>
        
      <h4>Server Responses:</h4>
      The following are trapped error responses you may encounter.  The response is sent back as a simple text string.
      <h5>Authentication Errors</h5>
      These are typically problems logging in with the users credentials.
      <ul>
        <li><b>error_login_failed</b> - login failed on shack news.</li>
        <li><b>error_username_missing</b> - request is missing the users login</li>
        <li><b>error_password_missing</b> - request is missing the users password</li>
      </ul>
        <h5>Communication Errors</h5> These typically are problems with the API server reaching Shack News, IE Steve dropped a server.
       <ul>
        <li><b>error_communication_authentication</b> - there was an error reaching the login page on Shack News</li>
        <li><b>error_communication_inbox</b> - there was an error reaching the shack messages page on Shack News</li>
        </ul>
   
        
    <div class="url">
        GET: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Messages/Send?[parameter_list]<br />
        POST: <% HttpContext.Current.Response.Write(ConfigurationManager.AppSettings["siteURL"]); %>Messages/Send/
        </div>        
        Sends a Shack Message after authenticating the user on the login page at Shack News.<br /><br />
        The request can be either a GET or POST with the following encoded variables.
        <ul>
          <li><b>username</b> - the users shack username.</li>
          <li><b>password</b> - the users shack password.</li>
          <li><b>to</b> - the shack name of the user your sending the message to.</li>
          <li><b>subject</b> - the subject of the message.</li>
          <li><b>body</b> - the body of the message.</li>
        
        </ul>
        
      <h4>Server Responses:</h4>
      The following are trapped error responses you may encounter.  The response is sent back as a simple text string.
      <h5>Authentication Errors</h5>
      These are typically problems logging in with the users credentials, or with required items missing from your request.
      <ul>
        <li><b>error_login_failed</b> - login failed on shack news.</li>
        <li><b>error_username_missing</b> - request is missing the users login/</li>
        <li><b>error_password_missing</b> - request is missing the users password/</li>
        <li><b>error_message_to_missing</b> - missing the shack users name your sending to/</li>
        <li><b>error_message_subject_missing</b> - message subject is missing/</li>
        <li><b>error_message_body_missing</b> - message body is missing.</li>
      </ul>
      
       <h5>Communication Errors</h5> These typically are problems with the API server reaching Shack News, IE Steve dropped a server.
       <ul>
        <li><b>error_communication_authentication</b> - there was an error reaching the login page on Shack News</li>
        <li><b>error_communication_send</b> - there was an error during the sending of the message on Shack News</li>
      </ul>
        
                
       
    <br /><br /><br /><br /><br /><br />
  </div>
  </form>
</body>
</html>
