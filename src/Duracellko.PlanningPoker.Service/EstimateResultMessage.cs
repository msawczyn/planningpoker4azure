using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Message sent to all members and observers after all members picked an estimate.
    /// The message contains collection <see cref="EstimateResultItem"/> objects.
    /// </summary>
    [Serializable]
    public class EstimateResultMessage : Message
    {
        /// <summary>
        /// Gets or sets the estimate result items associated to the message.
        /// </summary>
        /// <value>
        /// The estimate result items collection.
        /// </value>
        [JsonProperty("estimationResult")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
        public IList<EstimateResultItem> EstimateResult { get; set; }
    }
}
