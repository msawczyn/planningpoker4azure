using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class MemberMessageTest
    {
        [TestMethod]
        public void Constructor_TypeSpecified_MessageTypeIsSet()
        {
            // Arrange
            MessageType type = MessageType.MemberJoined;

            // Act
            MemberMessage result = new MemberMessage(type);

            // Verify
            Assert.AreEqual<MessageType>(type, result.MessageType);
        }

        [TestMethod]
        public void Member_Set_MemberIsSet()
        {
            // Arrange
            MemberMessage target = new MemberMessage(MessageType.MemberJoined);
            ScrumTeam team = new ScrumTeam("test team");
            Member member = new Member(team, "test");

            // Act
            target.Member = member;

            // Verify
            Assert.AreEqual<Observer>(member, target.Member);
        }
    }
}
