using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Type of message that can be sent to Scrum team members or observers.
    /// </summary>
    [Serializable]
    public enum MessageType
    {
        /// <summary>
        /// Empty message that can be ignored. Used to notify member, that he/she should stop waiting for message.
        /// </summary>
        Empty,

        /// <summary>
        /// Message specifies that a new member joined Scrum team.
        /// </summary>
        MemberJoined,

        /// <summary>
        /// Message specifies that a member disconnected from Scrum team.
        /// </summary>
        MemberDisconnected,

        /// <summary>
        /// Message specifies that estimate started and members can pick estimate.
        /// </summary>
        EstimateStarted,

        /// <summary>
        /// Message specifies that estimate ended and all members picked their estimates.
        /// </summary>
        EstimateEnded,

        /// <summary>
        /// Message specifies that estimate was canceled by Scrum master.
        /// </summary>
        EstimateCanceled,

        /// <summary>
        /// Message specifies that a member placed estimate.
        /// </summary>
        MemberEstimated,

        /// <summary>
        /// Message specifies that a member is still active.
        /// </summary>
        MemberActivity,

        /// <summary>
        /// Message specifies that a new Scrum team was created.
        /// </summary>
        TeamCreated
    }
}
