using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ShackMessage
/// </summary>
public class ShackMessage
{
  private string _name;
  private string _msgSubject;
  private string _msgDate;
  private string _msgText;
  private string _msgID;
  private string _messageStatus;

  public string unread
  {
    get { return _messageStatus; }
    set { _messageStatus = value; }
  }

  public string from
  {
    get { return _name; }
    set { _name = value; }
  }
  public string subject
  {
    get { return _msgSubject; }
    set { _msgSubject = value; }
  }
  public string date
  {
    get { return _msgDate; }
    set { _msgDate = value; }
  }
  public string body
  {
    get { return _msgText; }
    set { _msgText = value; }
  }
  public string id
  {
    get { return _msgID; }
    set { _msgID = value; }
  }


}
