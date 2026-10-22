#!/usr/bin/env Rscript

library(DescTools)

args <- commandArgs(trailingOnly = TRUE)

if (length(args) == 0) {
  stop("No file passed.")
}
csv_path <- args[1]

data <- read.csv(csv_path, header = FALSE)[, 1]

mean <- mean(data)
print(mean, digits = 22)

variance <- var(data)
print(variance, digits = 22)

outliers <- Outlier(data)
print(outliers, digits = 22)

gini <- Gini(data)
print(gini, digits = 22)

hhi <- Herfindahl(data)
print(hhi, digits = 22)
