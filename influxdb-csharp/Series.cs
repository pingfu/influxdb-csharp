using Newtonsoft.Json;

namespace Influxdb
{
    /// <summary>
    /// 
    /// </summary>
    public class Series
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
    }
}
