using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;

public enum OutputFormats
{
    XML,
    JSON
}

public static class Helpers
{


    public static string FormatShackDate(string convertDate, OutputFormats format)
    {
        try
        {
            TimeZone tz = TimeZone.CurrentTimeZone;
            DateTime datePosted = DateTime.ParseExact(convertDate, "MMM dd, yyyy h:mmtt CST", CultureInfo.InvariantCulture);
            if (tz.IsDaylightSavingTime(DateTime.Now) == true)
                datePosted = datePosted.AddHours(-1);

            String dateout;
            if (format == OutputFormats.XML)
                dateout = String.Format("{0:ddd MMM dd HH:mm:00 -0700 yyyy}", datePosted);
            else
                dateout = String.Format("{0:yyyy/MM/dd HH:mm:00 -0700}", datePosted);

            return dateout;
        }
        catch
        {
        }

        return convertDate;

    }
}
