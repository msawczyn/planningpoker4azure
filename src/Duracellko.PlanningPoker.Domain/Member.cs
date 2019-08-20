using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Fields are placed near properties.")]
    public class Member : Observer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Member"/> class.
        /// </summary>
        /// <param name="team">The Scrum team, the member is joining.</param>
        /// <param name="name">The member name.</param>
        public Member(ScrumTeam team, string name)
            : base(team, name)
        {
        }

        private Estimate _estimate;

        /// <summary>
        /// Gets or sets the estimate, the member is picking in planning poker.
        /// </summary>
        /// <value>
        /// The estimate.
        /// </value>
        public Estimate Estimate
        {
            get
            {
                return _estimate;
            }

            set
            {
                if (_estimate != value)
                {
                    if (value != null && !Team.Estimates.Contains(value))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Error_EstimateIsNotAvailableInTeam, value.Value), nameof(value));
                    }

                    _estimate = value;
                    if (Team.State == TeamState.EstimateInProgress)
                    {
                        Team.OnMemberEstimated(this);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the estimate to unselected.
        /// </summary>
        internal void ResetEstimate()
        {
            _estimate = null;
        }
    }
}
