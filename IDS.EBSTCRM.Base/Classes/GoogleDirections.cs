//using System;
//using System.Collections;
//using System.Net;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using Newtonsoft.Json;
//using System.Text.RegularExpressions;

//namespace IDS.EBSTCRM.Base.GoogleDirections
//{
//    public class GDirections
//    {
        
//        //private string _key = "___ENTER_YOUR_KEY___";
//        private string _output = "js";
//        private string _requestFormat = "http://maps.google.com/maps/nav?key={0}&output={1}&q={2}";
//        private Hashtable _data = new Hashtable();

//        public bool IsValid { get; private set; }

//        public decimal DrivingMeters
//        {
//            get { return Convert.ToDecimal(((Hashtable)Directions["Distance"])["meters"]); }
//        }

//        public TimeSpan DrivingTime
//        {
//            get { return new TimeSpan(0, 0, Convert.ToInt32(((Hashtable)Directions["Duration"])["seconds"])); }
//        }

//        public decimal DrivingMiles
//        {
//            get { return Math.Round(DrivingMeters * (decimal)0.000621371192, 2); } // according to Google.
//        }

//        private Hashtable Directions
//        {
//            get { return (Hashtable)_data["Directions"]; }
//        }

//        private List<Route> _routes = null;

//        /// <summary>
//        /// Returns a list of driving routes from the given start location to the destination.
//        /// </summary>
//        public List<Route> Routes
//        {
//            get
//            {
//                // Cache results
//                if (_routes == null && IsValid)
//                {
//                    ArrayList data_routes = (ArrayList)Directions["Routes"];
//                    if (data_routes.Count < 1)
//                        return null;

//                    // Cycle through each returned route
//                    List<Route> routes = new List<Route>();
//                    foreach (Hashtable data_route in data_routes)
//                    {
//                        Route route = new Route()
//                        {
//                            DrivingMeters = Convert.ToDecimal(((Hashtable)data_route["Distance"])["meters"]),
//                            DrivingSeconds = Convert.ToInt32(((Hashtable)data_route["Duration"])["seconds"]),
//                            DrivingDistanceHTML = (string)((Hashtable)data_route["Distance"])["html"],
//                            SummaryHTML = (string)data_route["summaryHtml"]
//                        };

//                        // Cycle through each step in a route
//                        ArrayList data_routesteps = (ArrayList)data_route["Steps"];
//                        foreach (Hashtable data_routestep in data_routesteps)
//                        {
//                            RouteStep step = new RouteStep()
//                            {
//                                DrivingMeters = Convert.ToDecimal(((Hashtable)data_routestep["Distance"])["meters"]),
//                                DrivingSeconds = Convert.ToInt32(((Hashtable)data_routestep["Duration"])["seconds"]),
//                                DrivingDistanceHTML = (string)((Hashtable)data_routestep["Distance"])["html"],
//                                DescriptionHTML = (string)data_routestep["descriptionHtml"]
//                            };
//                            route.Add(step);
//                        }

//                        routes.Add(route);
//                    }

//                    _routes = routes;
//                }

//                return _routes;
//            }
//        }

//        /// <summary>
//        /// Returns a semantic, xhtml ordered list representing driving directions.
//        /// </summary>
//        public List<string> RoutesHtml
//        {
//            get
//            {
//                List<string> routesHtml = new List<string>();

//                foreach (Route route in Routes)
//                {
//                    string html = "";

//                    foreach (RouteStep step in route)
//                    {
//                        html += "<li><span class=\"description\">" + step.DescriptionHTML + "</span>"
//                              + "<span class=\"distance\">" + step.DrivingMiles.ToString("0.0") + " mi</span></li>";
//                    }

//                    html = "<ol>" + html + "</ol>";
//                    routesHtml.Add(html);
//                }

//                return routesHtml;
//            }
//        }

//        public GDirections(string Key, string startAddress, string endAddress)
//            : this(Key, "From " + startAddress + " to " + endAddress)
//        {
//        }
        
//        /// <summary>
//        /// Returns a GDirections object.
//        /// </summary>
//        /// <param name="path">A path in the format "From {a} to {b}".  Example: "From Lexington, KY to Burkesville, KY"</param>
//        public GDirections(string Key, string path)
//        {
//            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
//                string.Format(_requestFormat, Key, _output, Uri.EscapeUriString(path)));
//            request.Method = "GET";
//            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//            StreamReader reader = new StreamReader(response.GetResponseStream());
//            string json = reader.ReadToEnd();
//            json = parseUnicode(json);

//            _data = readJson(new JsonReader(new StringReader(json)));
//            IsValid = _data.ContainsKey("Directions");

//        }

//        private Hashtable readJson(JsonReader jreader)
//        {
//            Hashtable data = new Hashtable();
//            while (jreader.Read() && jreader.TokenType != JsonToken.EndObject)
//            {
//                string key = null;
//                object value = null;

//                if (jreader.TokenType == JsonToken.PropertyName)
//                {
//                    key = (string)jreader.Value;
//                    jreader.Read();
//                    switch (jreader.TokenType)
//                    {
//                        case JsonToken.StartArray:
//                            value = readJsonArray(jreader);
//                            break;
//                        case JsonToken.StartObject:
//                            value = readJson(jreader);
//                            break;
//                        case JsonToken.Null:
//                        case JsonToken.Undefined:
//                            value = null;
//                            break;
//                        default:
//                            value = jreader.Value;
//                            break;
//                    }
//                }

//                if (key != null)
//                    data.Add(key, value);
//            }
//            return data;
//        }

//        private ArrayList readJsonArray(JsonReader jreader)
//        {
//            ArrayList value = new ArrayList();
//            while (jreader.Read() && jreader.TokenType != JsonToken.EndArray)
//            {
//                switch (jreader.TokenType)
//                {
//                    case JsonToken.StartArray:
//                        value.Add(readJsonArray(jreader));
//                        break;
//                    case JsonToken.StartObject:
//                        value.Add(readJson(jreader));
//                        break;
//                    case JsonToken.Null:
//                    case JsonToken.Undefined:
//                        value.Add(null);
//                        break;
//                    default:
//                        value.Add(jreader.Value);
//                        break;
//                }
//            }
//            return value;
//        }

//        private string parseUnicode(string s)
//        {
//            string t = s;
//            Match match = Regex.Match(s, "[^\\\\]\\\\u([0-9abcdef]{4})", RegexOptions.IgnoreCase);
//            while(match != null && match.Success)
//            {
//                int charCode = Convert.ToInt32(match.Groups[1].Value, 16);
//                t = t.Replace(match.Value.Substring(1), Convert.ToChar(charCode).ToString());
//                match = match.NextMatch();
//            }
//            return t;
//        }
//    }

//    public class RouteStep
//    {        
//        // Core data properties
//        public string DescriptionHTML { get; set; }
//        public string DrivingDistanceHTML { get; set; }
//        public decimal DrivingMeters { get; set; }
//        public int DrivingSeconds { get; set; }

//        // Convenience properties
//        public decimal DrivingMiles
//        {
//            get { return Math.Round(DrivingMeters * (decimal)0.000621371192, 2); }
//        }
        
//        public TimeSpan DrivingTime 
//        {
//            get
//            {
//                return new TimeSpan(0, 0, DrivingSeconds);
//            }
//        }        
//    }

//    public class Route : List<RouteStep>
//    {
//        public string SummaryHTML { get; set; }
//        public string DrivingDistanceHTML { get; set; }
//        public decimal DrivingMeters { get; set; }
//        public int DrivingSeconds { get; set; }

//        public decimal DrivingMiles
//        {
//            get { return Math.Round(DrivingMeters * (decimal)0.000621371192, 2); }
//        }

//        public TimeSpan DrivingTime
//        {
//            get
//            {
//                return new TimeSpan(0, 0, DrivingSeconds);
//            }
//        } 

//        public Route() : base() { }
//        public Route(int capacity) : base(capacity) { }
//        public Route(IEnumerable<RouteStep> collection) : base(collection) { }
//    }
//}
