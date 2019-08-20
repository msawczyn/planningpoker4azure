using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Message of event in specific Scrum team, which notifies that a member placed estimate.
    /// </summary>
    public class ScrumTeamMemberEstimateMessage : ScrumTeamMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMemberEstimateMessage"/> class.
        /// </summary>
        public ScrumTeamMemberEstimateMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamMemberEstimateMessage"/> class.
        /// </summary>
        /// <param name="teamName">The name of team, this message is related to.</param>
        /// <param name="messageType">The type of message.</param>
        public ScrumTeamMemberEstimateMessage(string teamName, MessageType messageType)
            : base(teamName, messageType)
        {
        }

        /// <summary>
        /// Gets or sets a name of member, which this message is related to.
        /// </summary>
        /// <value>The member name.</value>
        public string MemberName { get; set; }

        /// <summary>
        /// Gets or sets a member's estimate.
        /// </summary>
        /// <value>The member's estimate.</value>
        public double? Estimate { get; set; }
    }
}
