#!/usr/bin/env Rscript

library(DescTools)
library(jsonlite)

# name -> tau, R-7, mirroring CommonQuantile.  Emitted in this order.
k_quantiles <- c(
  SevenLowOuter  = 0.0215,
  Percentile5th  = 0.05,
  SevenLowInner  = 0.0887,
  Percentile10th = 0.10,
  Octile1st      = 0.125,
  Quartile1st    = 0.25,
  Median         = 0.5,
  Quartile3rd    = 0.75,
  Octile7th      = 0.875,
  Percentile90th = 0.90,
  SevenHighInner = 0.9113,
  Percentile95th = 0.95,
  SevenHighOuter = 0.9785
)

# Weighted samples used only as inputs by the count-oriented suites; no statistics recorded.
k_input_only <- c("heavy-tail-count-10k")

script_dir <- function() {
  a <- commandArgs(trailingOnly = FALSE)
  f <- grep("^--file=", a, value = TRUE)
  if (length(f)) dirname(normalizePath(sub("^--file=", "", f[1]))) else getwd()
}

# Trusted expected statistics for a unit-weight sample.  Returns a named list;
compute <- function(x) {
  qs <- quantile(x, k_quantiles, type = 7, names = FALSE)

  out <- list(
    count = length(x),
    count0 = sum(x == 0),
    count_negative = sum(x < 0),
    count_positive = sum(x > 0),
    min_value = min(x),
    min_count = sum(x == min(x)),
    max_value = max(x),
    max_count = sum(x == max(x)),
    mean = DescTools::Mean(x),
    variance = DescTools::Var(x),
    skewness = DescTools::Skew(x, method = 2), # G1
    kurtosis = DescTools::Kurt(x, method = 2), # G2

    geometric_mean = DescTools::Gmean(x[x > 0]),
    harmonic_mean = DescTools::Hmean(x[x > 0]),
    mean_absolute_deviation = DescTools::MeanAD(x),
    median_absolute_deviation = DescTools::MeanAD(x, center = median(x)),
    entropy = DescTools::Entropy(as.numeric(table(x))),
    gini_coefficient = DescTools::Gini(x),
    herfindahl_hirschman_index = DescTools::Herfindahl(x),
    outlier_count = length(DescTools::Outlier(x)),
    quantiles = setNames(as.list(qs), names(k_quantiles))
  )

  # Filter undefined, skipping defining it in the json
  undefined <- vapply(out, function(v) length(v) == 1 && is.na(v), logical(1))
  out[!undefined]
}

main <- function() {
  root <- script_dir()
  files <- sort(list.files(root, pattern = "\\.csv$", full.names = TRUE))

  for (path in files) {
    id <- sub("\\.csv$", "", basename(path))
    df <- read.csv(path, header = FALSE)

    values <- df[[1]]
    weighted <- ncol(df) >= 2
    counts <- if (weighted) df[[2]] else numeric(0)

    obj <- list(identifier = id, values = values, counts = counts)

    if (!(id %in% k_input_only)) {
      if (weighted) {
        stop(sprintf("fixture '%s' has a count column but is not in k_input_only", id))
      }
      # Flattened: ComptimeJson maps top-level JSON keys directly onto SampleData's properties.
      obj <- c(obj, compute(values))
    }

    dest <- file.path(script_dir(), paste0(id, ".json"))
    writeLines(toJSON(obj, auto_unbox = TRUE, digits = 22, na = "null", pretty = TRUE), dest)
    cat(sprintf("wrote %s\n", dest))
  }
}

main()
