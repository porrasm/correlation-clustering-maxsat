#!/bin/bash 

# parameter 1
startDataPoint=$1

dataPointIncrement=$2

# parameter 2
repeatCount=$3

currentDataPoint=$((0+startDataPoint))

# ./run -i ../_problems/instances/dataset1.sim --parallel 1 --timestamp-csv --csv 1 --data-points 675

# run script with data points, increment after each iteration
# loop for repeatCount
for (( i=1; i<=repeatCount; i++ ))
do
    echo "Running with $currentDataPoint data points"
    ./run -i ../_problems/instances/dataset1.sim --parallel 1 --timestamp-csv --csv 1 --data-points $currentDataPoint
    currentDataPoint=$((currentDataPoint+dataPointIncrement))
done
