using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ShackPost
/// </summary>
public class ShackPost
{
    public List<ShackPost> comments { get; set; }
    public List<Participants> participants { get; set; }
    public string preview { get; set; }
    public string category { get; set; }
    public string body { get; set; }
    public string date { get; set; }
    public string author { get; set; }
    public string reply_count{ get; set; }
    public string id { get; set; }
    public string last_reply_id{ get; set; }
}

public class Participants
{
    public string username { get; set; }
    public int post_count { get; set; }
}
