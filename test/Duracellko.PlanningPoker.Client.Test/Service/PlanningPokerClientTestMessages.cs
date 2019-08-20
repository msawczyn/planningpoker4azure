using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [TestClass]
    public class PlanningPokerClientTestMessages
    {
        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_RequestsGetMessagesUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ScrumMasterName}&lastMessageId=0")
                .Respond(PlanningPokerClientTest.JsonType, "[]");
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, 0, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task GetMessages_LastMessageId_RequestsGetMessagesUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&lastMessageId=2157483849")
                .Respond(PlanningPokerClientTest.JsonType, "[]");
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 2157483849, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEmptyMessage()
        {
            string messageJson = PlanningPokerClientData.GetEmptyMessageJson();
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Message message = result[0];
            Assert.AreEqual(0, message.Id);
            Assert.AreEqual(MessageType.Empty, message.Type);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsMemberJoinedMessage()
        {
            string messageJson = PlanningPokerClientData.GetMemberJoinedMessageJson("1");
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(MemberMessage));
            MemberMessage message = (MemberMessage)result[0];
            Assert.AreEqual(1, message.Id);
            Assert.AreEqual(MessageType.MemberJoined, message.Type);
            Assert.AreEqual(PlanningPokerClientData.MemberName, message.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, message.Member.Type);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsMemberDisconnectedMessage()
        {
            string messageJson = PlanningPokerClientData.GetMemberDisconnectedMessageJson("2", name: PlanningPokerClientData.ObserverName, type: PlanningPokerClientData.ObserverType);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(MemberMessage));
            MemberMessage message = (MemberMessage)result[0];
            Assert.AreEqual(2, message.Id);
            Assert.AreEqual(MessageType.MemberDisconnected, message.Type);
            Assert.AreEqual(PlanningPokerClientData.ObserverName, message.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, message.Member.Type);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEstimateStartedMessage()
        {
            string messageJson = PlanningPokerClientData.GetEstimateStartedMessageJson("2157483849");
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Message message = result[0];
            Assert.AreEqual(2157483849, message.Id);
            Assert.AreEqual(MessageType.EstimateStarted, message.Type);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEstimateEndedMessage()
        {
            string messageJson = PlanningPokerClientData.GetEstimateEndedMessageJson("8");
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(EstimateResultMessage));
            EstimateResultMessage message = (EstimateResultMessage)result[0];
            Assert.AreEqual(8, message.Id);
            Assert.AreEqual(MessageType.EstimateEnded, message.Type);

            Assert.AreEqual(4, message.EstimateResult.Count);
            EstimateResultItem estimationResult = message.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(2.0, estimationResult.Estimate.Value);

            estimationResult = message.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimate.Value);

            estimationResult = message.EstimateResult[2];
            Assert.AreEqual("Me", estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimate);

            estimationResult = message.EstimateResult[3];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimate.Value.Value));
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsEstimateCanceledMessage()
        {
            string messageJson = PlanningPokerClientData.GetEstimateCanceledMessageJson("123");
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Message message = result[0];
            Assert.AreEqual(123, message.Id);
            Assert.AreEqual(MessageType.EstimateCanceled, message.Type);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_ReturnsMemberEstimatedMessage()
        {
            string messageJson = PlanningPokerClientData.GetMemberEstimatedMessageJson("22");
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, PlanningPokerClientData.GetMessagesJson(messageJson));
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(MemberMessage));
            MemberMessage message = (MemberMessage)result[0];
            Assert.AreEqual(22, message.Id);
            Assert.AreEqual(MessageType.MemberEstimated, message.Type);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, message.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, message.Member.Type);
        }

        [TestMethod]
        public async Task GetMessages_TeamAndMemberName_Returns3Messages()
        {
            string estimationStartedMessageJson = PlanningPokerClientData.GetEstimateStartedMessageJson("8");
            string memberEstimatedMessageJson = PlanningPokerClientData.GetMemberEstimatedMessageJson("9");
            string estimationEndedMessageJson = PlanningPokerClientData.GetEstimateEndedMessage2Json("10");
            string json = PlanningPokerClientData.GetMessagesJson(estimationStartedMessageJson, memberEstimatedMessageJson, estimationEndedMessageJson);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(PlanningPokerClientTest.BaseUrl + $"api/PlanningPokerService/GetMessages")
                .Respond(PlanningPokerClientTest.JsonType, json);
            PlanningPokerClient target = PlanningPokerClientTest.CreatePlanningPokerClient(httpMock);

            IList<Message> result = await target.GetMessages(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 0, CancellationToken.None);

            Assert.AreEqual(3, result.Count);
            Message message = result[0];
            Assert.AreEqual(8, message.Id);
            Assert.AreEqual(MessageType.EstimateStarted, message.Type);

            Assert.IsInstanceOfType(result[1], typeof(MemberMessage));
            MemberMessage memberMessage = (MemberMessage)result[1];
            Assert.AreEqual(9, memberMessage.Id);
            Assert.AreEqual(MessageType.MemberEstimated, memberMessage.Type);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, memberMessage.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, memberMessage.Member.Type);

            Assert.IsInstanceOfType(result[2], typeof(EstimateResultMessage));
            EstimateResultMessage estimationMessage = (EstimateResultMessage)result[2];
            Assert.AreEqual(10, estimationMessage.Id);
            Assert.AreEqual(MessageType.EstimateEnded, estimationMessage.Type);

            Assert.AreEqual(2, estimationMessage.EstimateResult.Count);
            EstimateResultItem estimationResult = estimationMessage.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(5.0, estimationResult.Estimate.Value);

            estimationResult = estimationMessage.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.AreEqual(40.0, estimationResult.Estimate.Value);
        }
    }
}
