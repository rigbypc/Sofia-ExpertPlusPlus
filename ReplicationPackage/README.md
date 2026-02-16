# Replication Package

The overall steps are

1. Install the dependencies
2. Download and restore the database in your local SQL server
3. Run the simulations for each research question
4. Analyze the simulations and get results
5. Calculate the outcome measures: Expertise, Gini-Workload, KRT, CCSR, and Rev++

## Install Relational Git

1) Make sure you [download and install](../README.md) the dependencies.

## Get the Database

1) Restore the data backup into MS SQL Server from [Figshare](https://figshare.com/s/e62f52f5550d0f373815). There is a separate database for each studied project, and you should restore all of them. To restore a database from a ```.bacpac``` file, start by opening SQL Server Management Studio (SSMS) and connecting to your local SQL Server instance. In the Object Explorer, right-click on the Databases node and choose Import Data-tier Application. From there, click Browse to locate the ```.bacpac``` file on your local disk. Once you’ve selected the file, follow the wizard by clicking Next, reviewing the steps, and then clicking Finish to complete the import process. This will restore your database and make it available in your SQL Server instance. Note that the databases are approximately 2 GB in size.
2) Open and modify each configuration file in the [config directory](./config) to set up the connection with the database. You have to provide the server address along with the credentials to your local SQL server. The following snippet shows a sample of how the connection string should be set.

```json

"ConnectionStrings": {
	"RelationalGit": "Server=ip_db_server;User Id=user_name;Password=pass_word;Database=Roslyn_Defect"
},

```

## Run the Simulations

1) Open [simulations.ps1](simulations.ps1) using an editor and update all the paths to the configuration files. For instance, each of the following variables contains the absolute path of the corresponding configuration file for the Roslyn Project.


```PowerShell
$roslyn_conf_replace = "Absolute\Path\To\Config\Replace_All\roslyn_conf.json"
$roslyn_conf_AddExpert25 = "Absolute\Path\To\Config\Add_Expert_25\roslyn_conf.json"
$roslyn_conf_AddExpert50 = "Absolute\Path\To\Config\Add_Expert_50\roslyn_conf.json"
$roslyn_conf_AddExpert75 = "Absolute\Path\To\Config\Add_Expert_75\roslyn_conf.json"
```

2) Open PowerShell and run the [simulations.ps1](simulations.ps1) script.

``` PowerShell
./simulations.ps1
```

This script simulates the performance of all the defined reviewer recommendation algorithms across all projects.

**Note**: if you get any error, make sure you have set the PowerShell [execution policy](https://superuser.com/questions/106360/how-to-enable-execution-of-powershell-scripts) to **Unrestricted** or **RemoteAssigned**.


## If you want to run the simulations separately for each RQ:

The following sections describe the commands needed to run simulations for each research question. For each simulation, a sample is provided that illustrates how to run the simulation using the tool. To run the simulations for each of the following research questions, you need to open the [source code](../src/RelationalGit.sln) as a project in your IDE like [Microsoft Visual Studio](https://visualstudio.microsoft.com/downloads/) and run the corresponding commands for each RQ (Debug → RelationalGit Debug Properties → Create a new profile → Project → insert the commands in ```Command line arguments``` box). Since you need to change the configuration files for each research question, we recommend using configuration files in the [config directory](./config) to avoid confusion. 

### RQ1 & RQ2: CCSR vs. other outcome measures

To replicate the results of ```RQ1 & RQ2```, you should set the ```PullRequestReviewerSelectionStrategy``` to  ```replacerandom```. In this way, one of the reviewers on each pull request will be randomly (seeded) replaced with the top-recommended candidate suggested by each recommender. 

```
"PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:replacerandom-1",
```

Then, run the following commands for each project to simulate the performance of the recommenders at the replacement level. For example, for the Roslyn project, you should run the following commands:

```PowerShell
# Roslyn Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --simulation-type "Random" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AcHRev --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy WhoDo --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy SofiaWL  --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path <path_to_roslyn_config_file>
```

**Note**: In order to select between ```Random``` and ```SeededRandom``` replacement, adjust the ```--simulation-type``` command. In the commands above, first, the simulator will randomly replace one of the actual reviewers with the top candidate from cHRev. Then, it will use these seeded indices in the following simulation to have a fair comparison between recommenders, as all of them replace the same actual reviewer.

---

### RQ3: AddExpertRec(Dt)

To run the AddExpertRec strategy, we define the ```addDefect_Dt``` value for the ```PullRequestReviewerSelectionStrategy``` argument.

```
"PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:addDefect_Dt-1",
```

The ```Dt``` parameter should be adjusted based on the recommendation strategy. In this paper, we run simulations for the defect risk thresholds of `Dt = {25, 50, 75}`. For example, if you want to run the **AddExpertRec(25)** recommender, you should set the ```PullRequestReviewerSelectionStrategy``` argument as follows. In this way, the recommender will add an extra reviewer for PRs whose defect proneness is at or above 25% and replace one of the actual reviewers with the top recommended candidate in other PRs.

```
"PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:addDefect_25-1",
```

After adjusting the configuration files, you can simulate the performance of this strategy for each project as follows: 

```PowerShell
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_rust_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_kubernetes_config_file>
```
---

## Analyze Simulations

To get your simulation IDs, connect to your local SQL Server instance and run the following query for each project. This query is a sample for the Rolsyn project.

```SQL
-- Get the Id of the simulation 
SELECT  Id,
	KnowledgeShareStrategyType, 
	StartDateTime,
	EndDateTime
	PullRequestReviewerSelectionStrategy,
	SimulationType 
FROM [Roslyn_Defect].[dbo].[LossSimulations]
WHERE EndDateTime > StartDateTime
ORDER BY StartDateTime DESC
```

To get your simulation results, you can create a new profile in RelationalGit Debug Properties and run the analyzer using the following command. Substitute the ```<rec_sim_id>``` variable with the Id of your desired recommender, and compare the recommender performance with the actual values, ```<reality_id>```. Note that you can add multiple simulation IDs, separating them with a space. You should also substitute ```<path_to_result>``` and ```<path_to_config_file>``` variables with the path where you want to save the results and the config file of the corresponding project.

```PowerShell
dotnet-rgit --cmd analyze-simulations --analyze-result-path <path_to_result> --recommender-simulation <rec_sim_id> --reality-simulation <reality_id>  --conf-path <path_to_config_file>
```

### Results for the outcome measures:

After running the analyzer, the tool creates five CSV files: **Expertise.csv**, **KRT.csv**, **Core_Workload.csv**, **CCSR.csv**, and **ReviewerPlusPlus.csv**. The first column shows the project's periods (quarters) in the first four files. Each column corresponds to one of the simulations. Each cell in the first four files displays the percentage change between the actual and simulated outcomes for that period. The last two rows show the *median* and *average* of columns. The **ReviewerPlusPlus.csv** file shows the proportion of pull requests to which a recommender adds an extra reviewer in each period. The last rows of this file present the *Rev++* outcome during the whole lifetime of projects. Note that the **Core_Workload.csv** file includes the percentage change in the number of reviews for the top 10 reviewers in each period. This outcome measure is defined in [prior work](https://dl.acm.org/doi/10.1145/3377811.3380335) that was published in ICSE 2020. To calculate the Gini-Workload of reviewers, follow the instructions in [WorkloadAUC.r](WorkloadMeasures/README.md).

### Our Simulation IDs:

As some of the simulations can take hours to run, the following table includes the simulation IDs for our experiments. 

| **Recommender**           | **Roslyn** | **Rust** | **Kubernetes** |
|:-------------------------:|:----------:|:--------:|:--------------:|
| Reality                   | 324        | 235      | 221            |
| **RQ1 & RQ2**             |            |          |                |
| cHRev                     | 306        | 217      | 203            |
| AcHRev                    | 307        | 218      | 204            |
| TurnoverRec               | 312        | 223      | 209            |
| Sofia                     | 313        | 224      | 210            |
| RAR                       | 316        | 227      | 213            |
| WhoDo                     | 315        | 226      | 212            |
| SofiaWL                   | 314        | 225      | 211            |
| **RQ3: AddExpertRec(Dt)** |            |          |                |
| AddExpertRec(25)          | 317        | 228      | 214            |
| AddExpertRec(50)          | 318        | 229      | 215            |
| AddExpertRec(75)          | 319        | 230      | 216            |

---
## More Options:

#### Other Reviewer Recommenders:
In addition to the studied recommenders, you can also execute simulations for the `AuthorshipRec`, `RevOwnRec`, `LearnRec`, and `RetentionRec` strategies introduced in prior studies. To simulate any of these recommenders, simply specify their names in the simulation command. For example, to evaluate the performance of the `RetentionRec` recommender on the Roslyn project, use the following command:

```PowerShell
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
```

#### Other Aggregation Functions:
In addition to the linear summation of reviewers and author expertise, we implement other aggregation functions in this replication package for the **changeset safety ratio**. 

```csharp
// CSR outcome: MaxReviewers
public double ComputeMaxRevExpertise(DeveloperKnowledge[] selectedReviewers)

// SumReviewers
public double ComputeSumRevExpertise(DeveloperKnowledge[] selectedReviewers)

// MaxReviewers + Author
public double ComputeMaxRevPlusAutExpertise(DeveloperKnowledge[] selectedReviewers)

// CCSR outcome:  SumReviewers + Author
public double ComputeSumRevAutExpertise(DeveloperKnowledge[] selectedReviewers)

// P75([reviewers, author])
public double Compute75PercentileRevAutExpertise(DeveloperKnowledge[] selectedReviewers)

// mean([reviewers, author])
public double ComputeMeanRevAutExpertise(DeveloperKnowledge[] selectedReviewers)

// median([reviewers, author])
public double ComputeMedianRevAutExpertise(DeveloperKnowledge[] selectedReviewers)

// max([reviewers, author])
public double ComputeMaxRevAutExpertise(DeveloperKnowledge[] selectedReviewers)

```

To run simulations with these aggregation functions, you should update the **`PullRequestRecommendationResult()`** function in [SpreadingKnowledgeShareStrategyBase.cs](../src/RelationalGit.Recommendation/Strategies/Spreading/SpreadingKnowledgeShareStrategyBase.cs#L235) (lines 235 & 256) and [RealityRecommendationStrategy.cs](../src/RelationalGit.Recommendation/Strategies/General/RealityRecommendationStrategy.cs#L23) (line 23) files.
