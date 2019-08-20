using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimateResultMessageTest
    {
        [TestMethod]
        public void Constructor_TypeSpecified_MessageTypeIsSet()
        {
            // Arrange
            MessageType type = MessageType.EstimateEnded;

            // Act
            EstimateResultMessage result = new EstimateResultMessage(type);

            // Verify
            Assert.AreEqual<MessageType>(type, result.MessageType);
        }

        [TestMethod]
        public void EstimateResult_Set_EstimateResultIsSet()
        {
            // Arrange
            EstimateResultMessage target = new EstimateResultMessage(MessageType.EstimateEnded);
            EstimateResult estimationResult = new EstimateResult(Enumerable.Empty<Member>());

            // Act
            target.EstimateResult = estimationResult;

            // Verify
            Assert.AreEqual<EstimateResult>(estimationResult, target.EstimateResult);
        }
    }
}
