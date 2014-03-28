using Newtonsoft.Json;

namespace Influxdb
{
    /// <summary>
    /// 
    /// </summary>
    public class Database
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("replicationFactor")]
        public int ReplicationFactor;
    }
}
