using System;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Message sent to all members and observers after all members picked an estimate. The message contains <see cref="EstimateResult"/>.
    /// </summary>
    [Serializable]
    public class EstimateResultMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EstimateResultMessage"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        public EstimateResultMessage(MessageType type)
            : base(type)
        {
        }

        /// <summary>
        /// Gets or sets the estimate result associated to the message.
        /// </summary>
        /// <value>
        /// The estimate result.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Message is sent to client and forgotten. Client can modify it as it wants.")]
        public EstimateResult EstimateResult { get; set; }
    }
}
