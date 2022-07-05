using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Container for GeoCodes grabbed from Google Maps
    /// </summary>
    [Serializable()]
    public class GeoCode
    {
        public int Id { get; set; }
        public string InputAddress { get; set; }
        public string FormattedAddress { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal ViewPortSWLat { get; set; }
        public decimal ViewPortSWLong { get; set; }
        public decimal ViewPortNELat { get; set; }
        public decimal ViewPortNELong { get; set; }
        public DateTime Datestamp { get; set; }

        /// <summary>
        /// Constructs a new GeoCode object
        /// </summary>
        public GeoCode()
        {

        }

        /// <summary>
        /// Constructs a new GeoCode object from DataReader
        /// </summary>
        /// <param name="dr">Open DataReader</param>
        public GeoCode(ref System.Data.SqlClient.SqlDataReader dr)
        {
            this.Id = TypeCast.ToInt(dr["Id"]);
            this.InputAddress = TypeCast.ToString(dr["InputAddress"]);
            this.FormattedAddress = TypeCast.ToString(dr["FormattedAddress"]);
            this.Latitude = TypeCast.ToDecimal(dr["Latitude"]);
            this.Longitude = TypeCast.ToDecimal(dr["Longitude"]);
            this.ViewPortSWLat = TypeCast.ToDecimal(dr["ViewPortSWLat"]);
            this.ViewPortSWLong = TypeCast.ToDecimal(dr["ViewPortSWLong"]);
            this.ViewPortNELat = TypeCast.ToDecimal(dr["ViewPortNELat"]);
            this.ViewPortNELong = TypeCast.ToDecimal(dr["ViewPortNELong"]);
            this.Datestamp = TypeCast.ToDateTime(dr["Datestamp"]);

        }

        /// <summary>
        /// Lookup a new GeoCode from Google Maps API, save to DB and return item
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static GeoCode CreateGeoCode(string apiKey, string address)
        {
            //No address - no result
            if (address == null || address.Trim() == "") return null;

            GeoCode g = new GeoCode();

            #region Request GeoCode From Google Maps
            
            var requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false&key=" + apiKey, Uri.EscapeDataString(address));

            WebRequest request = WebRequest.Create(requestUri);
            WebResponse response = request.GetResponse();
            System.Xml.Linq.XDocument xdoc = System.Xml.Linq.XDocument.Load(response.GetResponseStream());
            
            List<XElement> results = new List<XElement>();
            foreach (XElement x in xdoc.Element("GeocodeResponse").Elements("result"))
                results.Add(x);

            XElement result = null;
            //No result or anything but 1 result, exit!
            if (results == null) return null;
            if (results.Count != 1) return null;
            result = results[0];

            #endregion

            #region Addresses

            g.InputAddress = address;
            g.FormattedAddress = result.Element("formatted_address").Value;

            #endregion

            #region Lat / Long

            var locationElement = result.Element("geometry").Element("location");
            g.Latitude = TypeCast.ToDecimal(locationElement.Element("lat").Value, System.Globalization.CultureInfo.InvariantCulture);
            g.Longitude = TypeCast.ToDecimal(locationElement.Element("lng").Value, System.Globalization.CultureInfo.InvariantCulture);
            
            #endregion

            #region ViewPort

            var viewport = result.Element("geometry").Element("viewport");
            var sw = viewport.Element("southwest");
            var ne = viewport.Element("northeast");

            g.ViewPortSWLat = TypeCast.ToDecimal(sw.Element("lat").Value, System.Globalization.CultureInfo.InvariantCulture);
            g.ViewPortSWLong = TypeCast.ToDecimal(sw.Element("lng").Value, System.Globalization.CultureInfo.InvariantCulture);

            g.ViewPortNELat = TypeCast.ToDecimal(ne.Element("lat").Value, System.Globalization.CultureInfo.InvariantCulture);
            g.ViewPortNELong = TypeCast.ToDecimal(ne.Element("lng").Value, System.Globalization.CultureInfo.InvariantCulture);

            #endregion

            #region Save GeoCode to DB

            SQLDB sql = new SQLDB();

            sql.GeoCode_Update(g);

            sql.Dispose();
            sql = null;

            #endregion

            return g;
        }
    }
}
