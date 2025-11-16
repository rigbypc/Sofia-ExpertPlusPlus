using System;
using System.Collections.Generic;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class CommitBasedKnowledgeMap
    {
        private readonly HashSet<string> _mapCommits = new HashSet<string>();
        private readonly Dictionary<string, List<Commit>> _mapDeveloperCommit = new Dictionary<string, List<Commit>>();
        private readonly Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>> _map = new Dictionary<string, Dictionary<string, DeveloperFileCommitDetail>>();
        private static List<Commit> _emptyList = new List<Commit>(0);

        public Dictionary<string, DeveloperFileCommitDetail> this[string filePath]
        {
            get
            {
                return _map.GetValueOrDefault(filePath);
            }
        }

        public IEnumerable<DeveloperFileCommitDetail> Details => _map.Values.SelectMany(q => q.Values);

        internal IEnumerable<DeveloperFileCommitDetail> GetCommittersOfPeriod(long periodId)
        {
            return _map.Values.SelectMany(q => q.Values.Where(c => c.Periods.Any(p => p.Id == periodId)));
        }

        public void Add(string filePath, LibGit2Sharp.ChangeKind changeKind, Developer developer, Commit commit, Period period)
        {
            var developerName = developer?.NormalizedName;

            if (filePath == null || developerName == null)
            {
                return;
            }

            if (!_map.ContainsKey(filePath))
            {
                _map[filePath] = new Dictionary<string, DeveloperFileCommitDetail>();
            }

            if (!_map[filePath].ContainsKey(developerName))
            {
                _map[filePath][developerName] = new DeveloperFileCommitDetail()
                {
                    FilePath = filePath,
                    Developer = developer,
                };
            }

            if (!_map[filePath][developerName].Commits.Any(q => q.Sha == commit?.Sha))
            {
                _map[filePath][developerName].CommitDetails.Add(new CommitDetail(commit, period, changeKind));
                _map[filePath][developerName].Commits.Add(commit);
                UpdateDeveloperCommits(commit, developerName);

            }

            if (!_map[filePath][developerName].Periods.Any(q => q.Id == period.Id))
            {
                _map[filePath][developerName].Periods.Add(period);
            }
        }

        private void UpdateDeveloperCommits(Commit commit, string developerName)
        {
            if (_mapCommits.Contains(commit.Sha))
                return;

            _mapCommits.Add(commit.Sha);

            if (!_mapDeveloperCommit.ContainsKey(developerName))
            {
                _mapDeveloperCommit[developerName] = new List<Commit>();
            }

            _mapDeveloperCommit[developerName].Add(commit);
        }

        internal bool IsPersonHasCommittedThisFile(string normalizedName, string path)
        {
            var developersFileCommitsDetails = _map.GetValueOrDefault(path);

            if (developersFileCommitsDetails == null)
            {
                return false;
            }

            return developersFileCommitsDetails.Any(q => q.Value.Developer.NormalizedName == normalizedName);
        }

        internal void Remove(string filePath, Developer developer, Commit commit, Period period)
        {
            if (_map.ContainsKey(filePath))
            {
                _map.Remove(filePath);
                UpdateDeveloperCommits(commit, developer.NormalizedName);
            }
        }

        public List<Commit> GetDeveloperCommits(string developerName)
        {
            if (_mapDeveloperCommit.ContainsKey(developerName))
            {
                return _mapDeveloperCommit[developerName];
            }

            return _emptyList;
        }


        internal Dictionary<string, List<Commit>> GetCommitters()
        {
            return _mapDeveloperCommit;
        }

        public (int developerTotalCommit,
                DateTime? developerRecentCommit,
                int fileTotalCommit,
                DateTime? fileRecentCommit)
            GetDeveloperAuthorship(string filePath,
                                string developerName,
                                DateTime pullReqTime)
        {
            // Initialize outputs
            int developerTotalCommit = 0;
            DateTime? developerRecentCommit = null;
            int fileTotalCommit = 0;
            DateTime? fileRecentCommit = null;

            // If the file isn't tracked at all, bail out
            if (!_map.ContainsKey(filePath))
                return (developerTotalCommit,
                        developerRecentCommit,
                        fileTotalCommit,
                        fileRecentCommit);

            var fileCommitDetails = _map[filePath];

            // 1) Developer-specific stats, filtered:
            if (fileCommitDetails.TryGetValue(developerName, out var devDetail))
            {
                // Only commits _before_ pullReqTime
                var devFiltered = devDetail.Commits
                                    .Where(c => c.AuthorDateTime < pullReqTime)
                                    .ToList();

                developerTotalCommit = devFiltered.Count;

                if (developerTotalCommit > 0)
                    developerRecentCommit = devFiltered
                                            .Max(c => c.AuthorDateTime);
            }

            // 2) File-wide stats, filtered:
            foreach (var devCommitDetail in fileCommitDetails.Values)
            {
                // Filter each developer’s commits by the same cut-off
                var filtered = devCommitDetail.Commits
                                .Where(c => c.AuthorDateTime < pullReqTime)
                                .ToList();

                fileTotalCommit += filtered.Count;

                if (filtered.Any())
                {
                    var mostRecent = filtered.Max(c => c.AuthorDateTime);
                    if (fileRecentCommit == null || mostRecent > fileRecentCommit)
                        fileRecentCommit = mostRecent;
                }
            }

            return (developerTotalCommit,
                    developerRecentCommit,
                    fileTotalCommit,
                    fileRecentCommit);
        }

        public int GetDeveloperCommitsOnFile(string normalizedName, string path, DateTime pullReqTime, out DateTime recent)
        {
            var developersFileCommitsDetails = _map.GetValueOrDefault(path);
            if (developersFileCommitsDetails == null)
            {
                recent = DateTime.MinValue;
                return 0;
            }

            var developercommits = developersFileCommitsDetails.Where(a => a.Key == normalizedName).FirstOrDefault();
            if (developercommits.Value == null)
            {
                recent = DateTime.MinValue;
                return 0;
            }

            recent = DateTime.MinValue;
            var filtered = developercommits.Value.Commits.Where(b => b.AuthorDateTime < pullReqTime);
            if (filtered.Count() > 0)
            {
                recent = filtered.Max(a => a.CommitterDateTime);
            }

            return filtered.Count();
        }
    }
}
