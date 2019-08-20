﻿using System;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Status of participant in estimate.
    /// </summary>
    [Serializable]
    public class EstimateParticipantStatus
    {
        /// <summary>
        /// Gets or sets the name of the participant.
        /// </summary>
        /// <value>
        /// The name of the member.
        /// </value>
        [JsonProperty("memberName")]
        public string MemberName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this participant submitted an estimate already.
        /// </summary>
        /// <value>
        /// <c>True</c> if participant estimated; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("estimated")]
        public bool Estimated { get; set; }
    }
}
