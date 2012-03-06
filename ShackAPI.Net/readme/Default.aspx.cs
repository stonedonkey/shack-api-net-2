using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class readme_Default : System.Web.UI.Page
{

  public int _activeCache = 0;
  protected void Page_Load(object sender, EventArgs e)
  {

    if (Application["PostCache"] != null)
    {
      Hashtable postsHash = (Hashtable)Application["PostCache"];
      _activeCache = postsHash.Count -1;
      if (_activeCache < 0)
        _activeCache = 0;
    }


  }
}
