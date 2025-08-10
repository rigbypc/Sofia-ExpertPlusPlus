# Replication Package

The overall steps are

1. Install Relational Git
2. Get the Database
3. Run the Simulations for each research question
4. Dump the Simulation Data to CSV
5. Calculate the outcome measures: Expertise, Gini-Workload, FaR, and Reviewer++

## Install Relational Git

1) [Install](https://github.com/fahimeh1368/SofiaWL/blob/gh-pages/install.md) the Relational Git and its dependencies.

## Get the Database

1) Restore the data backup into MS SQL Server from [Figshare](https://figshare.com/s/e62f52f5550d0f373815). There is a separate database for each studied project. Note that the databases are approximately 2 GB in size.
2) Copy the configuration files and simulation.ps1, which are provided in the replication package.
3) Open and modify each configuration file to set the connection string. You need to provide the server address along with the credentials. The following snippet shows a sample of how the connection string should be set.

```json
 {
	"ConnectionStrings": {
	  "RelationalGit": "Server=ip_db_server;User Id=user_name;Password=pass_word;Database=coreclr"
	},
	"Mining":{
 		
  	}
 }
```
## Run All Simulations
Open [simulations.ps1](simulations.ps1) using an editor and make sure the corresponding config variables for each research question are defined in the file and refer to the correct location. For instance, each of the following variables should contain the absolute path of the corresponding configuration file for the Roslyn project.


```PowerShell

$roslyn_conf_replace = "Absolute\Path\To\Config\Replace_All\roslyn_conf.json"
$roslyn_conf_AddExpert25 = "Absolute\Path\To\Config\Add_Expert_25\roslyn_conf.json"
$roslyn_conf_AddExpert50 = "Absolute\Path\To\Config\Add_Expert_50\roslyn_conf.json"
$roslyn_conf_AddExpert75 = "Absolute\Path\To\Config\Add_Expert_75\roslyn_conf.json"

```

1) Run the [simulations.ps1](simulations.ps1) script. Open PowerShell and run the following command in the directory of the file

``` PowerShell
./simulations.ps1
```

This script runs all the defined reviewer recommendation algorithms across all projects.

**Note**: Make sure you have set the PowerShell [execution policy](https://superuser.com/questions/106360/how-to-enable-execution-of-powershell-scripts) to **Unrestricted** or **RemoteAssigned**.

## If you want to run the simulations separately for each RQ

If you want to run the simulation separately, the following sections describe the commands needed to run simulations for each research question. For each simulation, a sample is provided that illustrates how to run the simulation using the tool.

**Note:** To run the simulations for each of the following research questions, you need to change the config file of all three projects. To avoid confusion, we recommend creating a separate configuration file for each research question.

### Simulation RQ1 & RQ2:

To replicate the results of RQ1 & RQ2, you should adjust the config files for each project and set the _PullRequestReviewerSelectionStrategy_ as follows:

```
"PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:replacerandom-1",
```

In this way, one of the reviewers on pull requests will be randomly (seeded) replaced with the top-recommended candidate suggested by each recommender. Then, run the following commands for each project to simulate the performance of the recommenders at the replacement level. For example, for the Roslyn project, you should run the following commands:

```PowerShell
# Roslyn Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --simulation-type "Random" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AcHRev --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy WhoDo --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy SofiaWL  --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
```

**Note**: In order to select between ```Random``` and ```SeededRandom```, adjust the ```--simulation-type``` command. If you want to run the seeded version, set the value of ```--simulation-type``` to ```Random``` for **cHRev** and all the other algorithms to ```SeededRandom```. If you wish to run the random version, set the value of ```--simulation-type``` to ```Random``` for all the algorithms.

---

### Simulation RQ3, AddExpertRec:

To run the AddExpertRec strategy, you should apply the following change to the config file of each project.

```
"PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:addDefect_Dt-1",
```

The Dt parameter should be adjusted based on the recommender. In our paper, we run simulations for the defect risk thresholds of Dt = {25, 50, 75}. For example, if you want to run the **AddExpertRec(25)** recommender, you should change the config files as follows. In this way, the recommender will add an extra reviewer for PRs whose defect proneness is at or above 25% and replace one of the reviewers in other PRs.

```
"PullRequestReviewerSelectionStrategy" : "0:nothing-nothing,-:addDefect_25-1",
```

After adjusting the configuration files, you should run the commands for each project as follows: 

```PowerShell
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_roslyn_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_rust_config_file>
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path <path_to_kubernetes_config_file>
```
---

## Dump the Simulation Data to CSV

Log in to the database of each project and run the following command to find the IDs of your simulation.

```SQL
-- Get the Id of the simulation 
SELECT Id,
	KnowledgeShareStrategyType, 
	StartDateTime,
	EndDateTime
	PullRequestReviewerSelectionStrategy,
	SimulationType 
FROM LossSimulations
WHERE EndDateTime > StartDateTime
ORDER BY StartDateTime DESC
```

To obtain your simulation results, run the analyzer using the following command. Substitute the ```<rec_sim_id>``` variable with the Id of your desired recommender, and compare the recommender performance with the actual values, ```<reality_id>```. Note that you can add multiple simulation IDs, separating them with a space. You should also substitute ```<path_to_result>``` and ```<path_to_config_file>``` variables with the path where you want to save the results and the config file of the corresponding RQ and project.

```PowerShell
dotnet-rgit --cmd analyze-simulations --analyze-result-path <path_to_result> --recommender-simulation <rec_sim_id> --reality-simulation <reality_id>  --conf-path <path_to_config_file>
```

### Results for the outcome measures:

After running the analyzer, the tool creates five CSV files: **Expertise.csv**, **FaR.csv**, **Core_Workload.csv**, **CCSR.csv**, and **ReviewerPlusPlus.csv**. The first column shows the project's periods (quarters) in the first four files. Each column corresponds to one of the simulations. Each cell in the first four files displays the percentage change between the actual and simulated outcomes for that period. The last two rows show the *median* and *average* of columns. The **ReviewerPlusPlus.csv** file shows the proportion of pull requests to which a recommender adds an extra reviewer in each period. The last rows of this file present the *Rev++* outcome during the whole lifetime of projects. Note that the **Core_Workload.csv** file includes the number of reviews for the top 10 reviewers in each period. This outcome measure is defined in [prior work](https://dl.acm.org/doi/10.1145/3377811.3380335) that was published in ICSE 2020. To calculate the Gini-Workload of reviewers, follow the instructions in [WorkloadAUC.r](WorkloadMeasures/WorkloadAUC.R).

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
| SofiaWL                   | 314        | 225      | 211            |
| WhoDo                     | 315        | 226      | 212            |
| **RQ3: AddExpertRec(Dt)** |            |          |                |
| AddExpertRec(25)          | 317        | 228      | 214            |
| AddExpertRec(50)          | 318        | 229      | 215            |
| AddExpertRec(75)          | 319        | 230      | 216            |


