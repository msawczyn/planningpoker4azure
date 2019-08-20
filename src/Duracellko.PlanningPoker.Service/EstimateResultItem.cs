using System;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Item of estimate result. It specifies what member picked what estimate.
    /// </summary>
    [Serializable]
    public class EstimateResultItem
    {
        /// <summary>
        /// Gets or sets the member, who picked an estimate.
        /// </summary>
        /// <value>
        /// The Scrum team member.
        /// </value>
        [JsonProperty("member")]
        public TeamMember Member { get; set; }

        /// <summary>
        /// Gets or sets the estimate picked by the member.
        /// </summary>
        /// <value>
        /// The picked estimate.
        /// </value>
        [JsonProperty("estimate")]
        public Estimate Estimate { get; set; }
    }
}
