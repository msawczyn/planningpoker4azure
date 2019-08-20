using System.Collections.Generic;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    public static class PlanningPokerData
    {
        public const string ScrumMasterType = "ScrumMaster";
        public const string MemberType = "Member";
        public const string ObserverType = "Observer";

        public const string TeamName = "Test team";
        public const string ScrumMasterName = "Test Scrum Master";
        public const string MemberName = "Test member";
        public const string ObserverName = "Test observer";

        public static ScrumTeam GetScrumTeam()
        {
            return new ScrumTeam
            {
                Name = TeamName,
                ScrumMaster = new TeamMember
                {
                    Name = ScrumMasterName,
                    Type = ScrumMasterType
                },
                Members = new List<TeamMember>()
                {
                    new TeamMember
                    {
                        Name = ScrumMasterName,
                        Type = ScrumMasterType
                    },
                    new TeamMember
                    {
                        Name = MemberName,
                        Type = MemberType
                    }
                },
                Observers = new List<TeamMember>()
                {
                    new TeamMember
                    {
                        Name = ObserverName,
                        Type = ObserverType
                    }
                },
                State = TeamState.Initial,
                AvailableEstimates = GetAvailableEstimates()
            };
        }

        public static ScrumTeam GetInitialScrumTeam()
        {
            return new ScrumTeam
            {
                Name = TeamName,
                ScrumMaster = new TeamMember
                {
                    Name = ScrumMasterName,
                    Type = ScrumMasterType
                },
                State = TeamState.Initial,
                AvailableEstimates = GetAvailableEstimates()
            };
        }

        public static IList<Estimate> GetAvailableEstimates()
        {
            return new List<Estimate>
            {
                new Estimate() { Value = 0.0 },
                new Estimate() { Value = 0.5 },
                new Estimate() { Value = 1.0 },
                new Estimate() { Value = 2.0 },
                new Estimate() { Value = 3.0 },
                new Estimate() { Value = 5.0 },
                new Estimate() { Value = 8.0 },
                new Estimate() { Value = 13.0 },
                new Estimate() { Value = 20.0 },
                new Estimate() { Value = 40.0 },
                new Estimate() { Value = 100.0 },
                new Estimate() { Value = double.PositiveInfinity },
                new Estimate() { Value = null }
            };
        }

        public static ReconnectTeamResult GetReconnectTeamResult()
        {
            return new ReconnectTeamResult
            {
                ScrumTeam = GetScrumTeam(),
                LastMessageId = 123
            };
        }

        public static MemberCredentials GetMemberCredentials()
        {
            return new MemberCredentials
            {
                TeamName = TeamName,
                MemberName = MemberName
            };
        }
    }
}
