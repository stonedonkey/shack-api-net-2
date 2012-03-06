using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class JsonComments
{
    public string page { get; set; }
    public List<ShackPost> comments { get; set; }
    //public List<Participants> participants { get; set; }
    public string story_name { get; set; }
    public string story_id { get; set; }
    public string last_page { get; set; }
}