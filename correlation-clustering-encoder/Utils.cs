using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

public static class Utils {
    public static void SaveAssignments(string outputFile, ProtoLiteral[] assignments) {
        File.WriteAllText(outputFile, string.Join("\n", assignments.Select(s => s.GetDisplayString(true))));
    }

    public static bool FailedOnTwoConsecutiveAttempts(string workingDirectory, int currentMaxPoints, int dataPointIncrement) {
        var failedOnDataPoints = FailedOnPreviousAttempts(workingDirectory, currentMaxPoints, dataPointIncrement);

        if (failedOnDataPoints.Count < 2) {
            return false;
        }

        // Check if two consecutive failures have occurred
        for (int i = 1; i < failedOnDataPoints.Count; i++) {
            if (Math.Abs(failedOnDataPoints[i] - failedOnDataPoints[i - 1]) == dataPointIncrement) {
                return true;
            }
        }

        return false;
    }

    public static List<int> FailedOnPreviousAttempts(string workingDirectory, int currentMaxPoints, int dataPointIncrement) {
        string succesfulAttemptStringMatch = "OptimumFound";

        string currentDirMatch = $"{currentMaxPoints}p";

        var failedOnDataPoints = new List<int>();
        for (int dataPoints = currentMaxPoints - dataPointIncrement; dataPoints > 0; dataPoints -= dataPointIncrement) {
            string previousAttemptWorkingDirectory = workingDirectory.Replace(currentDirMatch, $"{dataPoints}p");

            // Check if we should stop trying subsequent data points
            if (failedOnDataPoints.Count > 1 && failedOnDataPoints[failedOnDataPoints.Count - 1] - dataPoints == dataPointIncrement &&
                failedOnDataPoints[failedOnDataPoints.Count - 2] - failedOnDataPoints[failedOnDataPoints.Count - 1] == dataPointIncrement) {
                break;
            }

            if (!Directory.Exists(previousAttemptWorkingDirectory)) {
                continue;
            }


            // find a CSV file in the previous attempt directory
            string[] files = Directory.GetFiles(previousAttemptWorkingDirectory, "*.csv");
            if (files.Length == 0) {
                continue;
            }

            string csvFile = files[0];

            // check if the CSV file contains the string "OptimumFound"
            string csvFileContents = File.ReadAllText(csvFile);
            if (!csvFileContents.Contains(succesfulAttemptStringMatch)) {
                System.Console.WriteLine($"Failed previously on {dataPoints} data points");
                failedOnDataPoints.Add(dataPoints);
            }
        }

        return failedOnDataPoints.OrderBy(p => p).ToList();
    }
}
