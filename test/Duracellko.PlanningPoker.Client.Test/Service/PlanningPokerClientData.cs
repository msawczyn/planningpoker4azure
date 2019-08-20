using System.Globalization;
using System.Text;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    public static class PlanningPokerClientData
    {
        public const string ScrumMasterType = "ScrumMaster";
        public const string MemberType = "Member";
        public const string ObserverType = "Observer";

        public const string TeamName = "Test team";
        public const string ScrumMasterName = "Test Scrum Master";
        public const string MemberName = "Test member";
        public const string ObserverName = "Test observer";

        public static string GetScrumTeamJson(bool member = false, bool observer = false, int state = 0, string estimationResult = "", string estimationParticipants = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine(@"""name"": ""Test team"",");
            sb.AppendLine(@"    ""scrumMaster"": {
        ""name"": ""Test Scrum Master"",
        ""type"": ""ScrumMaster""
    },
");
            sb.AppendLine(@"    ""members"": [");
            sb.Append(@"        {
            ""name"": ""Test Scrum Master"",
            ""type"": ""ScrumMaster""
        }");
            if (member)
            {
                sb.Append(',');
            }

            sb.AppendLine();

            if (member)
            {
                sb.AppendLine(@"        {
            ""name"": ""Test member"",
            ""type"": ""Member""
        }");
            }

            sb.AppendLine("],");

            sb.AppendLine(@"    ""observers"": [");
            if (observer)
            {
                sb.AppendLine(@"        {
            ""name"": ""Test observer"",
            ""type"": ""Observer""
        }");
            }

            sb.AppendLine("],");

            sb.AppendFormat(CultureInfo.InvariantCulture, @"    ""state"": {0},", state);
            sb.AppendLine();

            sb.AppendLine(@"    ""availableEstimates"": [
        { ""value"": 0 },
        { ""value"": 0.5 },
        { ""value"": 1 },
        { ""value"": 2 },
        { ""value"": 3 },
        { ""value"": 5 },
        { ""value"": 8 },
        { ""value"": 13 },
        { ""value"": 20 },
        { ""value"": 40 },
        { ""value"": 100 },
        { ""value"": -1111100 },
        { ""value"": null }
    ],");

            sb.AppendLine(@"    ""estimationResult"": [");
            sb.AppendLine(estimationResult);
            sb.AppendLine("],");

            sb.AppendLine(@"    ""estimationParticipants"": [");
            sb.AppendLine(estimationParticipants);
            sb.AppendLine("]");

            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string GetEstimateResultJson(string scrumMasterEstimate = "5", string memberEstimate = "20")
        {
            string scrumMasterEstimateJson = string.Empty;
            if (scrumMasterEstimate != null)
            {
                scrumMasterEstimateJson = @",
            ""estimate"": {
                ""value"": " + scrumMasterEstimate + @"
            }";
            }

            string memberEstimateJson = string.Empty;
            if (memberEstimate != null)
            {
                memberEstimateJson = @",
            ""estimate"": {
                ""value"": " + memberEstimate + @"
            }";
            }

            return @"{
            ""member"": {
                ""name"": ""Test Scrum Master"",
                ""type"": ""ScrumMaster""
            }" + scrumMasterEstimateJson + @"
        },
        {
            ""member"": {
                ""name"": ""Test member"",
                ""type"": ""Member""
            }" + memberEstimateJson + @"
        }";
        }

        public static string GetEstimateParticipantsJson(bool scrumMaster = true, bool member = false)
        {
            return @"{
                ""memberName"": ""Test Scrum Master"",
                ""estimated"": " + (scrumMaster ? "true" : "false") + @"
            },
            {
                ""memberName"": ""Test member"",
                ""estimated"": " + (member ? "true" : "false") + @"
            }";
        }

        public static string GetReconnectTeamResultJson(string scrumTeamJson, string lastMessageId = "0", string selectedEstimate = null)
        {
            string selectedEstimateJson = string.Empty;
            if (selectedEstimate != null)
            {
                selectedEstimateJson = @",
                ""selectedEstimate"": {
                    ""value"": " + selectedEstimate + @"
                }";
            }

            return @"{
            ""lastMessageId"": " + lastMessageId + @",
            ""scrumTeam"": " + scrumTeamJson + selectedEstimateJson + @"
}";
        }

        public static string GetMessagesJson(params string[] messages)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            sb.AppendJoin(",\r\n", messages);
            sb.AppendLine();
            sb.Append(']');
            return sb.ToString();
        }

        public static string GetEmptyMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 0
}";
        }

        public static string GetMemberJoinedMessageJson(string id = "0", string name = MemberName, string type = MemberType)
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 1,
            ""member"": {
                ""name"": """ + name + @""",
                ""type"": """ + type + @"""
            }
}";
        }

        public static string GetMemberDisconnectedMessageJson(string id = "0", string name = MemberName, string type = MemberType)
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 2,
            ""member"": {
                ""name"": """ + name + @""",
                ""type"": """ + type + @"""
            }
}";
        }

        public static string GetEstimateStartedMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 3
}";
        }

        public static string GetEstimateEndedMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 4,
            ""estimationResult"": [
                {
                    ""member"": {
                        ""name"": ""Test Scrum Master"",
                        ""type"": ""ScrumMaster""
                    },
                    ""estimate"": {
                        ""value"": 2
                    }
                },
                {
                    ""member"": {
                        ""name"": ""Test member"",
                        ""type"": ""Member""
                    },
                    ""estimate"": {
                        ""value"": null
                    }
                },
                {
                    ""member"": {
                        ""name"": ""Me"",
                        ""type"": ""Member""
                    },
                    ""estimate"": null
                },
                {
                    ""member"": {
                        ""name"": ""Test observer"",
                        ""type"": ""Member""
                    },
                    ""estimate"": {
                        ""value"": -1111100
                    }
                }
            ]
}";
        }

        public static string GetEstimateEndedMessage2Json(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 4,
            ""estimationResult"": [
                {
                    ""member"": {
                        ""name"": ""Test Scrum Master"",
                        ""type"": ""ScrumMaster""
                    },
                    ""estimate"": {
                        ""value"": 5
                    }
                },
                {
                    ""member"": {
                        ""name"": ""Test member"",
                        ""type"": ""Member""
                    },
                    ""estimate"": {
                        ""value"": 40
                    }
                }
            ]
}";
        }

        public static string GetEstimateCanceledMessageJson(string id = "0")
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 5
}";
        }

        public static string GetMemberEstimatedMessageJson(string id = "0", string name = ScrumMasterName, string type = ScrumMasterType)
        {
            return @"{
            ""id"": " + id + @",
            ""type"": 6,
            ""member"": {
                ""name"": """ + name + @""",
                ""type"": """ + type + @"""
            }
}";
        }
    }
}
