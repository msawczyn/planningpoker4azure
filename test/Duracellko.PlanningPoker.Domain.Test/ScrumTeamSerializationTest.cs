using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class ScrumTeamSerializationTest
    {
        [TestMethod]
        public void SerializeAndDeserialize_EmptyTeam_CopyOfTheTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<string>(team.Name, result.Name);
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamWithScrumMaster_CopyOfTheTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            team.SetScrumMaster("master");

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<string>(team.ScrumMaster.Name, result.ScrumMaster.Name);
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamWithMember_CopyOfTheTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            team.SetScrumMaster("master");
            Observer member = team.Join("member", false);

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<int>(2, result.Members.Count());
            Member resultMember = result.Members.First(m => m != result.ScrumMaster);
            Assert.AreEqual<string>(member.Name, resultMember.Name);
            Assert.AreEqual<DateTime>(member.LastActivity, resultMember.LastActivity);
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamWithObserver_CopyOfTheTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            team.SetScrumMaster("master");
            Observer observer = team.Join("member", true);

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<int>(1, result.Observers.Count());
            Observer resultObserver = result.Observers.First();
            Assert.AreEqual<string>(observer.Name, resultObserver.Name);
            Assert.AreEqual<DateTime>(observer.LastActivity, resultObserver.LastActivity);
        }

        [TestMethod]
        public void SerializeAndDeserialize_EstimateStarted_CopyOfTheTeam()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            ScrumMaster master = team.SetScrumMaster("master");
            Observer member = team.Join("member", false);
            master.StartEstimate();

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            Assert.AreEqual<TeamState>(team.State, result.State);
            Assert.AreEqual<int>(master.Messages.Count(), result.ScrumMaster.Messages.Count());
        }

        [TestMethod]
        public void SerializeAndDeserialize_TeamMessageReceivedEventHandler_NoMessageReceivedEventHandler()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            int eventsCount = 0;
            team.MessageReceived += new EventHandler<MessageReceivedEventArgs>((s, e) => eventsCount++);
            ScrumMaster master = team.SetScrumMaster("master");

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            eventsCount = 0;
            result.ScrumMaster.StartEstimate();
            Assert.AreEqual<int>(0, eventsCount);
        }

        [TestMethod]
        public void SerializeAndDeserialize_MemberMessageReceivedEventHandler_NoMessageReceivedEventHandler()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            ScrumMaster master = team.SetScrumMaster("master");
            int eventsCount = 0;
            master.MessageReceived += new EventHandler((s, e) => eventsCount++);

            // Act
            byte[] bytes = SerializeTeam(team);
            ScrumTeam result = DeserializeTeam(bytes);

            // Verify
            eventsCount = 0;
            result.ScrumMaster.StartEstimate();
            Assert.AreEqual<int>(0, eventsCount);
        }

        [TestMethod]
        public void SerializeAndDeserialize_DateTimeProviderAsContext_DateTimeProviderIsSet()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test");
            DateTimeProviderMock dateTimeProvider = new DateTimeProviderMock();

            // Act
            byte[] bytes = SerializeTeam(team);
            StreamingContext streamingContext = new StreamingContext(StreamingContextStates.All, dateTimeProvider);
            ScrumTeam result = DeserializeTeam(bytes, streamingContext);

            // Verify
            Assert.AreEqual<DateTimeProvider>(dateTimeProvider, result.DateTimeProvider);
        }

        private static byte[] SerializeTeam(ScrumTeam team)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, team);
                return memoryStream.ToArray();
            }
        }

        private static ScrumTeam DeserializeTeam(byte[] value)
        {
            return DeserializeTeam(value, null);
        }

        private static ScrumTeam DeserializeTeam(byte[] value, StreamingContext? context)
        {
            BinaryFormatter formatter = context.HasValue ? new BinaryFormatter(null, context.Value) : new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(value))
            {
                return (ScrumTeam)formatter.Deserialize(memoryStream);
            }
        }
    }
}
