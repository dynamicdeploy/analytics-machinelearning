#' Anomaly Detection Using Seasonal Hybrid ESD Test
#'
#' A technique for detecting anomalies in seasonal univariate time series where the input is a
#' series of <timestamp, count> pairs.
#' @name AnomalyDetectionTsw
#' @param dataset1 Time series as a two column data frame where the first column consists of the
#' timestamps and the second column consists of the observations.
#' @param max_anoms Maximum number of anomalies that S-H-ESD will detect as a percentage of the
#' data.
#' @param direction Directionality of the anomalies to be detected. Options are:
#' \code{'pos' | 'neg' | 'both'}.
#' @param alpha The level of statistical significance with which to accept or reject anomalies.
#' @param only_last Find and report anomalies only within the last day or hr in the time series.
#' \code{NULL | 'day' | 'hr'}.
#' @param threshold Only report positive going anoms above the threshold specified. Options are:
#' \code{'None' | 'med_max' | 'p95' | 'p99'}.
#' @param e_value Add an additional column to the anoms output containing the expected value.
#' @param longterm Increase anom detection efficacy for time series that are greater than a month.
#' See Details below.
#' @param piecewise_median_period_weeks The piecewise median time window as described in Vallis, Hochenbaum, and Kejariwal (2014).
#' Defaults to 2.
#' @param plot A flag indicating if a plot with both the time series and the estimated anoms,
#' indicated by circles, should also be returned.
#' @param y_log Apply log scaling to the y-axis. This helps with viewing plots that have extremely
#' large positive anomalies relative to the rest of the data.
#' @param xlabel X-axis label to be added to the output plot.
#' @param ylabel Y-axis label to be added to the output plot.
#' @details
#' \code{longterm} This option should be set when the input time series is longer than a month.
#' The option enables the approach described in Vallis, Hochenbaum, and Kejariwal (2014).\cr\cr
#' \code{threshold} Filter all negative anomalies and those anomalies whose magnitude is smaller
#' than one of the specified thresholds which include: the median
#' of the daily max values (med_max), the 95th percentile of the daily max values (p95), and the
#' 99th percentile of the daily max values (p99).
#' @param title Title for the output plot.
#' @param verbose Enable debug messages.
#' @param narm Remove any NAs in timestamps.(default: FALSE) 
#' @return The returned value is a list with the following components.
#' @return \item{anoms}{Data frame containing timestamps, values, and optionally expected values.}
#' @return \item{plot}{A graphical object if plotting was requested by the user. The plot contains
#' the estimated anomalies annotated on the input time series.}
#' @return One can save \code{anoms} to a file in the following fashion:
#' \code{write.csv(<return list name>[["anoms"]], file=<filename>)}
#' @return One can save \code{plot} to a file in the following fashion:
#' \code{ggsave(<filename>, plot=<return list name>[["plot"]])}
#' @references Vallis, O., Hochenbaum, J. and Kejariwal, A., (2014) "A Novel Technique for
#' Long-Term Anomaly Detection in the Cloud", 6th USENIX, Philadelphia, PA.
#' @references Rosner, B., (May 1983), "Percentage Points for a Generalized ESD Many-Outlier Procedure"
#' , Technometrics, 25(2), pp. 165-172.
#'
#' @docType data
#' @keywords datasets
#' @name raw_data
#'
#' @examples
#' data(raw_data)
#' AnomalyDetectionTsw(raw_data, max_anoms=0.02, direction='both', plot=TRUE)
#' # To detect only the anomalies on the last day, run the following:
#' AnomalyDetectionTsw(raw_data, max_anoms=0.02, direction='both', only_last="day", plot=TRUE)
#' @seealso \code{\link{AnomalyDetectionVec}}
#' @export
#'
AnomalyDetectionTsw <- function(dataset1, max_anoms = 0.10, direction = 'pos',
                               alpha = 0.05, only_last = NULL, threshold = 'None',
                               e_value = FALSE, longterm = FALSE, piecewise_median_period_weeks = 2, plot = FALSE,
                               y_log = FALSE, xlabel = '', ylabel = 'count',
                               title = NULL, verbose=FALSE, narm = FALSE){

#Added by Tejaswi. Wrapper to work with AzureML
# Contents of optional Zip port are in ./src/
source("src/date_utils.R");
source("src/detect_anoms.R");
source("src/plot_utils.R");
source("src/vec_anom_detection.R");
source("src/ts_anom_detection.R");

if(only_last == "None"){ only_last <- NULL}
if(xlabel == "None") { xlabel <- ''}
 
 
	if(verbose)
 {
    print("Dataset Input")
	str(dataset1)
	
	}
	#dataset1$timestamp <- as.POSIXlt(dataset1$timestamp)
	
	dataset1[[1]] <- as.POSIXlt(dataset1[[1]])

	if(verbose)
 {
    print("Dataset Input after time modifications")
	str(dataset1)
	
	}
	res <- AnomalyDetectionTs(dataset1, max_anoms, direction,
                               alpha, only_last, threshold,
                               e_value, longterm, piecewise_median_period_weeks, plot,
                               y_log, xlabel, ylabel,
                               title, verbose, narm)
 
 if(verbose)
 {
  print("Result")
  str(res)
 
 }
 
 #res$anoms$timestamp <- as.character(res$anoms$timestamp, format="%Y-%m-%dT%I:%M:%S %Z")
 res$anoms[[1]] <- as.character(res$anoms[[1]], format="%Y-%m-%dT%I:%M:%S %Z")

 if(plot == TRUE){print(res$plot) }
 
 return(res$anoms)
 
}
