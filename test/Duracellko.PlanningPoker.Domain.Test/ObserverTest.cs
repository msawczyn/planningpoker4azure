using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ObserverTest
    {
        [TestMethod]
        public void Constructor_TeamAndNameIsSpecified_TeamAndNameIsSet()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            string name = "test";

            // Act
            Observer result = new Observer(team, name);

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
            Assert.AreEqual<string>(name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_TeamNotSpecified_ArgumentNullException()
        {
            // Arrange
            string name = "test";

            // Act
            Observer result = new Observer(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NameIsEmpty_ArgumentNullException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");

            // Act
            Observer result = new Observer(team, string.Empty);
        }

        [TestMethod]
        public void HasMessages_GetAfterConstruction_ReturnsFalse()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Observer target = new Observer(team, "test");

            // Act
            bool result = target.HasMessage;

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Messages_GetAfterConstruction_ReturnsEmptyCollection()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Observer target = new Observer(team, "test");

            // Act
            IEnumerable<Message> result = target.Messages;

            // Verify
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void PopMessage_GetAfterConstruction_ReturnsNull()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Observer target = new Observer(team, "test");

            // Act
            Message result = target.PopMessage();

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ClearMessages_AfterConstruction_ReturnsZero()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            Observer target = new Observer(team, "test");

            // Act
            long result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(0, result);
        }

        [TestMethod]
        public void ClearMessages_After2Messages_HasNoMessages()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer target = team.Join("test", true);
            master.StartEstimate();
            master.CancelEstimate();

            // Act
            long result = target.ClearMessages();

            // Verify
            Assert.AreEqual<long>(2, result);
            Assert.IsFalse(target.HasMessage);
        }

        [TestMethod]
        public void LastActivity_AfterConstruction_ReturnsUtcNow()
        {
            // Arrange
            DateTime utcNow = new DateTime(2012, 1, 2, 4, 50, 13);
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(utcNow);

            ScrumTeam team = new ScrumTeam("test team", dateTimeProvider);
            Observer target = new Observer(team, "test");

            // Act
            DateTime result = target.LastActivity;

            // Verify
            Assert.AreEqual<DateTime>(utcNow, result);
        }

        [TestMethod]
        public void UpdateActivity_UtcNowIsChanged_LastActivityIsChanged()
        {
            // Arrange
            DateTime utcNow = new DateTime(2012, 1, 2, 4, 50, 13);
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 2, 3, 35, 0));

            ScrumTeam team = new ScrumTeam("test team", dateTimeProvider);
            Observer target = new Observer(team, "test");
            dateTimeProvider.SetUtcNow(utcNow);

            // Act
            target.UpdateActivity();
            DateTime result = target.LastActivity;

            // Verify
            Assert.AreEqual<DateTime>(utcNow, result);
        }
    }
}
