using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;

namespace Influxdb
{
    /// <summary>
    /// csharp API inspired by https://github.com/influxdb/influxdb-python/blob/master/influxdb/client.py
    /// </summary>
    public class InfluxDb
    {
        /// <summary>
        /// 
        /// </summary>
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// 
        /// </summary>
        public static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 
        /// </summary>
        public enum TimePrecision
        {
            Microseconds,
            Seconds,
            Minutes,
        };

        /// <summary>
        /// 
        /// </summary>
        public enum HttpVerb
        {
            // ReSharper disable InconsistentNaming
            GET,
            PUT,
            POST,
            DELETE
            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// 
        /// </summary>
        private HttpWebRequest _webRequest;

        /// <summary>
        /// 
        /// </summary>
        private Session _session;

        /// <summary>
        /// 
        /// </summary>
        public InfluxDb()
        {
            Init("http://localhost/", "root", "root");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        public InfluxDb(string url, string username, string password, string database = "")
        {
            Init(url, username, password, database);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        private void Init(string url, string username, string password, string database = "")
        {
            _session = new Session
            {
                Url = url,
                Username = username,
                Password = password,
                Database = database
            };
        }

        /// <summary>
        /// Manages communication to the influxdb API using System.Net.WebRequest
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        private string Invoke(HttpRequest httpRequest)
        {
            var completeUrl = String.Format("{0}{1}?u={3}&p={4}&{2}", _session.Url, httpRequest.Path, httpRequest.UrlParameters, _session.Username, _session.Password);

            _webRequest = (HttpWebRequest)WebRequest.Create(completeUrl);
            _webRequest.KeepAlive = false;
            _webRequest.ServicePoint.Expect100Continue = false;
            _webRequest.UserAgent = "influxdb-sharp";
            _webRequest.ContentType = "application/json; charset=UTF-8";
            _webRequest.ProtocolVersion = HttpVersion.Version11;
            _webRequest.Method = httpRequest.Verb.ToString();

            // attach json payload data to the request if appropriate
            if (httpRequest.Payload != null)
            {
                _webRequest.ContentLength = httpRequest.Payload.Length;

                using (var r = _webRequest.GetRequestStream())
                {
                    if (httpRequest.Payload != null)
                    {
                        using (var streamWriter = new StreamWriter(r))
                        {
                            streamWriter.Write(httpRequest.Payload);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                    }
                }
            }

            // receive the response
            try
            {
                using (var response = (HttpWebResponse) _webRequest.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            return streamReader.ReadToEnd().Trim();
                        }
                    }
                    throw new NullReferenceException("responseStream");
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var responseStream = ex.Response.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (var body = new StreamReader(responseStream))
                        {
                            switch (((HttpWebResponse)ex.Response).StatusCode)
                            {
                                case HttpStatusCode.Unauthorized:
                                    {
                                        throw new AuthenticationException(body.ReadToEnd());
                                    }
                                default: throw;
                            }
                        }
                    }
                }
                throw;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        public void SwitchUser(string username)
        {
            _session.Username = username;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void SwitchUser(string username, string password)
        {
            _session.Username = username;
            _session.Password = username;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void SwitchDatabase(string name)
        {
            _session.Database = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TestCredentials()
        {
            Invoke(new HttpRequest
            {
                Verb = HttpVerb.GET,
                Path = "/cluster_admins/authenticate"
            });

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private void WritePoints(string json)
        {
            Invoke( new HttpRequest {
                Verb = HttpVerb.POST,
                Path = String.Format("/db/{0}/series", _session.Database),
                UrlParameters = new UrlParameterCollection { { "time_precision", "m" } },
                Payload = json
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <returns></returns>
        public void WritePoints(TimeSeriesContainer[] timeSeries)
        {
            WritePoints(JsonConvert.SerializeObject(timeSeries, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="columns"></param>
        /// <param name="values">Accepts either a single[], or multi-dimensional[][] array of value objects</param>
        public void WritePoints(string name, string[] columns, dynamic values)
        {
            // point values must always be a multi-dimensional array
            WritePoints(values[0].GetType().IsArray == true
                            ? new[] {new TimeSeriesContainer {Name = name, Columns = columns, Points = values}}
                            : new[] {new TimeSeriesContainer {Name = name, Columns = columns, Points = new[] {values}}});
        }

        public NotImplemented WritePointsWithPrecision()
        {
            throw new NotImplementedException();
        }

        public NotImplemented DeletePoints()
        {
            throw new NotImplementedException();
        }

        public NotImplemented CreateScheduledDelete()
        {
            throw new NotImplementedException();
        }

        public NotImplemented ListScheduledDelete()
        {
            throw new NotImplementedException();
        }

        public NotImplemented RemoveScheduledDelete()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">SQL command text</param>
        /// <param name="timePrecision"></param>
        /// <param name="chunked"></param>
        /// <returns>The corresponding response from influxdb</returns>
        public TimeSeriesContainer Query(string command, TimePrecision timePrecision = TimePrecision.Seconds, bool chunked = false)
        {
            var result = Invoke(new HttpRequest
            {
                Verb = HttpVerb.GET,
                Path = String.Format("/db/{0}/series", _session.Database),
                UrlParameters = new UrlParameterCollection { { "time_precision", "m" }, { "q", command } }
            });

            // assume only one result set is ever returned
            return JsonConvert.DeserializeObject<TimeSeriesContainer[]>(result)[0];
        }

        public void CreateDatabase(string name)
        {
            var d = new Database { Name = name };
            Invoke(new HttpRequest
            {
                Verb = HttpVerb.POST,
                Path = "/db",
                Payload = JsonConvert.SerializeObject(d)
            });
        }

        public void DeleteDatabase(string name)
        {
            Invoke(new HttpRequest
            {
                Verb = HttpVerb.DELETE,
                Path = string.Format("/db/{0}", name)
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Database[] ListDatabase()
        {
            var result = Invoke(new HttpRequest
            {
                Verb = HttpVerb.GET,
                Path = "/db"
            });

            return JsonConvert.DeserializeObject<Database[]>(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Series[] ListSeries()
        {
            var result = Invoke(new HttpRequest
            {
                Verb = HttpVerb.GET,
                Path = String.Format("/db/{0}/series", _session.Database),
                UrlParameters = new UrlParameterCollection { { "q", "list series" } }
            });

            return JsonConvert.DeserializeObject<Series[]>(result);
        }

		public void DeleteSeries(string name)
        {
            Invoke(new HttpRequest
			{
				Verb = HttpVerb.DELETE,
				Path = string.Format("/db/{0}/series/{1}", _session.Database, name)
			});
        }

        public NotImplemented ListClusterAdmins()
        {
            throw new NotImplementedException();
        }

        public NotImplemented AddClusterAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented UpdateClusterAdminPassword()
        {
            throw new NotImplementedException();
        }

        public NotImplemented DeleteClusterAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented SetDatabaseAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented UnsetDatabaseAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented AlterDatabaseAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented ListDatabaseAdmins()
        {
            throw new NotImplementedException();
        }

        public NotImplemented AddDatabaseAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented UpdateDatabaseAdminPassword()
        {
            throw new NotImplementedException();
        }

        public NotImplemented DeleteDatabaseAdmin()
        {
            throw new NotImplementedException();
        }

        public NotImplemented ListDatabaseUsers()
        {
            throw new NotImplementedException();
        }

        public NotImplemented AddDatabaseUser()
        {
            throw new NotImplementedException();
        }

        public NotImplemented UpdateDatabaseUserPassword()
        {
            throw new NotImplementedException();
        }

        public NotImplemented DeleteDatabaseUser()
        {
            throw new NotImplementedException();
        }

        public NotImplemented UpdatePermission()
        {
            // update by POSTing to db/site_dev/users/<username>
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        private class Session
        {
            /// <summary>
            /// Address of the influxdb api - http://localhost:8086 - without a trailing slash
            /// todo: remove trailing forwardslashes in this accessor
            /// </summary>
            public string Url;

            /// <summary>
            /// 
            /// </summary>
            public string Username;

            /// <summary>
            /// 
            /// </summary>
            public string Password;

            /// <summary>
            /// 
            /// </summary>
            public string Database;
        }

        /// <summary>
        /// An object describing the http request which will be sent to the influx API
        /// </summary>
        private class HttpRequest
        {
            /// <summary>
            /// 
            /// </summary>
            public HttpVerb Verb;

            /// <summary>
            /// 
            /// </summary>
            public UrlParameterCollection UrlParameters;

            /// <summary>
            /// 
            /// </summary>
            public string Path;

            /// <summary>
            /// 
            /// </summary>
            public string Payload;

            /// <summary>
            /// 
            /// </summary>
            public HttpRequest()
            {
                UrlParameters = new UrlParameterCollection();
            }
        }

        /// <summary>
        /// A standard Dictionary with an overloaded ToString() method for returning
        /// the collection as a concetenated string of URL parameters
        /// </summary>
        private class UrlParameterCollection : Dictionary<string, string>
        {
            public override string ToString()
            {
                if (Count <= 0)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                foreach (var entry in this)
                {
                    sb.Append(entry.Key + "=" + entry.Value + "&");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Placeholder class
        /// </summary>
        public class NotImplemented
        {
        }
    }
}