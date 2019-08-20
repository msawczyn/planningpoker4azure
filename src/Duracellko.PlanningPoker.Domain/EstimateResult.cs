using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Collection of estimates of all members involved in planning poker.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "EstimateResult is more than just a collection.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Interface implemetnation members are grouped together.")]
    public sealed class EstimateResult : ICollection<KeyValuePair<Member, Estimate>>
    {
        private readonly Dictionary<Member, Estimate> _estimates = new Dictionary<Member, Estimate>();
        private bool _isReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="EstimateResult"/> class.
        /// </summary>
        /// <param name="members">The members involved in planning poker.</param>
        public EstimateResult(IEnumerable<Member> members)
        {
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            foreach (Member member in members)
            {
                _estimates.Add(member, null);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Duracellko.PlanningPoker.Domain.Estimate"/> for the specified member.
        /// </summary>
        /// <param name="member">The member to get or set estimate for.</param>
        /// <returns>The estimate of the member.</returns>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Member is valid indexer of EstimateResult.")]
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Key not found in indexer.")]
        public Estimate this[Member member]
        {
            get
            {
                if (!ContainsMember(member))
                {
                    throw new KeyNotFoundException(Resources.Error_MemberNotInResult);
                }

                return _estimates[member];
            }

            set
            {
                if (_isReadOnly)
                {
                    throw new InvalidOperationException(Resources.Error_EstimateResultIsReadOnly);
                }

                if (!ContainsMember(member))
                {
                    throw new KeyNotFoundException(Resources.Error_MemberNotInResult);
                }

                _estimates[member] = value;
            }
        }

        /// <summary>
        /// Determines whether the specified member contains member.
        /// </summary>
        /// <param name="member">The Scrum team member.</param>
        /// <returns>
        ///   <c>True</c> if the specified member contains member; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsMember(Member member)
        {
            return _estimates.ContainsKey(member);
        }

        /// <summary>
        /// Sets the collection as read only. Mostly used after all members picked their estimates.
        /// </summary>
        public void SetReadOnly()
        {
            _isReadOnly = true;
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count
        {
            get { return _estimates.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///     <c>True</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerator&lt;T&gt;"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<Member, Estimate>> GetEnumerator()
        {
            return _estimates.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool ICollection<KeyValuePair<Member, Estimate>>.Contains(KeyValuePair<Member, Estimate> item)
        {
            return ((ICollection<KeyValuePair<Member, Estimate>>)_estimates).Contains(item);
        }

        void ICollection<KeyValuePair<Member, Estimate>>.CopyTo(KeyValuePair<Member, Estimate>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Member, Estimate>>)_estimates).CopyTo(array, arrayIndex);
        }

        void ICollection<KeyValuePair<Member, Estimate>>.Add(KeyValuePair<Member, Estimate> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<Member, Estimate>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<Member, Estimate>>.Remove(KeyValuePair<Member, Estimate> item)
        {
            throw new NotSupportedException();
        }
    }
}
