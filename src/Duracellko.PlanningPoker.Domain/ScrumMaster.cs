using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Scrum master can additionally to member start and cancel estimate planning poker.
    /// </summary>
    [Serializable]
    public class ScrumMaster : Member
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumMaster"/> class.
        /// </summary>
        /// <param name="team">The Scrum team, the the master is joining.</param>
        /// <param name="name">The Scrum master name.</param>
        public ScrumMaster(ScrumTeam team, string name)
            : base(team, name)
        {
        }

        /// <summary>
        /// Starts new estimate.
        /// </summary>
        public void StartEstimate()
        {
            if (Team.State == TeamState.EstimateInProgress)
            {
                throw new InvalidOperationException(Resources.Error_EstimateIsInProgress);
            }

            Team.StartEstimate();
        }

        /// <summary>
        /// Cancels current estimate.
        /// </summary>
        public void CancelEstimate()
        {
            if (Team.State == TeamState.EstimateInProgress)
            {
                Team.CancelEstimate();
            }
        }
    }
}
