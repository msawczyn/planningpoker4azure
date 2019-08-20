using System;
using Duracellko.PlanningPoker.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    public class ScrumTeamMessageTest
    {
        [TestMethod]
        public void Constructor_TeamNameSpecified_TeamNameIsSet()
        {
            // Arrange
            string teamName = "test";
            MessageType messageType = MessageType.Empty;

            // Act
            ScrumTeamMessage result = new ScrumTeamMessage(teamName, messageType);

            // Verify
            Assert.AreEqual<string>(teamName, result.TeamName);
        }

        [TestMethod]
        public void Constructor_MessageTypeSpecified_MessageTypeIsSet()
        {
            // Arrange
            string teamName = "test";
            MessageType messageType = MessageType.MemberJoined;

            // Act
            ScrumTeamMessage result = new ScrumTeamMessage(teamName, messageType);

            // Verify
            Assert.AreEqual<MessageType>(messageType, result.MessageType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            string teamName = string.Empty;
            MessageType messageType = MessageType.Empty;

            // Act
            ScrumTeamMessage result = new ScrumTeamMessage(teamName, messageType);
        }
    }
}
