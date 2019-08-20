namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Object contains information about member estimates.
    /// </summary>
    public class MemberEstimate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberEstimate" /> class.
        /// </summary>
        /// <param name="memberName">Name of member, who estimated.</param>
        /// <remarks>Estimated value is not disclosed.</remarks>
        public MemberEstimate(string memberName)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberEstimate" /> class.
        /// </summary>
        /// <param name="memberName">Name of member, who estimated.</param>
        /// <param name="estimate">Estimated value.</param>
        public MemberEstimate(string memberName, double? estimate)
            : this(memberName)
        {
            Estimate = estimate;
            HasEstimate = true;
        }

        /// <summary>
        /// Gets name of member, who estimated.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Gets a value indicating whether estimate is disclosed.
        /// </summary>
        public bool HasEstimate { get; }

        /// <summary>
        /// Gets estimated value.
        /// </summary>
        public double? Estimate { get; }
    }
}
