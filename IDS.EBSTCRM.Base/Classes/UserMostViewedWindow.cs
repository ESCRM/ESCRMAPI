using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// GUI Relevant class:
    /// Holds how many times a given Web Windows has been displayed.
    /// </summary>
    public class UserMostViewedWindow
    {
        public int Id;
        public string UserId;
        public string URL;
        public string Title;
        public string Icon;
        public int Count;

        public int Width;
        public int Height;

        public bool Minimizeable;
        public bool Maximizeable;
        public bool Closeable;
        public bool Resizeable;


        public UserMostViewedWindow()
        {
        }

        public UserMostViewedWindow(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            UserId = TypeCast.ToString(dr["UserId"]);
            URL = TypeCast.ToString(dr["URL"]);
            Title = TypeCast.ToString(dr["Title"]);
            Icon = TypeCast.ToString(dr["Icon"]);
            Count = TypeCast.ToInt(dr["Count"]);

            Width = TypeCast.ToInt(dr["Width"]);
            Height = TypeCast.ToInt(dr["Height"]);

            Minimizeable = TypeCast.ToBool(dr["Minimizeable"]);
            Maximizeable = TypeCast.ToBool(dr["Maximizeable"]);
            Closeable = TypeCast.ToBool(dr["Closeable"]);
            Resizeable = TypeCast.ToBool(dr["Resizeable"]);
        }
    }
}
