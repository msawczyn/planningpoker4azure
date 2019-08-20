using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class MessageReceivedEventArgsTest
    {
        [TestMethod]
        public void Constructor_Message_MessagePropertyIsSet()
        {
            // Arrange
            Message message = new Message(MessageType.Empty);

            // Act
            MessageReceivedEventArgs result = new MessageReceivedEventArgs(message);

            // Verify
            Assert.AreEqual(message, result.Message);
        }
    }
}
