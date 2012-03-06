using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using System.Net;

public partial class ShackPicsTest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            username.Text = "latestchatty";
            password.Text = "8675309";
        }

    }
    protected void ButtonSubmit_Click(object sender, EventArgs e)
    {
        Byte[] b = new byte[filename.PostedFile.ContentLength];
        filename.PostedFile.InputStream.Read(b, 0, b.Length);
        string base64String = System.Convert.ToBase64String(b, 0, b.Length);

        using (WebClient client = new WebClient())
        {
            //string loginUrl = "http://localhost:1073/shack-api-net/images.json";
            string loginUrl = "http://shackapi.stonedonkey.com/images.json";
            //string loginUrl = "http://www.shackchatty.com/images";
            

            NameValueCollection c = new NameValueCollection();
            c.Add("username", username.Text);
            c.Add("password", password.Text);
            c.Add("filename", "testupload.jpg");
            c.Add("image", base64String);

            Byte[] webResponse = client.UploadValues(loginUrl, "POST", c);
            String result = Encoding.ASCII.GetString(webResponse);

            Response.Write(result);

        }

        
    }


}
