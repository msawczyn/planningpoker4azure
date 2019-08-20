using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Objects provides operations of Planning Poker service.
    /// </summary>
    public class PlanningPokerClient : IPlanningPokerClient
    {
        private const string BaseUri = "api/PlanningPokerService/";
        private readonly HttpClient _client;
        private readonly UrlEncoder _urlEncoder = UrlEncoder.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerClient"/> class.
        /// </summary>
        /// <param name="client">HttpClient used for HTTP communication with server.</param>
        public PlanningPokerClient(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        public async Task<ScrumTeam> CreateTeam(string teamName, string scrumMasterName, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string encodedScrumMasterName = _urlEncoder.Encode(scrumMasterName);
            string uri = $"CreateTeam?teamName={encodedTeamName}&scrumMasterName={encodedScrumMasterName}";

            ScrumTeam result = await GetJsonAsync<ScrumTeam>(uri, cancellationToken);

            ConvertScrumTeam(result);
            return result;
        }

        /// <summary>
        /// Connects member or observer with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member or observer.</param>
        /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The Scrum team the member or observer joined to.
        /// </returns>
        public async Task<ScrumTeam> JoinTeam(string teamName, string memberName, bool asObserver, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string encodedMemberName = _urlEncoder.Encode(memberName);
            string encodedAsObserver = asObserver.ToString(CultureInfo.InvariantCulture);
            string uri = $"JoinTeam?teamName={encodedTeamName}&memberName={encodedMemberName}&asObserver={encodedAsObserver}";

            ScrumTeam result = await GetJsonAsync<ScrumTeam>(uri, cancellationToken);

            ConvertScrumTeam(result);
            return result;
        }

        /// <summary>
        /// Reconnects member with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The Scrum team the member or observer reconnected to.
        /// </returns>
        /// <remarks>
        /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
        /// </remarks>
        public async Task<ReconnectTeamResult> ReconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string encodedMemberName = _urlEncoder.Encode(memberName);
            string uri = $"ReconnectTeam?teamName={encodedTeamName}&memberName={encodedMemberName}";

            ReconnectTeamResult result = await GetJsonAsync<ReconnectTeamResult>(uri, cancellationToken);

            ConvertScrumTeam(result.ScrumTeam);
            ConvertEstimate(result.SelectedEstimate);
            return result;
        }

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task DisconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string encodedMemberName = _urlEncoder.Encode(memberName);
            string uri = $"DisconnectTeam?teamName={encodedTeamName}&memberName={encodedMemberName}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Signal from Scrum master to starts the estimate.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task StartEstimate(string teamName, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string uri = $"StartEstimate?teamName={encodedTeamName}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Signal from Scrum master to cancels the estimate.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task CancelEstimate(string teamName, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string uri = $"CancelEstimate?teamName={encodedTeamName}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Submits the estimate for specified team member.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimate">The estimate the member is submitting.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task SubmitEstimate(string teamName, string memberName, double? estimate, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string encodedMemberName = _urlEncoder.Encode(memberName);
            string encodedEstimate;
            if (!estimate.HasValue)
            {
                encodedEstimate = "-1111111";
            }
            else if (double.IsPositiveInfinity(estimate.Value))
            {
                encodedEstimate = "-1111100";
            }
            else
            {
                encodedEstimate = _urlEncoder.Encode(estimate.Value.ToString(CultureInfo.InvariantCulture));
            }

            string uri = $"SubmitEstimate?teamName={encodedTeamName}&memberName={encodedMemberName}&estimate={encodedEstimate}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// List of messages.
        /// </returns>
        public async Task<IList<Message>> GetMessages(string teamName, string memberName, long lastMessageId, CancellationToken cancellationToken)
        {
            string encodedTeamName = _urlEncoder.Encode(teamName);
            string encodedMemberName = _urlEncoder.Encode(memberName);
            string encodedLastMessageId = _urlEncoder.Encode(lastMessageId.ToString(CultureInfo.InvariantCulture));
            string uri = $"GetMessages?teamName={encodedTeamName}&memberName={encodedMemberName}&lastMessageId={encodedLastMessageId}";

            return await GetJsonAsync<List<Message>>(uri, cancellationToken);
        }

        private static void DeserializeMessages(List<Message> messages, string json)
        {
            List<MemberMessage> memberMessages = Json.Deserialize<List<MemberMessage>>(json);
            List<EstimateResultMessage> estimationResultMessages = Json.Deserialize<List<EstimateResultMessage>>(json);

            for (int i = 0; i < messages.Count; i++)
            {
                Message message = messages[i];
                switch (message.Type)
                {
                    case MessageType.MemberJoined:
                    case MessageType.MemberDisconnected:
                    case MessageType.MemberEstimated:
                        messages[i] = memberMessages[i];
                        break;
                    case MessageType.EstimateEnded:
                        EstimateResultMessage estimationResultMessage = estimationResultMessages[i];
                        ConvertEstimates(estimationResultMessage.EstimateResult);
                        messages[i] = estimationResultMessage;
                        break;
                }
            }
        }

        private static void ConvertScrumTeam(ScrumTeam scrumTeam)
        {
            if (scrumTeam.AvailableEstimates != null)
            {
                ConvertEstimates(scrumTeam.AvailableEstimates);
            }

            if (scrumTeam.EstimateResult != null)
            {
                ConvertEstimates(scrumTeam.EstimateResult);
            }
        }

        private static void ConvertEstimates(IEnumerable<Estimate> estimates)
        {
            foreach (Estimate estimate in estimates)
            {
                ConvertEstimate(estimate);
            }
        }

        private static void ConvertEstimates(IEnumerable<EstimateResultItem> estimationResultItems)
        {
            foreach (EstimateResultItem estimationResultItem in estimationResultItems)
            {
                ConvertEstimate(estimationResultItem.Estimate);
            }
        }

        private static void ConvertEstimate(Estimate estimate)
        {
            if (estimate != null && estimate.Value == Estimate.PositiveInfinity)
            {
                estimate.Value = double.PositiveInfinity;
            }
        }

        private async Task<T> GetJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseUri + requestUri))
                {
                    using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken))
                    {
                        if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            throw new PlanningPokerException(content);
                        }
                        else if (!response.IsSuccessStatusCode)
                        {
                            throw new PlanningPokerException(Client.Resources.PlanningPokerService_UnexpectedError);
                        }

                        string responseContent = await response.Content.ReadAsStringAsync();
                        T result = Json.Deserialize<T>(responseContent);
                        if (result is List<Message> messages)
                        {
                            DeserializeMessages(messages, responseContent);
                        }

                        return result;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new PlanningPokerException(Client.Resources.PlanningPokerService_ConnectionError, ex);
            }
        }

        private async Task SendAsync(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseUri + requestUri))
                {
                    using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken))
                    {
                        if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            throw new PlanningPokerException(content);
                        }
                        else if (!response.IsSuccessStatusCode)
                        {
                            throw new PlanningPokerException(Client.Resources.PlanningPokerService_UnexpectedError);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new PlanningPokerException(Client.Resources.PlanningPokerService_ConnectionError, ex);
            }
        }
    }
}
