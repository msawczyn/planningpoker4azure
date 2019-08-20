using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [TestClass]
    public class PlanningPokerClientTest
    {
        internal const string BaseUrl = "http://planningpoker.duracellko.net/";
        internal const string JsonType = "application/json";
        internal const string TextType = "text/plain";

        [TestMethod]
        public async Task CreateTeam_TeamAndScrumMasterName_RequestsCreateTeamUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/CreateTeam?teamName={PlanningPokerClientData.TeamName}&scrumMasterName={PlanningPokerClientData.ScrumMasterName}")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson());
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task CreateTeam_TeamAndScrumMasterName_ReturnsScrumTeam()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + "api/PlanningPokerService/CreateTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson());
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.Initial, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(1, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimates(result);

            Assert.AreEqual(0, result.EstimateResult.Count);
            Assert.AreEqual(0, result.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task CreateTeam_TeamNameExists_PlanningPokerException()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + "api/PlanningPokerService/CreateTeam")
                .Respond(HttpStatusCode.BadRequest, TextType, "Team 'Test team' already exists.");
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            PlanningPokerException exception = await Assert.ThrowsExceptionAsync<PlanningPokerException>(() => target.CreateTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None));

            Assert.AreEqual("Team 'Test team' already exists.", exception.Message);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_RequestsJoinTeamUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/JoinTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&asObserver=False")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndObserverName_RequestsJoinTeamUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/JoinTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ObserverName}&asObserver=True")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(observer: true));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ObserverName, true, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeam()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.Initial, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimates(result);

            Assert.AreEqual(0, result.EstimateResult.Count);
            Assert.AreEqual(0, result.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndObserverName_ReturnsScrumTeam()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(observer: true));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ObserverName, true, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.Initial, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(1, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            Assert.AreEqual(1, result.Observers.Count);
            member = result.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimates(result);

            Assert.AreEqual(0, result.EstimateResult.Count);
            Assert.AreEqual(0, result.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateFinished()
        {
            string estimationResultJson = PlanningPokerClientData.GetEstimateResultJson();
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimateFinished, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(1, result.Observers.Count);
            member = result.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimates(result);

            Assert.AreEqual(2, result.EstimateResult.Count);
            EstimateResultItem estimationResult = result.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(5.0, estimationResult.Estimate.Value);

            estimationResult = result.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.AreEqual(20.0, estimationResult.Estimate.Value);

            Assert.AreEqual(0, result.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateFinishedAndEstimateIsInfinity()
        {
            string estimationResultJson = PlanningPokerClientData.GetEstimateResultJson(scrumMasterEstimate: "-1111100", memberEstimate: "null");
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimateFinished, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(1, result.Observers.Count);
            member = result.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimates(result);

            Assert.AreEqual(2, result.EstimateResult.Count);
            EstimateResultItem estimationResult = result.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimate.Value.Value));

            estimationResult = result.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimate.Value);

            Assert.AreEqual(0, result.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateCanceled()
        {
            string estimationResultJson = PlanningPokerClientData.GetEstimateResultJson(scrumMasterEstimate: "0", memberEstimate: null);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, state: 3, estimationResult: estimationResultJson));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimateCanceled, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimates(result);

            Assert.AreEqual(2, result.EstimateResult.Count);
            EstimateResultItem estimationResult = result.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(0.0, estimationResult.Estimate.Value);

            estimationResult = result.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimate);

            Assert.AreEqual(0, result.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task JoinTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateInProgress()
        {
            string estimationParticipantsJson = PlanningPokerClientData.GetEstimateParticipantsJson();
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/JoinTeam")
                .Respond(JsonType, PlanningPokerClientData.GetScrumTeamJson(member: true, state: 1, estimationParticipants: estimationParticipantsJson));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ScrumTeam result = await target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(TeamState.EstimateInProgress, result.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, result.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, result.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, result.ScrumMaster.Type);

            Assert.AreEqual(2, result.Members.Count);
            TeamMember member = result.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = result.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, result.Observers.Count);

            AssertAvailableEstimates(result);

            Assert.AreEqual(0, result.EstimateResult.Count);

            Assert.AreEqual(2, result.EstimateParticipants.Count);
            EstimateParticipantStatus estimationParticipant = result.EstimateParticipants[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationParticipant.MemberName);
            Assert.IsTrue(estimationParticipant.Estimated);

            estimationParticipant = result.EstimateParticipants[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationParticipant.MemberName);
            Assert.IsFalse(estimationParticipant.Estimated);
        }

        [TestMethod]
        public async Task JoinTeam_TeamDoesNotExist_PlanningPokerException()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + "api/PlanningPokerService/JoinTeam")
                .Respond(HttpStatusCode.BadRequest, TextType, "Team 'Test team' does not exist.");
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            PlanningPokerException exception = await Assert.ThrowsExceptionAsync<PlanningPokerException>(() => target.JoinTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, false, CancellationToken.None));

            Assert.AreEqual("Team 'Test team' does not exist.", exception.Message);
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_RequestsJoinTeamUrl()
        {
            string scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/ReconnectTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}")
                .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeam()
        {
            string scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
                .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ReconnectTeamResult result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual(0, result.LastMessageId);
            Assert.IsNull(result.SelectedEstimate);

            ScrumTeam scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.Initial, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

            Assert.AreEqual(2, scrumTeam.Members.Count);
            TeamMember member = scrumTeam.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = scrumTeam.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, scrumTeam.Observers.Count);

            AssertAvailableEstimates(scrumTeam);

            Assert.AreEqual(0, scrumTeam.EstimateResult.Count);
            Assert.AreEqual(0, scrumTeam.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamAndLastMessageId()
        {
            string estimationResultJson = PlanningPokerClientData.GetEstimateResultJson(scrumMasterEstimate: "1", memberEstimate: "1");
            string scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
                .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "123"));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ReconnectTeamResult result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual(123, result.LastMessageId);
            Assert.IsNull(result.SelectedEstimate);

            ScrumTeam scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimateFinished, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

            Assert.AreEqual(2, scrumTeam.Members.Count);
            TeamMember member = scrumTeam.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = scrumTeam.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(1, scrumTeam.Observers.Count);
            member = scrumTeam.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimates(scrumTeam);

            Assert.AreEqual(2, scrumTeam.EstimateResult.Count);
            EstimateResultItem estimationResult = scrumTeam.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(1.0, estimationResult.Estimate.Value);

            estimationResult = scrumTeam.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.AreEqual(1.0, estimationResult.Estimate.Value);

            Assert.AreEqual(0, scrumTeam.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateFinished()
        {
            string estimationResultJson = PlanningPokerClientData.GetEstimateResultJson(scrumMasterEstimate: "null", memberEstimate: "-1111100");
            string scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, observer: true, state: 2, estimationResult: estimationResultJson);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
                .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "123", selectedEstimate: "-1111100"));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ReconnectTeamResult result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual(123, result.LastMessageId);
            Assert.IsNotNull(result.SelectedEstimate);
            Assert.IsNotNull(result.SelectedEstimate.Value);
            Assert.IsTrue(double.IsPositiveInfinity(result.SelectedEstimate.Value.Value));

            ScrumTeam scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimateFinished, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

            Assert.AreEqual(2, scrumTeam.Members.Count);
            TeamMember member = scrumTeam.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = scrumTeam.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(1, scrumTeam.Observers.Count);
            member = scrumTeam.Observers[0];
            Assert.AreEqual(PlanningPokerClientData.ObserverName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ObserverType, member.Type);

            AssertAvailableEstimates(scrumTeam);

            Assert.AreEqual(2, scrumTeam.EstimateResult.Count);
            EstimateResultItem estimationResult = scrumTeam.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimate.Value);

            estimationResult = scrumTeam.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsTrue(double.IsPositiveInfinity(estimationResult.Estimate.Value.Value));

            Assert.AreEqual(0, scrumTeam.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateFinishedAndEstimateIsNull()
        {
            string estimationResultJson = PlanningPokerClientData.GetEstimateResultJson(scrumMasterEstimate: "8", memberEstimate: null);
            string scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, state: 2, estimationResult: estimationResultJson);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
                .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "2157483849", selectedEstimate: "8"));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ReconnectTeamResult result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, CancellationToken.None);

            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual(2157483849, result.LastMessageId);
            Assert.IsNotNull(result.SelectedEstimate);
            Assert.AreEqual(8.0, result.SelectedEstimate.Value);

            ScrumTeam scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimateFinished, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

            Assert.AreEqual(2, scrumTeam.Members.Count);
            TeamMember member = scrumTeam.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = scrumTeam.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, scrumTeam.Observers.Count);

            AssertAvailableEstimates(scrumTeam);

            Assert.AreEqual(2, scrumTeam.EstimateResult.Count);
            EstimateResultItem estimationResult = scrumTeam.EstimateResult[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, estimationResult.Member.Type);
            Assert.AreEqual(8.0, estimationResult.Estimate.Value);

            estimationResult = scrumTeam.EstimateResult[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationResult.Member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, estimationResult.Member.Type);
            Assert.IsNull(estimationResult.Estimate);

            Assert.AreEqual(0, scrumTeam.EstimateParticipants.Count);
        }

        [TestMethod]
        public async Task ReconnectTeam_TeamAndMemberName_ReturnsScrumTeamWithEstimateInProgress()
        {
            string estimationParticipantsJson = PlanningPokerClientData.GetEstimateParticipantsJson(scrumMaster: false, member: true);
            string scrumTeamJson = PlanningPokerClientData.GetScrumTeamJson(member: true, state: 1, estimationParticipants: estimationParticipantsJson);
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.When(BaseUrl + $"api/PlanningPokerService/ReconnectTeam")
                .Respond(JsonType, PlanningPokerClientData.GetReconnectTeamResultJson(scrumTeamJson, lastMessageId: "1", selectedEstimate: "null"));
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            ReconnectTeamResult result = await target.ReconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

            Assert.IsNotNull(result.ScrumTeam);
            Assert.AreEqual(1, result.LastMessageId);
            Assert.IsNotNull(result.SelectedEstimate);
            Assert.IsNull(result.SelectedEstimate.Value);

            ScrumTeam scrumTeam = result.ScrumTeam;
            Assert.AreEqual(TeamState.EstimateInProgress, scrumTeam.State);
            Assert.AreEqual(PlanningPokerClientData.TeamName, scrumTeam.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, scrumTeam.ScrumMaster.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, scrumTeam.ScrumMaster.Type);

            Assert.AreEqual(2, scrumTeam.Members.Count);
            TeamMember member = scrumTeam.Members[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterType, member.Type);

            member = scrumTeam.Members[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, member.Name);
            Assert.AreEqual(PlanningPokerClientData.MemberType, member.Type);

            Assert.AreEqual(0, scrumTeam.Observers.Count);

            AssertAvailableEstimates(scrumTeam);

            Assert.AreEqual(0, scrumTeam.EstimateResult.Count);

            Assert.AreEqual(2, scrumTeam.EstimateParticipants.Count);
            EstimateParticipantStatus estimationParticipant = scrumTeam.EstimateParticipants[0];
            Assert.AreEqual(PlanningPokerClientData.ScrumMasterName, estimationParticipant.MemberName);
            Assert.IsFalse(estimationParticipant.Estimated);

            estimationParticipant = scrumTeam.EstimateParticipants[1];
            Assert.AreEqual(PlanningPokerClientData.MemberName, estimationParticipant.MemberName);
            Assert.IsTrue(estimationParticipant.Estimated);
        }

        [TestMethod]
        public async Task DisconnectTeam_TeamAndMemberName_RequestsDisconnectTeamUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/DisconnectTeam?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.DisconnectTeam(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task StartEstimate_TeamName_RequestsStartEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/StartEstimate?teamName={PlanningPokerClientData.TeamName}")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.StartEstimate(PlanningPokerClientData.TeamName, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task CancelEstimate_TeamName_RequestsCancelEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/CancelEstimate?teamName={PlanningPokerClientData.TeamName}")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.CancelEstimate(PlanningPokerClientData.TeamName, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SubmitEstimate_3_RequestsSubmitEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimate?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimate=3")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.SubmitEstimate(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 3, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SubmitEstimate_0_RequestsSubmitEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimate?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.ScrumMasterName}&estimate=0")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.SubmitEstimate(PlanningPokerClientData.TeamName, PlanningPokerClientData.ScrumMasterName, 0, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SubmitEstimate_100_RequestsSubmitEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimate?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimate=100")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.SubmitEstimate(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, 100, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SubmitEstimate_PositiveInfinity_RequestsSubmitEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimate?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimate=-1111100")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.SubmitEstimate(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, double.PositiveInfinity, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task SubmitEstimate_Null_RequestsSubmitEstimateUrl()
        {
            MockHttpMessageHandler httpMock = new MockHttpMessageHandler();
            httpMock.Expect(BaseUrl + $"api/PlanningPokerService/SubmitEstimate?teamName={PlanningPokerClientData.TeamName}&memberName={PlanningPokerClientData.MemberName}&estimate=-1111111")
                .Respond(TextType, string.Empty);
            PlanningPokerClient target = CreatePlanningPokerClient(httpMock);

            await target.SubmitEstimate(PlanningPokerClientData.TeamName, PlanningPokerClientData.MemberName, null, CancellationToken.None);

            httpMock.VerifyNoOutstandingExpectation();
        }

        internal static PlanningPokerClient CreatePlanningPokerClient(MockHttpMessageHandler messageHandler)
        {
            HttpClient httpClient = messageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri(BaseUrl);
            return new PlanningPokerClient(httpClient);
        }

        private static void AssertAvailableEstimates(ScrumTeam scrumTeam)
        {
            Assert.AreEqual(13, scrumTeam.AvailableEstimates.Count);
            Assert.AreEqual(0.0, scrumTeam.AvailableEstimates[0].Value);
            Assert.AreEqual(0.5, scrumTeam.AvailableEstimates[1].Value);
            Assert.AreEqual(1.0, scrumTeam.AvailableEstimates[2].Value);
            Assert.AreEqual(2.0, scrumTeam.AvailableEstimates[3].Value);
            Assert.AreEqual(3.0, scrumTeam.AvailableEstimates[4].Value);
            Assert.AreEqual(5.0, scrumTeam.AvailableEstimates[5].Value);
            Assert.AreEqual(8.0, scrumTeam.AvailableEstimates[6].Value);
            Assert.AreEqual(13.0, scrumTeam.AvailableEstimates[7].Value);
            Assert.AreEqual(20.0, scrumTeam.AvailableEstimates[8].Value);
            Assert.AreEqual(40.0, scrumTeam.AvailableEstimates[9].Value);
            Assert.AreEqual(100.0, scrumTeam.AvailableEstimates[10].Value);
            Assert.AreEqual(100.0, scrumTeam.AvailableEstimates[10].Value);
            Assert.IsTrue(double.IsPositiveInfinity(scrumTeam.AvailableEstimates[11].Value.Value));
            Assert.IsNull(scrumTeam.AvailableEstimates[12].Value);
        }
    }
}
