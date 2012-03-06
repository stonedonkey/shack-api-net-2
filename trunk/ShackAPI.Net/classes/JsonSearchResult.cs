using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class JsonSearchResult
{

    public List<SearchResult> comments { get; set; }
    public string terms { get; set; }
    public string parent_author { get; set; }
    public string author { get; set; }
}
