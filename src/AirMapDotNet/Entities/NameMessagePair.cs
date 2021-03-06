﻿using Newtonsoft.Json;

namespace AirMapDotNet.Entities
{
    /// <summary>
    /// A representation of AirMap's error name-message object.
    /// </summary>
    public sealed class NameMessagePair
    {
        /// <summary>
        /// The name of the query parameter that failed.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// The reason why the query parameter failed.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; internal set; }
    }
}