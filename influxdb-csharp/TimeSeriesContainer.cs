using System;
using System.Text;
using Newtonsoft.Json;

namespace Influxdb
{
    /// <summary>
    /// 
    /// </summary>
    public class TimeSeriesContainer
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("columns")]
        public string[] Columns;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("points")]
        public dynamic Points;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(String.Format("Name: {0}\n", Name));

            if (Columns != null && Columns.Length > 0)
            {
                sb.Append(String.Format("Columns: {0}\n", string.Join(",", Columns)));
            }

            if (Points != null && Points.Count > 0)
            {
                sb.Append("Points:\n");

                var i = 0;
                foreach (var point in Points)
                {
                    sb.Append(String.Format(" ({0}) {1}\n", i, string.Join(",", point)));
                    i++;
                }
            }

            return sb.ToString();
        }
    }
}