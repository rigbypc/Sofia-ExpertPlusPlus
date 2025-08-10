# Each of the following variables contains the path to the corresponding configuration files.

$roslyn_conf_replace = "Absolute\Path\To\Config\Replace_All\roslyn_conf.json"
$roslyn_conf_AddExpert25 = "Absolute\Path\To\Config\Add_Expert_25\roslyn_conf.json"
$roslyn_conf_AddExpert50 = "Absolute\Path\To\Config\Add_Expert_50\roslyn_conf.json"
$roslyn_conf_AddExpert75 = "Absolute\Path\To\Config\Add_Expert_75\roslyn_conf.json"


$rust_conf_replace = "Absolute\Path\To\Config\Replace_All\rust_conf.json"
$rust_conf_AddExpert25 = "Absolute\Path\To\Config\Add_Expert_25\rust_conf.json"
$rust_conf_AddExpert50 = "Absolute\Path\To\Config\Add_Expert_50\rust_conf.json"
$rust_conf_AddExpert75 = "Absolute\Path\To\Config\Add_Expert_75\rust_conf.json"

$kubernetes_conf_replace = "Absolute\Path\To\Config\Replace_All\kubernetes_conf.json"
$kubernetes_conf_AddExpert25 = "Absolute\Path\To\Config\Add_Expert_25\kubernetes_conf.json"
$kubernetes_conf_AddExpert50 = "Absolute\Path\To\Config\Add_Expert_50\kubernetes_conf.json"
$kubernetes_conf_AddExpert75 = "Absolute\Path\To\Config\Add_Expert_75\kubernetes_conf.json"

# Roslyn Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --simulation-type "Random" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AcHRev --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy WhoDo --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy SofiaWL  --simulation-type "SeededRandom" --conf-path $roslyn_conf_replace
# AddExpert Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $roslyn_conf_AddExpert25
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $roslyn_conf_AddExpert50
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $roslyn_conf_AddExpert75


# Rust Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --simulation-type "Random" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --simulation-type "SeededRandom" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AcHRev --simulation-type "SeededRandom" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --simulation-type "SeededRandom" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --simulation-type "SeededRandom" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $rust_conf_AddExpert25
dotnet-rgit --cmd simulate-recommender --recommendation-strategy WhoDo --simulation-type "SeededRandom" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy SofiaWL  --simulation-type "SeededRandom" --conf-path $rust_conf_replace
# AddExpert Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $rust_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $rust_conf_AddExpert50
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $rust_conf_AddExpert75


# Kubernetes Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --simulation-type "Random" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AcHRev --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy WhoDo --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
dotnet-rgit --cmd simulate-recommender --recommendation-strategy SofiaWL  --simulation-type "SeededRandom" --conf-path $kubernetes_conf_replace
# AddExpert Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $kubernetes_conf_AddExpert25
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $kubernetes_conf_AddExpert50
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RAR --simulation-type "SeededRandom" --conf-path $kubernetes_conf_AddExpert75


## Run analyzer to calculate the outcome measures
## You should first run the previous commands and restore the Simulation-Ids from the database. Then, update the Simulation IDs and Reality ID for each project and run the following commands.
# dotnet-rgit --cmd analyze-simulations --analyze-result-path "Absolute\Path\To\Roslyn_Results_Directory" --recommender-simulation <Roslyn-Simulation-Ids-SeparatedWithSpace> --reality-simulation <Reality-Id> --conf-path $roslyn_conf_replace
# dotnet-rgit --cmd analyze-simulations --analyze-result-path "Absolute\Path\To\Rust_Results_Directory" --recommender-simulation <Rust-Simulation-Ids-SeparatedWithSpace> --reality-simulation <Reality-Id> --conf-path $rust_conf_replace
# dotnet-rgit --cmd analyze-simulations --analyze-result-path "Absolute\Path\To\Kubernetes_Results_Directory" --recommender-simulation <Kubernetes-Simulation-Ids-SeparatedWithSpace> --reality-simulation <Reality-Id> --conf-path $kubernetes_conf_replace
