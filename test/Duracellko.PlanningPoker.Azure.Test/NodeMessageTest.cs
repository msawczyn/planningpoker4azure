using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    public class NodeMessageTest
    {
        [TestMethod]
        public void Constructor_MessageType_MessageTypeIsSet()
        {
            // Arrange
            NodeMessageType messageType = NodeMessageType.ScrumTeamMessage;

            // Act
            NodeMessage result = new NodeMessage(messageType);

            // Verify
            Assert.AreEqual<NodeMessageType>(messageType, result.MessageType);
        }
    }
}
