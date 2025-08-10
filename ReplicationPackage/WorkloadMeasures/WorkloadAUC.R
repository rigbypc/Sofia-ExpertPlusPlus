source("relative/path/to/CumulativePercentageLib.r")

# Reality
OpenReviewsReality <- read.csv('relative/path/to/OpenReviewsRealityReplacePerQuarter.csv', header = T)

# RQ1 & RQ2
OpenReviewsCHRev <- read.csv('relative/path/to/OpenReviewsCHRevReplacePerQuarter.csv', header = T)
OpenReviewsAcHRev <- read.csv('relative/path/to/OpenReviewsAcHRevReplacePerQuarter.csv', header = T)
OpenReviewsTurnoverRec <- read.csv('relative/path/to/OpenReviewsTurnoverRecReplacePerQuarter.csv', header = T)
OpenReviewsSofia <- read.csv('relative/path/to/OpenReviewsSofiaReplacePerQuarter.csv', header = T)
OpenReviewsRAR <- read.csv('relative/path/to/OpenReviewsRARReplacePerQuarter.csv', header = T)
OpenReviewsWhoDo <- read.csv('relative/path/to/OpenReviewsWhoDoReplacePerQuarter.csv', header = T)
OpenReviewsSofiaWL <- read.csv('relative/path/to/OpenReviewsSofiaWLReplacePerQuarter.csv', header = T)

# RQ3: AddExpertRec(Dt)
OpenReviewsAddExpertRec25 <- read.csv('relative/path/to/OpenReviewsAddExpertRec25PerQuarter.csv', header = T)
OpenReviewsAddExpertRec50 <- read.csv('relative/path/to/OpenReviewsAddExpertRec50PerQuarter.csv', header = T)
OpenReviewsAddExpertRec75 <- read.csv('relative/path/to/OpenReviewsAddExpertRec75PerQuarter.csv', header = T)


cat("\n")
cat("\n")
print('-------- GiniWorkload of Development Team --------')
cat("\n")
cat("\n")

print(' -------- Replication --------')
cat("\n")

print('cHRev:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsCHRev)
cat("\n")
print('AcHRev:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsAcHRev)
cat("\n")
print('TurnoverRec:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsTurnoverRec)
cat("\n")
print('Sofia:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsSofia)
cat("\n")
print('RAR:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsRAR)
cat("\n")
print('WhoDo:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsWhoDo)
cat("\n")
print('SofiaWL:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsSofiaWL)
cat("\n")

print(' -------- AddExpertRec(Dt) --------')
cat("\n")

print('AddExpertRec25:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsAddExpertRec25)
cat("\n")
print('AddExpertRec50:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsAddExpertRec50)
cat("\n")
print('AddExpertRec75:')
auc_difference_percentage_all(OpenReviewsReality, OpenReviewsAddExpertRec75)

cat("\n")
cat("\n")
p <- 20
print(sprintf(' -------------------------- %d%% of reviewers do what percentage of code reviews? -------------------------- ', p))
cat("\n")
cat("\n")


print(' -------- Reality --------')
cat("\n")
cumulative_at_p_all(OpenReviewsReality, p)
cat("\n")
print(' -------- Baseline --------')
cat("\n")
print('cHRev:')
cumulative_at_p_all(OpenReviewsCHRev, p)
cat("\n")
print('AcHRev:')
cumulative_at_p_all(OpenReviewsAcHRev, p)
cat("\n")
print('TurnoverRec:')
cumulative_at_p_all(OpenReviewsTurnoverRec, p)
cat("\n")
print('Sofia:')
cumulative_at_p_all(OpenReviewsSofia, p)
cat("\n")
print('RAR:')
cumulative_at_p_all(OpenReviewsRAR, p)
cat("\n")
print('WhoDo:')
cumulative_at_p_all(OpenReviewsWhoDo, p)
cat("\n")
print('SofiaWL:')
cumulative_at_p_all(OpenReviewsSofiaWL, p)
cat("\n")

print(' -------- AddExpertRec(Dt) --------')
cat("\n")

print('AddExpertRec25:')
cumulative_at_p_all(OpenReviewsAddExpertRec25, p)
cat("\n")
print('AddExpertRec50:')
cumulative_at_p_all(OpenReviewsAddExpertRec50, p)
cat("\n")
print('AddExpertRec75:')
cumulative_at_p_all(OpenReviewsAddExpertRec75, p)
cat("\n")
print('Finished.')


# # You can plot the Lorenz curve to show area differences in the background methodology

# plot(seq(1:100), seq(1:100), type = 'l', lwd = 3, lty = "solid", ylab = "Cumulative Percentage of Reviews", xlab = "Percentage of Reviewers")
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsReality$Kubernetes), col='red', lty = "solid")
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsAcHRev$Kubernetes), lty = "dotted", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsCHRev$Kubernetes), lty = "dotdash", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsTurnoverRec$Kubernetes), lty = "dashed", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsSofia$Kubernetes), lty = "twodash", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsRAR$Kubernetes), lty = "dashed", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsSofiaWL$Kubernetes), lty = "dashed", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsWhoDo$Kubernetes), lty = "longdash", pch = 2)
# abline(v = 20)
# legend(
#   "bottomright", # Places the legend in the bottom-right corner
#   legend = c(
#     "Actual-Workload",
#     "AcHRev",
#     "cHRev",
#     "TurnoverRec",
#     "Sofia",
#     "RAR",
#     "SofiaWL",
#     "Whodo",
#     "x = y"
#   ),
#   lty = c('solid', 'dotted', 'dotdash', 'twodash', 'dashed', 'dashed', 'dashed', 'longdash', 'solid'),
#   col = c('red', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black'),
#   lwd = 3,
#   cex = 1.2
# )#dev.off()

# # # discussion plot (replace Rust with any other project to see results for different projects)
# # #pdf("~/Downloads/WorkloadAUCDiscussion.pdf")
# plot(seq(1:100), seq(1:100), type = 'l', lwd = 3, lty = "solid",  ylab = "Cumulative Percentage of Reviews", xlab = "Percentage of Reviewers")
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsReality$Rust), col='red', lty = "solid")
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsSofia$Rust), lty = "dotted", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsRAR$Rust), lty = "dotted", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsAcHRev$Rust), lty = "dotdash", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsAddExpertRec25$Rust), lty = "longdash", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsAddExpertRec50$Rust), lty = "longdash", pch = 2)
# plot_line_percentage_percentage(percentage_percentage_freq(OpenReviewsAddExpertRec75$Rust), lty = "longdash", pch = 2)
# abline(v = 1.5)
# abline(v = 10)
# abline(v = 20)
# abline(h = 80)
# abline(h = 60)
# abline(h = 20)
# legend(
#   "bottomright", # Places the legend in the bottom-right corner
#   legend = c(
#     "Actual-Workload", "Sofia", "RAR", "AcHRev",
#     "AddExpertRec25", "AddExpertRec25", "AddExpertRec75", "x = y"
#   ),
#   lty = c('solid', 'dotted', 'dashed', 'dotdash', 'longdash', 'longdash', 'longdash', 'solid'),
#   col = c('red', 'black', 'black', 'black', 'black', 'black', 'black', 'black'),
#   lwd = 3,
#   cex = 1.2
# )
