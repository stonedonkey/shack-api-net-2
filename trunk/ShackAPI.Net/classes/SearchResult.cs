using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SearchResult
/// </summary>
public class SearchResult
{

    private string _posterName;
    private string _postPreview;
    private string _threadTitle;
    private string _datePosted;
    private string _threadID;
    private string _storyID;

    
    public string reply_count { get; set; }
    public string body { get; set; }
    public List<ShackPost> comments { get; set; }
    public string last_reply_id { get; set; }

    public string preview
    {
        get { return _postPreview; }
        set { _postPreview = value; }
    }
    public string story_name
    {
        get { return _threadTitle; }
        set { _threadTitle = value; }
    }
    public string story_id
    {
        get { return _storyID; }
        set { _storyID = value; }
    }

    public string date
    {
        get { return _datePosted; }
        set { _datePosted = value; }
    }

    public string category { get; set; }


    public string author
    {
        get { return _posterName; }
        set { _posterName = value; }
    }

    public string id
    {
        get { return _threadID; }
        set { _threadID = value; }
    }


    public SearchResult()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}
