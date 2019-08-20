using System;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Specifies status if Scrum team.
    /// </summary>
    [Serializable]
    public enum TeamState
    {
        /// <summary>
        /// Scrum team is initial state and estimate has not started yet.
        /// </summary>
        Initial,

        /// <summary>
        /// Estimate is in progress. Members can pick their estimates.
        /// </summary>
        EstimateInProgress,

        /// <summary>
        /// All members picked estimates and the estimate is finished.
        /// </summary>
        EstimateFinished,

        /// <summary>
        /// Estimate was canceled by Scrum master.
        /// </summary>
        EstimateCanceled
    }
}
