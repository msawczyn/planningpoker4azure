﻿using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Status of participant in estimate.
    /// </summary>
    public class EstimateParticipantStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EstimateParticipantStatus"/> class.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimated">If set to <c>true</c> then the member already estimated.</param>
        public EstimateParticipantStatus(string memberName, bool estimated)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName));
            }

            MemberName = memberName;
            Estimated = estimated;
        }

        /// <summary>
        /// Gets the name of the participant.
        /// </summary>
        /// <value>
        /// The name of the member.
        /// </value>
        public string MemberName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this participant submitted an estimate already.
        /// </summary>
        /// <value>
        /// <c>True</c> if participant estimated; otherwise, <c>false</c>.
        /// </value>
        public bool Estimated { get; private set; }
    }
}
