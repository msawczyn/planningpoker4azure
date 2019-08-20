﻿using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Estimate value of a planning poker card.
    /// </summary>
    [Serializable]
    public class Estimate : IEquatable<Estimate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Estimate"/> class.
        /// </summary>
        public Estimate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Estimate"/> class.
        /// </summary>
        /// <param name="value">The estimate value.</param>
        public Estimate(double? value)
        {
            Value = value;
        }

      /// <summary>
      /// Gets the estimate value. Estimate can be any positive number (usually Fibonacci numbers) or positive infinity or null representing unknown estimate.
      /// </summary>
      /// <value>The estimate value.</value>
      public double? Value { get; private set; }

      /// <summary>
      /// Determines whether the specified <see cref="object"/> is equal to this instance.
      /// </summary>
      /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
      /// <returns>
      ///   <c>True</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
      /// </returns>
      public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Estimate))
            {
                return false;
            }

            return Equals((Estimate)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Value.HasValue ? Value.GetHashCode() : 0;
        }

        /// <summary>
        /// Indicates whether the current Estimate is equal to another Estimate.
        /// </summary>
        /// <param name="other">The other Estimate to compare with.</param>
        /// <returns><c>True</c> if the specified Estimate is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Estimate other)
        {
            return other != null ? Value == other.Value : false;
        }
    }
}
