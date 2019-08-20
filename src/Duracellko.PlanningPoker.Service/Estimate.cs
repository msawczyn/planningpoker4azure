using System;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Estimate value of a planning poker card.
    /// </summary>
    [Serializable]
    public class Estimate
    {
        /// <summary>
        /// Value representing estimate of positive infinity.
        /// </summary>
        public const double PositiveInfinity = -1111100.0;

        /// <summary>
        /// Gets or sets the estimate value. Estimate can be any positive number (usually Fibonacci numbers) or
        /// positive infinity or null representing unknown estimate.
        /// </summary>
        /// <value>The estimate value.</value>
        [JsonProperty("value")]
        public double? Value { get; set; }
    }
}
