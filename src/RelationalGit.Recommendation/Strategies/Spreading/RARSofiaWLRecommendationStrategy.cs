using Microsoft.Extensions.Logging;
using RelationalGit.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit.Recommendation
{
    public class RARSofiaWLRecommendationStrategy : ScoreBasedRecommendationStrategy
    {
        private int? _numberOfPeriodsForCalculatingProbabilityOfStay;
        private double _alpha;
        private double _beta;
        private double pd;
        private int _riskOwenershipThreshold;
        private double _hoarderRatio;
        string _pullRequestReviewerSelectionStrategy;


        public RARSofiaWLRecommendationStrategy(string knowledgeSaveReviewerReplacementType, 
            ILogger logger, int? numberOfPeriodsForCalculatingProbabilityOfStay, 
            string pullRequestReviewerSelectionStrategy,
            bool? addOnlyToUnsafePullrequests,
            string recommenderOption, bool changePast, string simulationType)
            : base(knowledgeSaveReviewerReplacementType, logger, pullRequestReviewerSelectionStrategy, addOnlyToUnsafePullrequests, recommenderOption, changePast, simulationType)
        {
            _numberOfPeriodsForCalculatingProbabilityOfStay = numberOfPeriodsForCalculatingProbabilityOfStay;
            var parameters = GetParameters(recommenderOption);
            _alpha = parameters.Alpha;
            _beta = parameters.Beta;
            _riskOwenershipThreshold = parameters.RiskOwenershipThreshold;
            _hoarderRatio = parameters.HoarderRatio;
            _pullRequestReviewerSelectionStrategy = pullRequestReviewerSelectionStrategy;
        }

        private (double Alpha,double Beta,int RiskOwenershipThreshold,double HoarderRatio) GetParameters(string recommenderOption)
        {
            if (string.IsNullOrEmpty(recommenderOption))
                return (0.5, 1,3,0.7);

            var options = recommenderOption.Split(',');
            var alphaOption = options.FirstOrDefault(q => q.StartsWith("alpha")).Substring("alpha".Length+1);
            var betaOption = options.FirstOrDefault(q => q.StartsWith("beta")).Substring("beta".Length + 1);
            var riskOwenershipThreshold = options.FirstOrDefault(q => q.StartsWith("risk")).Substring("risk".Length + 1);
            var hoarderRatioOption = options.FirstOrDefault(q => q.StartsWith("hoarder_ratio")).Substring("hoarder_ratio".Length + 1);

            return (double.Parse(alphaOption), double.Parse(betaOption),int.Parse(riskOwenershipThreshold),double.Parse(hoarderRatioOption));
        }

        internal override double ComputeReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            

            var defectPronenessScore = pullRequestContext.ComputeDefectPronenessScore();

            //  static method
            // double low = 0;
            // double high = 1;
            // double threshold = 0.1; //RQ2: static value for P_D

            // dynamic method
            double low = pullRequestContext.Periods[pullRequestContext.PullRequestPeriod.Id].dynLow;
            double high = pullRequestContext.Periods[pullRequestContext.PullRequestPeriod.Id].dynHigh;
            
            //  normal method
            // double low = pullRequestContext.Periods[pullRequestContext.PullRequestPeriod.Id].normLow;
            // double high = pullRequestContext.Periods[pullRequestContext.PullRequestPeriod.Id].normHigh;
            
            if (_pullRequestReviewerSelectionStrategy.StartsWith("addDefect"))
            {
                pd = double.Parse(_pullRequestReviewerSelectionStrategy.Split('_')[1]) / 100.0;
            }

            else
            {
                pd = 0.5;
            } 
            
            double threshold = low + pd * (high - low); // RQ3: dynamic value for P_D

            // defectPronenessScore constant
            
            if (defectPronenessScore > threshold)
            {
                double expertiseScore = ComputeBirdReviewerScore(pullRequestContext, reviewer);
                return expertiseScore;
            }

            else if (defectPronenessScore <= threshold && pullRequestContext.GetRiskyFiles(_riskOwenershipThreshold).Length > 0)
            {
                double spreadingScore = GetPersistSpreadingScore(pullRequestContext, reviewer, _pullRequestReviewerSelectionStrategy);
                return spreadingScore;
            } 

            else 
            {
                double loadscore = GetLoadScore(pullRequestContext, reviewer);
                var load = Math.Pow(Math.E, (0.5 * loadscore));
                var WhoDoExpertiseScore = ComputeWhoDoReviewerScore(pullRequestContext, reviewer);
                return WhoDoExpertiseScore / load;
            }
            
        }

        private double GetPersistSpreadingScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer, string pullRequestReviewerSelectionStrategy)
        {
            var reviewerImportance = pullRequestContext.IsHoarder(reviewer.DeveloperName, pullRequestReviewerSelectionStrategy) ? _hoarderRatio : 1;

            double probabilityOfStay = pullRequestContext.GetProbabilityOfStay(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);
            var effort = pullRequestContext.GetEffort(reviewer.DeveloperName, _numberOfPeriodsForCalculatingProbabilityOfStay.Value);

            var prFiles = pullRequestContext.PullRequestFiles.Select(q => pullRequestContext.CanononicalPathMapper[q.FileName])
                    .Where(q => q != null).ToArray();
            var reviewedFiles = reviewer.GetTouchedFiles().Where(q => prFiles.Contains(q));
            var specializedKnowledge = reviewedFiles.Count() / (double)pullRequestContext.PullRequestFiles.Length;

            var spreadingScore = 0.0;

            spreadingScore = reviewerImportance * Math.Pow(probabilityOfStay * effort, _alpha) * Math.Pow(1 - specializedKnowledge, _beta);

            return spreadingScore;
        }

        private double ComputeBirdReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var score = 0.0;

            foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);
                if (canonicalPath == null)
                {
                    continue;
                }

                var fileExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetFileExpertise(canonicalPath);

                if (fileExpertise.TotalComments == 0)
                {
                    continue;
                }

                var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(canonicalPath, reviewer.DeveloperName);

                if (reviewerExpertise == (0, 0, null))
                {
                    continue;
                }

                var scoreTotalComments = reviewerExpertise.TotalComments / (double)fileExpertise.TotalComments;
                var scoreTotalWorkDays = reviewerExpertise.TotalWorkDays / (double)fileExpertise.TotalWorkDays;
                var scoreRecency = (Math.Abs((reviewerExpertise.RecentWorkDay - fileExpertise.RecentWorkDay).Value.TotalDays) + 1);

                score += scoreTotalComments + scoreTotalWorkDays + scoreRecency;
            }

            return score / (3 * pullRequestContext.PullRequestFiles.Length);
        }

        private double ComputeWhoDoReviewerScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            double? commit_score = 0.0;
            double? review_score = 0.0;
            double? neighber_commit_score = 0.0;
            double? neighber_review_score = 0.0;

            foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
            {
                var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);

                if (canonicalPath == null)
                {
                    continue;
                }

                commit_score += ComputeCommitScore(pullRequestContext, canonicalPath, reviewer);

                review_score += ComputeReviewScore(pullRequestContext, canonicalPath, reviewer);
                var blameSnapshot = pullRequestContext.KnowledgeMap.BlameBasedKnowledgeMap.GetSnapshopOfPeriod(pullRequestContext.PullRequestPeriod.Id);
                var neighbors = blameSnapshot.Trie.GetFileNeighbors(1, canonicalPath);
                for (int j = 0; j < neighbors.Length; j++)
                {
                    var neighber_file = neighbors[j];
                    neighber_commit_score += ComputeCommitScore(pullRequestContext, neighber_file, reviewer);
                    neighber_review_score += ComputeReviewScore(pullRequestContext, neighber_file, reviewer);
                }
            }



            double final_score = Convert.ToDouble(review_score + commit_score + neighber_commit_score + neighber_review_score);
            return final_score;
        }
        
        private long GetLoadScore(PullRequestContext pullRequestContext, DeveloperKnowledge reviewer)
        {
            var reviwes = new List<long>();
            var overitems = pullRequestContext.Overlap;
            if (overitems.Count == 0)
                return 0;

            foreach (var pullreq in pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap.GetDeveloperReviews(reviewer.DeveloperName))
            {
                reviwes.Add(pullreq.Number);
            }
            var count = overitems.Intersect(reviwes);
            return count.Count();
        }

        private double ComputeCommitScore(PullRequestContext pullRequestContext, string filePath, DeveloperKnowledge reviewer)
        {
            double score = 0.0;

            DateTime reviewer_recency = DateTime.MinValue;
            DateTime nowTime = pullRequestContext.PullRequest.CreatedAtDateTime ?? DateTime.Now;
            var reviewerCommits = pullRequestContext.KnowledgeMap.CommitBasedKnowledgeMap.GetDeveloperCommitsOnFile(reviewer.DeveloperName, filePath, nowTime, out reviewer_recency);
            if (reviewerCommits == 0)
            {
                return score;
            }


            if (reviewer_recency != DateTime.MinValue)
            {
                var diff_review = (nowTime - reviewer_recency).TotalDays == 0 ? 1 : (nowTime - reviewer_recency).TotalDays;

                score = reviewerCommits / diff_review;
            }
            return score;
        }
        private double ComputeReviewScore(PullRequestContext pullRequestContext, string filePath, DeveloperKnowledge reviewer)
        {
            double score = 0.0;
            DateTime nowTime = pullRequestContext.PullRequest.CreatedAtDateTime ?? DateTime.Now;
            var data = pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap.GetReviowersOnFile(reviewer.DeveloperName, filePath);

            var reviewNumber = data.Keys.FirstOrDefault();

            var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(filePath, reviewer.DeveloperName);

            if (reviewNumber + reviewerExpertise.TotalComments == 0)
            {
                return score;
            }

            var recency = data.Values.FirstOrDefault();
            if (recency < reviewerExpertise.RecentWorkDay)
            {
                recency = reviewerExpertise.RecentWorkDay ?? recency;
            }
            if (recency != DateTime.MinValue)
            {
                var diff_review = (nowTime - recency).TotalDays == 0 ? 1 : (nowTime - recency).TotalDays;
                score = reviewNumber + reviewerExpertise.TotalComments / diff_review;
            }
            return score;
        }
    }
}
