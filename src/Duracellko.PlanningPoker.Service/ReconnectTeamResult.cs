using System;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Result data of reconnect operation.
    /// </summary>
    [Serializable]
    public class ReconnectTeamResult
    {
        /// <summary>
        /// Gets or sets the Scrum team.
        /// </summary>
        /// <value>
        /// The Scrum team.
        /// </value>
        [JsonProperty("scrumTeam")]
        public ScrumTeam ScrumTeam { get; set; }

        /// <summary>
        /// Gets or sets the last message ID for the member.
        /// </summary>
        /// <value>
        /// The last message ID.
        /// </value>
        [JsonProperty("lastMessageId")]
        public long LastMessageId { get; set; }

        /// <summary>
        /// Gets or sets the last selected estimate by member.
        /// </summary>
        /// <value>
        /// The selected estimate.
        /// </value>
        [JsonProperty("selectedEstimate")]
        public Estimate SelectedEstimate { get; set; }
    }
}
