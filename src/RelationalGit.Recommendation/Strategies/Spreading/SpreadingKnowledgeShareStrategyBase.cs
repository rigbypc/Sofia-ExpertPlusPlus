using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace RelationalGit.Recommendation
{
    public abstract class SpreadingKnowledgeShareStrategyBase : RecommendationStrategy
    {
        public SpreadingKnowledgeShareStrategyBase(string knowledgeSaveReviewerReplacementType, ILogger logger, bool changePast, string simulationType)
            : base(knowledgeSaveReviewerReplacementType, logger, changePast, simulationType)
        {
        }

        protected override PullRequestRecommendationResult RecommendReviewers(PullRequestContext pullRequestContext)
        {
            var availableDevs = AvailablePRKnowledgeables(pullRequestContext);

            if (availableDevs.Length == 0)
            {
                return Actual(pullRequestContext);
            }

            var simulationResults = new List<PullRequestKnowledgeDistribution>();
            var simulator = new PullRequestReviewSimulator(pullRequestContext, availableDevs, ComputeScore);

            foreach (var candidateSet in GetPossibleCandidateSets(pullRequestContext, availableDevs))
            {
                var simulationResult = simulator.Simulate(candidateSet.Reviewers, candidateSet.SelectedCandidateKnowledge);
                simulationResults.Add(simulationResult);
            }

            if (simulationResults.Count == 0)
            {
                return Actual(pullRequestContext);
            }

            var bestPullRequestKnowledgeDistribution = GetBestDistribution(simulationResults);

            return Recommendation(pullRequestContext, availableDevs, bestPullRequestKnowledgeDistribution);
        }

        private static PullRequestRecommendationResult Recommendation(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs, PullRequestKnowledgeDistribution bestPullRequestKnowledgeDistribution)
        {
            // Step 1: Compute the recommendation and swapped reviewer scores.
            double? recRevExp = null;
            if (availableDevs.Length != 0)
            {
                recRevExp = pullRequestContext.ComputeDeveloperBirdScore(availableDevs[0].DeveloperName);
            }

            var topCandidate = availableDevs.Length > 0 ? availableDevs[0] : null;
            var selectedReviewers = bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray();
            var actualReviewers = pullRequestContext.ActualReviewers.ToArray();
            var prFiles = pullRequestContext.PullRequestFiles
                .Select(q => pullRequestContext.CanononicalPathMapper.GetValueOrDefault(q.FileName));

            double? swappedRevExp = null;

            if (topCandidate == null || actualReviewers.Length == 0)
            {
                swappedRevExp = null;
            }
            else if (actualReviewers.SequenceEqual(selectedReviewers))
            {
                // If actualReviewers and selectedReviewers are the same, the top candidate replaces itself.
                swappedRevExp = pullRequestContext.ComputeDeveloperBirdScore(topCandidate.DeveloperName);
            }
            else if (actualReviewers.Length < selectedReviewers.Length)
            {
                // If a new reviewer is added, no one is swapped.
                swappedRevExp = 0;
            }
            else
            {
                // Find the swapped reviewer.
                var swappedRev = actualReviewers.Except(selectedReviewers).ToArray();
                if (swappedRev.Length > 0)
                {
                    swappedRevExp = pullRequestContext.ComputeDeveloperBirdScore(swappedRev[0].DeveloperName);
                }
                else if (selectedReviewers.Contains(topCandidate))
                {
                    swappedRevExp = pullRequestContext.ComputeDeveloperBirdScore(topCandidate.DeveloperName);
                }
            }

            // Step 2: Build the ActualBirdExpertise dictionary.
            var actualBirdExpertise = new Dictionary<string, double>();
            var actualExpertise = new Dictionary<string, double>();
            var actualAllTouchedFiles = new HashSet<string>();

            foreach (var reviewer in actualReviewers)
            {
                // Use a unique identifier (e.g., reviewer.Id) as the dictionary key.
                string key = reviewer.DeveloperName;
                double score = pullRequestContext.ComputeDeveloperBirdScore(reviewer.DeveloperName);
                actualBirdExpertise[key] = score;

                var touchedFiles = reviewer.GetTouchedFiles().ToHashSet();
                actualAllTouchedFiles.UnionWith(touchedFiles);
                double expertise = prFiles.Count(q => touchedFiles.Contains(q)) / (double)prFiles.Count();
                actualExpertise[key] = expertise;
            }

            double ActualOverallExpertise = prFiles.Count(q => actualAllTouchedFiles.Contains(q)) / (double)prFiles.Count();
            actualExpertise["Overall"] = ActualOverallExpertise;

            var selectedReviewersBirdExpertise = new Dictionary<string, double>();
            var selectedReviewersExpertise = new Dictionary<string, double>();
            var selectedReviewersAllTouchedFiles = new HashSet<string>();

            foreach (var reviewer in selectedReviewers)
            {
                // Use a unique identifier (e.g., reviewer.DeveloperName) as the dictionary key.
                string key = reviewer.DeveloperName;

                // Calculate Bird Score
                double birdScore = pullRequestContext.ComputeDeveloperBirdScore(reviewer.DeveloperName);
                selectedReviewersBirdExpertise[key] = birdScore;

                // Calculate Individual Expertise
                var touchedFiles = reviewer.GetTouchedFiles().ToHashSet();
                selectedReviewersAllTouchedFiles.UnionWith(touchedFiles); // Add to the global union
                double expertise = prFiles.Count(q => touchedFiles.Contains(q)) / (double)prFiles.Count();
                selectedReviewersExpertise[key] = expertise;
            }

            // Calculate Overall Expertise
            double selectedReviewersOverallExpertise = prFiles.Count(q => selectedReviewersAllTouchedFiles.Contains(q)) / (double)prFiles.Count();
            selectedReviewersExpertise["Overall"] = selectedReviewersOverallExpertise;

            // Step 3: Build the TopCandidateExpertise dictionary for the top 5 available developers.
            var topCandidateBirdExpertise = new Dictionary<string, double>();
            var topCandidateExpertise = new Dictionary<string, double>();
            var topCandidateAllTouchedFiles = new HashSet<string>();

            int limit = Math.Min(5, availableDevs.Length);
            for (int i = 0; i < limit; i++)
            {
                string key = availableDevs[i].DeveloperName;
                double score = pullRequestContext.ComputeDeveloperBirdScore(availableDevs[i].DeveloperName);
                topCandidateBirdExpertise[key] = score;

                var touchedFiles = availableDevs[i].GetTouchedFiles().ToHashSet();
                topCandidateAllTouchedFiles.UnionWith(touchedFiles); // Add to the global union
                double expertise = prFiles.Count(q => touchedFiles.Contains(q)) / (double)prFiles.Count();
                topCandidateExpertise[key] = expertise;

            }

            double topCandidateOverallExpertise = prFiles.Count(q => topCandidateAllTouchedFiles.Contains(q)) / (double)prFiles.Count();
            topCandidateExpertise["Overall"] = topCandidateOverallExpertise;

            string author = pullRequestContext.PRSubmitterNormalizedName;
            var AuthorBirdExpertise = new Dictionary<string, double>();
            var AuthorExpertise = new Dictionary<string, double>();

            if (!string.IsNullOrEmpty(pullRequestContext.PRSubmitterNormalizedName))
            {
                AuthorBirdExpertise[author] = pullRequestContext.ComputeDeveloperBirdScore(author);

                var touchedFiles = pullRequestContext.PullRequestKnowledgeables
                                    .FirstOrDefault(q => q.DeveloperName == pullRequestContext.PRSubmitterNormalizedName)
                                ?.GetTouchedFiles().ToHashSet()
                                ?? new HashSet<string>();


                double expertise = prFiles.Count(q => touchedFiles.Contains(q)) / (double)prFiles.Count();
                AuthorExpertise[author] = expertise;

            }
            // Step 4: Create an anonymous object for the expertise entry.
            var BirdExpertiseEntry = new
            {
                LossSimulationId = (int?)null,
                PullRequestNumber = pullRequestContext.PullRequest.Number,
                DefectProneness = pullRequestContext.ComputeDefectPronenessScore(),
                ActualBirdExpertise = JsonConvert.SerializeObject(actualBirdExpertise),
                TopCandidateBirdExpertise = JsonConvert.SerializeObject(topCandidateBirdExpertise),
                SelectedReviewersBirdExpertise = JsonConvert.SerializeObject(selectedReviewersBirdExpertise),
                AuthorBirdExpertise = JsonConvert.SerializeObject(AuthorBirdExpertise)

            };

            var expertiseEntry = new
            {
                LossSimulationId = (int?)null,
                PullRequestNumber = pullRequestContext.PullRequest.Number,
                DefectProneness = pullRequestContext.ComputeDefectPronenessScore(),
                ActualExpertise = JsonConvert.SerializeObject(actualExpertise),
                TopCandidateExpertise = JsonConvert.SerializeObject(topCandidateExpertise),
                SelectedReviewersExpertise = JsonConvert.SerializeObject(selectedReviewersExpertise),
                AuthorExpertise = JsonConvert.SerializeObject(AuthorExpertise)
            };


            // Step 5: Insert the new row into the table using raw SQL to avoid entity type issues.
            using (var dbContext = GetDbContext())
            {
                dbContext.Database.ExecuteSqlCommand(
                    "INSERT INTO BirdExpertise (LossSimulationId, PullRequestNumber, DefectProneness, ActualExpertise, TopCandidateExpertise, SelectedReviewersExpertise, AuthorExpertise) " +
                    "VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                    null,
                    BirdExpertiseEntry.PullRequestNumber,
                    BirdExpertiseEntry.DefectProneness,
                    BirdExpertiseEntry.ActualBirdExpertise,
                    BirdExpertiseEntry.TopCandidateBirdExpertise,
                    BirdExpertiseEntry.SelectedReviewersBirdExpertise,
                    BirdExpertiseEntry.AuthorBirdExpertise);

                dbContext.Database.ExecuteSqlCommand(
                    "INSERT INTO Expertise (LossSimulationId, PullRequestNumber, DefectProneness, ActualExpertise, TopCandidateExpertise, SelectedReviewersExpertise, AuthorExpertise) " +
                    "VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                    null,
                    expertiseEntry.PullRequestNumber,
                    expertiseEntry.DefectProneness,
                    expertiseEntry.ActualExpertise,
                    expertiseEntry.TopCandidateExpertise,
                    expertiseEntry.SelectedReviewersExpertise,
                    expertiseEntry.AuthorExpertise);
            }

            // Step 6: Return the recommendation result.
            return new PullRequestRecommendationResult(
                bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray(),
                availableDevs,
                pullRequestContext.IsRisky(),
                pullRequestContext.Features,
                pullRequestContext.ComputeDefectPronenessScore(),
                //pullRequestContext.ComputeMaxRevExpertise(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray()),
                pullRequestContext.ComputeSumRevAutExpertise(bestPullRequestKnowledgeDistribution.PullRequestKnowledgeDistributionFactors.Reviewers.ToArray()),
                pullRequestContext.PullRequest.Number,
                recRevExp,
                swappedRevExp
            );
        }

        private static Data.GitRepositoryDbContext GetDbContext()
        {
            return new Data.GitRepositoryDbContext(autoDetectChangesEnabled: false);
        }

        private static PullRequestRecommendationResult Actual(PullRequestContext pullRequestContext)
        {
            return new PullRequestRecommendationResult(
                pullRequestContext.ActualReviewers,
                Array.Empty<DeveloperKnowledge>(),
                pullRequestContext.IsRisky(),
                pullRequestContext.Features,
                pullRequestContext.ComputeDefectPronenessScore(),
                //pullRequestContext.ComputeMaxRevExpertise(pullRequestContext.ActualReviewers),
                pullRequestContext.ComputeSumRevAutExpertise(pullRequestContext.ActualReviewers),
                pullRequestContext.PullRequest.Number,
                0,
                0);
        }

        internal PullRequestKnowledgeDistribution GetBestDistribution(List<PullRequestKnowledgeDistribution> simulationResults)
        {
            var maxScore = simulationResults.Max(q => q.PullRequestKnowledgeDistributionFactors.Score);
            return simulationResults.First(q => q.PullRequestKnowledgeDistributionFactors.Score == maxScore);
        }

        protected DeveloperKnowledge[] GetFolderLevelOweners(int depthToScanForReviewers, PullRequestContext pullRequestContext)
        {
            var pullRequestFiles = pullRequestContext.PullRequestFiles;
            var blameSnapshot = pullRequestContext.KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(pullRequestContext.PullRequestPeriod.Id);

            var relatedFiles = new HashSet<string>();

            foreach (var pullRequestFile in pullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper[pullRequestFile.FileName];
                if (canonicalPath == null)
                {
                    continue;
                }

                var actualPath = blameSnapshot.GetActualPath(canonicalPath);

                if (actualPath == null)
                {
                    continue;
                }

                var neighbors = blameSnapshot.Trie.GetFileNeighbors(depthToScanForReviewers, actualPath);

                if (neighbors != null)
                {
                    foreach (var neighbor in neighbors)
                    {
                        relatedFiles.Add(neighbor);
                    }
                }
            }

            var developersKnowledge = new Dictionary<string, DeveloperKnowledge>();

            foreach (var relatedFile in relatedFiles)
            {
                TimeMachine.AddFileOwnership(pullRequestContext.KnowledgeMap, blameSnapshot, developersKnowledge, relatedFile, pullRequestContext.CanononicalPathMapper);
            }

            var folderLevelKnowlegeables = developersKnowledge.Values.Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q.DeveloperName)).ToArray();

            foreach (var folderLevelKnowlegeable in folderLevelKnowlegeables)
            {
                folderLevelKnowlegeable.IsFolderLevel = true;
            }

            return folderLevelKnowlegeables;
        }
        
        internal abstract IEnumerable<(IEnumerable<DeveloperKnowledge> Reviewers, IEnumerable<DeveloperKnowledge> SelectedCandidateKnowledge)> GetPossibleCandidateSets(PullRequestContext pullRequestContext, DeveloperKnowledge[] availableDevs);

        internal abstract DeveloperKnowledge[] AvailablePRKnowledgeables(PullRequestContext pullRequestContext);

        internal abstract double ComputeScore(PullRequestContext pullRequestContext, PullRequestKnowledgeDistributionFactors pullRequestKnowledgeDistributionFactors);

        internal abstract double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer);
    }
}
