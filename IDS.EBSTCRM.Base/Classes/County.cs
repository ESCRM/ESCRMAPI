using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// County details (NOT an organisation part)
    /// </summary>
    [Serializable()]
    public class County : EventLogBase
    {
        private int Id;
        private string Name="";
        private string Url="";
        private string Address="";
        private string Zip="";


	    public string zip
	    {
		    get { return Zip;}
		    set { Zip = value;}
	    }
	

	    public string address
	    {
		    get { return Address;}
		    set { Address= value;}
	    }
	
	    public string url
        {
            get {return Url;}
            set {Url=value;}
        }

	    public string name
	    {
		    get { return Name;}
		    set { Name= value;}
	    }
	
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }

        public int NNECode { get; set; }

        public County()
        { 
        }
	
        public County(ref SqlDataReader dr)
        {
            Populate(ref dr);
        }

        private void Populate(ref SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["Id"]);
            Name = TypeCast.ToString(dr["Name"]);
            Url = TypeCast.ToString(dr["Url"]);
            Address = TypeCast.ToString(dr["Address"]);
            Zip = TypeCast.ToString(dr["Zip"]);
            NNECode = TypeCast.ToInt(dr["NNECode"]);
        }


        public override VisualItems GetVisualItemsForEventLog(string Event)
        {
            VisualItems retval = new VisualItems();

            retval.Text = eventToText(Event) + " kommune " + Name;
            retval.Icon = "images/listviewIcons/county.png";

            retval.JavaScript = (Event == "DELETE" ?
                                    "alert('Kommunen er blevet slettet');"
                                    :
                                    "top.frames['root'].frames['frameGlobalAdmin'].counties_EditThisCounty(" + Id + ");");



            return retval;
        }
    }
}
