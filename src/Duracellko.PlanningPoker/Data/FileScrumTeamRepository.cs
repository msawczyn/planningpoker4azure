﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Data
{
    /// <summary>
    /// Repository of scrum teams using file system storage.
    /// </summary>
    public class FileScrumTeamRepository : IScrumTeamRepository
    {
        private const char SpecialCharacter = '%';
        private const string FileExtension = ".team";

        private readonly IFileScrumTeamRepositorySettings _settings;
        private readonly IPlanningPokerConfiguration _configuration;
        private readonly DateTimeProvider _dateTimeProvider;
        private readonly Lazy<string> _folder;
        private readonly Lazy<char[]> _invalidCharacters;
        private readonly ILogger<FileScrumTeamRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScrumTeamRepository" /> class.
        /// </summary>
        /// <param name="settings">The repository settings.</param>
        /// <param name="configuration">The configuration of the planning poker.</param>
        /// <param name="dateTimeProvider">The date-time provider.</param>
        /// <param name="logger">Logger instance to log events.</param>
        public FileScrumTeamRepository(IFileScrumTeamRepositorySettings settings, IPlanningPokerConfiguration configuration, DateTimeProvider dateTimeProvider, ILogger<FileScrumTeamRepository> logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _configuration = configuration ?? new PlanningPokerConfiguration();
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
            _folder = new Lazy<string>(GetFolder);
            _invalidCharacters = new Lazy<char[]>(GetInvalidCharacters);
            _logger = logger;
        }

        /// <summary>
        /// Gets the folder with stored scrum teams.
        /// </summary>
        /// <value>
        /// The storage folder.
        /// </value>
        public string Folder
        {
            get
            {
                return _folder.Value;
            }
        }

        /// <summary>
        /// Gets a collection of Scrum team names.
        /// </summary>
        public IEnumerable<string> ScrumTeamNames
        {
            get
            {
                _logger?.LogDebug(Resources.Repository_Debug_LoadScrumTeamNames);

                DateTime expirationTime = _dateTimeProvider.UtcNow - _configuration.RepositoryTeamExpiration;
                DirectoryInfo directory = new DirectoryInfo(Folder);
                if (directory.Exists)
                {
                    FileInfo[] files = directory.GetFiles("*" + FileExtension);
                    foreach (FileInfo file in files)
                    {
                        if (file.LastWriteTimeUtc >= expirationTime)
                        {
                            string teamName = GetScrumTeamName(file.Name);
                            if (teamName != null)
                            {
                                yield return teamName;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the Scrum team from repository.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>The Scrum team with specified name.</returns>
        public ScrumTeam LoadScrumTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException(nameof(teamName));
            }

            string file = GetFileName(teamName);
            file = Path.Combine(Folder, file);

            ScrumTeam result = null;
            if (File.Exists(file))
            {
                try
                {
                    using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        result = DeserializeScrumTeam(stream);
                    }
                }
                catch (IOException)
                {
                    // file is not accessible
                    result = null;
                }
                catch (SerializationException)
                {
                    // file is currupted
                    result = null;
                }
            }

            if (result != null)
            {
                _logger?.LogDebug(Resources.Repository_Debug_LoadScrumTeam, result.Name);
            }

            return result;
        }

        /// <summary>
        /// Saves the Scrum team to repository.
        /// </summary>
        /// <param name="team">The Scrum team.</param>
        public void SaveScrumTeam(ScrumTeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            InitializeFolder();

            string file = GetFileName(team.Name);
            file = Path.Combine(Folder, file);

            using (FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SerializeScrumTeam(team, stream);
            }

            _logger?.LogDebug(Resources.Repository_Debug_SaveScrumTeam, team.Name);
        }

        /// <summary>
        /// Deletes the Scrum team from repository.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        public void DeleteScrumTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                throw new ArgumentNullException(nameof(teamName));
            }

            string file = GetFileName(teamName);
            file = Path.Combine(Folder, file);

            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    _logger?.LogDebug(Resources.Repository_Debug_DeleteScrumTeam, teamName);
                }
                catch (IOException)
                {
                    // ignore, file might be already deleted
                }
            }
        }

        /// <summary>
        /// Deletes the expired Scrum teams, which were not used for period of expiration time.
        /// </summary>
        public void DeleteExpiredScrumTeams()
        {
            _logger?.LogDebug(Resources.Repository_Debug_DeleteExpiredScrumTeams);

            DateTime expirationTime = _dateTimeProvider.UtcNow - _configuration.RepositoryTeamExpiration;
            DirectoryInfo directory = new DirectoryInfo(Folder);
            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles("*" + FileExtension);
                foreach (FileInfo file in files)
                {
                    try
                    {
                        if (file.LastWriteTimeUtc < expirationTime)
                        {
                            file.Delete();
                        }
                    }
                    catch (Exception)
                    {
                        // ignore and continue
                    }
                }
            }
        }

        /// <summary>
        /// Deletes all Scrum teams.
        /// </summary>
        public void DeleteAll()
        {
            _logger?.LogDebug(Resources.Repository_Debug_DeleteAllScrumTeams);

            DirectoryInfo directory = new DirectoryInfo(Folder);
            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles("*" + FileExtension);
                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                        // ignore and continue
                    }
                }
            }
        }

        private static char[] GetInvalidCharacters()
        {
            char[] invalidFileCharacters = Path.GetInvalidFileNameChars();
            char[] result = new char[invalidFileCharacters.Length + 1];
            result[0] = SpecialCharacter;
            invalidFileCharacters.CopyTo(result, 1);
            Array.Sort<char>(result, Comparer<char>.Default);
            return result;
        }

        private static string GetScrumTeamName(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);

            StringBuilder result = new StringBuilder(name.Length);
            int specialPosition = 0;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (specialPosition == 0)
                {
                    if (c == SpecialCharacter)
                    {
                        if (name.Length <= i + 5)
                        {
                            return null;
                        }

                        int special = 0;
                        if (int.TryParse(name.Substring(i + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out special))
                        {
                            result.Append((char)special);
                        }
                        else
                        {
                            return null;
                        }

                        specialPosition = 4;
                    }
                    else
                    {
                        result.Append(c);
                    }
                }
                else
                {
                    specialPosition--;
                }
            }

            return result.ToString();
        }

        private static void SerializeScrumTeam(ScrumTeam team, Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence));
            formatter.Serialize(stream, team);
        }

        private string GetFolder()
        {
            return _settings.Folder;
        }

        private void InitializeFolder()
        {
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }
        }

        private string GetFileName(string teamName)
        {
            StringBuilder result = new StringBuilder(teamName.Length + 10);
            char[] invalidChars = _invalidCharacters.Value;
            for (int i = 0; i < teamName.Length; i++)
            {
                char c = teamName[i];
                bool isSpecial = Array.BinarySearch<char>(invalidChars, c, Comparer<char>.Default) >= 0;
                isSpecial = isSpecial || (c != ' ' && char.IsWhiteSpace(c));

                if (isSpecial)
                {
                    result.Append(SpecialCharacter);
                    result.Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                }
                else
                {
                    result.Append(c);
                }
            }

            result.Append(FileExtension);
            return result.ToString();
        }

        private ScrumTeam DeserializeScrumTeam(Stream stream)
        {
            BinaryFormatter formatter = _dateTimeProvider != null ?
                new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence, _dateTimeProvider)) :
                new BinaryFormatter();
            return (ScrumTeam)formatter.Deserialize(stream);
        }
    }
}
