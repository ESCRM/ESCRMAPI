using System;
using System.Net;
using System.Web;
using IDS.EBSTCRM.Base;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IDS.EBSTCRM.WindowManager.Service {
    public class ipStackService {
        public static void GeoLocation(User u) {
            var ip = string.Empty;
            var key = string.Empty;
            try { ip = HttpContext.Current.Request.UserHostAddress; } catch (Exception) { ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]; }
            key = "1442e7eedc2473cce75eb0176ee3c4fe";
            try {
                var client = new WebClient();
                var response = client.DownloadString("http://api.ipstack.com/" + ip + "?access_key=" + key + "&output=json");
                var geolocation = JsonConvert.DeserializeObject<RootObject>(response);
                if (geolocation != null && !string.IsNullOrEmpty(geolocation.country_code) && geolocation.country_code != "DK") {

                    string ipinfo = "https://whatismyipaddress.com/ip/" + ip;
                    string bd = "Kære Martin<br><br>" +
                                "Den følgende konto er blevet anvendt udenfor Danmark:<br><br>" +
                                "Navn: " + (u.Firstname + " " + u.Lastname) + "<br>" +
                                "Email: " + (u.Email) + "<br>" +
                                "Sidste login: " + (u.LastLogin.ToString("dd-MM-yyyy hh:mm tt")) +
                                "<br><br>" +
                                "<b><u>Geografisk information:</u></b>" +
                                "IP: " + ip + " <a href=\"" + ipinfo + "\" target=_blank>vis detaljer</a><br>" +
                                "City: " + geolocation.city + "<br>" +
                                "Region: " + geolocation.region_name + "<br>" +
                                "Land: " + geolocation.country_name +
                                (!string.IsNullOrEmpty(geolocation.location.country_flag) ? " <img alt=\"Country\" style=\"height:20px;\" src=\"" + geolocation.location.country_flag + "\"><br>" : "<br>") +
                                "Kontinent: " + geolocation.continent_name + "<br>" +
                                "<br><br>" +
                                "Med venlig hilsen<br>" +
                                "ESCRM.dk";

                    var sql = new SQLDB();
                    var receiverList = sql.GetUserGroupEmailList(128);
                    if (string.IsNullOrEmpty(receiverList)) { receiverList = "mgr@ehsyd.dk;"; }
                    EmailManager.SendEmail("noreply@escrm.dk", receiverList, "Kontosikkerhedsadvarsel: Login uden for Danmark", bd, true);
                }
            } catch (Exception) {
                throw;
            }
        }
    }

    public class RootObject {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string type { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string region_code { get; set; }
        public string region_name { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public Location location { get; set; }

        /*
        public TimeZone time_zone { get; set; }
        public Currency currency { get; set; }
        public Connection connection { get; set; }
        public Security security { get; set; }
        */
    }

    public class Language {
        public string code { get; set; }
        public string name { get; set; }
        public string native { get; set; }
    }

    public class Location {
        //public string geoname_id { get; set; }
        //public string capital { get; set; }
        //public List<Language> languages { get; set; }
        public string country_flag { get; set; }
        //public string country_flag_emoji { get; set; }
        //public string country_flag_emoji_unicode { get; set; }
        //public string calling_code { get; set; }
        //public bool is_eu { get; set; }
    }

    public class TimeZone {
        public string id { get; set; }
        public DateTime current_time { get; set; }
        public string gmt_offset { get; set; }
        public string code { get; set; }
        public bool is_daylight_saving { get; set; }
    }

    public class Currency {
        public string code { get; set; }
        public string name { get; set; }
        public string plural { get; set; }
        public string symbol { get; set; }
        public string symbol_native { get; set; }
    }

    public class Connection {
        public string asn { get; set; }
        public string isp { get; set; }
    }

    public class Security {
        public bool is_proxy { get; set; }
        public object proxy_type { get; set; }
        public bool is_crawler { get; set; }
        public object crawler_name { get; set; }
        public object crawler_type { get; set; }
        public bool is_tor { get; set; }
        public string threat_level { get; set; }
        public object threat_types { get; set; }
    }
}