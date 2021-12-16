library(DescTools)
library(ggplot2)
library(dplyr)

scores <- read.csv("../results/point_scores.csv", header=TRUE)
scores.mat <- data.matrix(scores)
n <- nrow(scores.mat)
cormat <- ((n - 1) / n) * cov(scores.mat)  # covariance 

#Pearsons
p <- cormat[1,2] / sqrt( cormat[1,1] * cormat[2,2] ) 

#Spearmans
rho <- SpearmanRho(scores$actual, scores$predicted)

# create duplicate values index
scores.freq <- data.frame(
  actual=scores$actual,
  predicted=scores$predicted,
  freq=vector(mode="integer", length=nrow(scores)))

for (i in 1:nrow(scores)) {
  for (j in 1:nrow(scores)) {
    if (scores[i,1] == scores[j,1] & scores[i,2] == scores[j,2]) {
      scores.freq[i,3] = scores.freq[i,3] + 1
    }
  }
}

scores.freq.unique <- distinct(scores.freq)