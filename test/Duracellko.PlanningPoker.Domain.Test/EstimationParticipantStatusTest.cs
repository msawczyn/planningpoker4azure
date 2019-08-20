using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimateParticipantStatusTest
    {
        [TestMethod]
        public void Constructor_MemberNameSpecified_MemberNameIsSet()
        {
            // Arrange
            string name = "Member";

            // Act
            EstimateParticipantStatus result = new EstimateParticipantStatus(name, false);

            // Verify
            Assert.AreEqual<string>(name, result.MemberName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_MemberNameNotSpecified_ArgumentNullException()
        {
            // Arrange
            string name = null;

            // Act
            EstimateParticipantStatus result = new EstimateParticipantStatus(name, false);
        }

        [TestMethod]
        public void Constructor_EstimatedSpecified_EstimatedIsSet()
        {
            // Arrange
            bool estimated = true;

            // Act
            EstimateParticipantStatus result = new EstimateParticipantStatus("Member", estimated);

            // Verify
            Assert.IsTrue(estimated);
        }
    }
}
