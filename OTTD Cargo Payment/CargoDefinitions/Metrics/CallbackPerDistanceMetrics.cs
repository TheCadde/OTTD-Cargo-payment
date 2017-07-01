using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using OxyPlot;

namespace OTTD_Cargo_Payment.CargoDefinitions.Metrics {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CallbackPerDistanceMetrics {
        private bool cacheInitialized;

        private static readonly int[] loadingTimes = { 4, 8, 12, 16, 20 };
        private static string dataStringFormat;

        public readonly int Distance;
        public readonly List<DataPoint> DataPoints;
        public CallbackDistanceRangeMetrics ParentRange { get; protected internal set; }

        public CallbackPerDistanceMetrics([NotNull] List<DataPoint> dataPoints, int distance) {
            DataPoints = dataPoints;
            Distance = distance;
        }

        public double FirstResult => DataPoints.First().Y;
        public double LastResult => DataPoints.Last().Y;

        public double FirstTime => DataPoints.First().X;

        public double LastTime => DataPoints.Last().X;

        private bool primaryDropTimeCached;
        private double primaryDropTime;
        private double primaryDropResult;

        public double PrimaryDropTime {
            get {
                if (!cacheInitialized)
                    InitializeCache();
                return primaryDropTime;
            }
        }

        public double PrimaryDropResult {
            get {
                if (!cacheInitialized)
                    InitializeCache();
                return primaryDropResult;
            }
        }


        private bool secondaryDropTimeCached;
        private double secondaryDropTime;
        private double secondaryDropResult;

        public double SecondaryDropTime {
            get {
                if (!cacheInitialized)
                    InitializeCache();
                return secondaryDropTime;
            }
        }

        public double SecondaryDropResult {
            get {
                if (!cacheInitialized)
                    InitializeCache();
                return secondaryDropResult;
            }
        }

        private void InitializeCache() {
            var firstResult = FirstResult;

            var primed = false;
            var lastResult = firstResult;

            foreach (var dp in DataPoints) {
                var result = dp.Y;
                if (Math.Abs(result - lastResult) <= 0)
                    primed = true;
                if (!primaryDropTimeCached && dp.Y < lastResult) {
                    primaryDropTime = Math.Max(0, dp.X - 2.5);
                    primaryDropResult = lastResult;
                    primaryDropTimeCached = true;

                    lastResult = result;
                    primed = false;
                }
                if (primed && !secondaryDropTimeCached && primaryDropTimeCached && dp.Y < lastResult) {
                    secondaryDropTime = Math.Max(0, dp.X - 2.5);
                    secondaryDropResult = lastResult;
                    secondaryDropTimeCached = true;
                }
                lastResult = result;
            }

            if (!primaryDropTimeCached) {
                primaryDropTime = double.PositiveInfinity;
                primaryDropResult = firstResult;
                primaryDropTimeCached = true;
            }
            if (!secondaryDropTimeCached) {
                secondaryDropTime = double.PositiveInfinity;
                secondaryDropResult = LastResult;
                secondaryDropTimeCached = true;
            }
            cacheInitialized = true;
        }

        public string GetHeaderString(string title) {
            var dataString = GetDataString(true);
            var headerWidth = dataString.Length + dataString.Length % 4;
            return 
                $"{new string('=', headerWidth)}\n" +
                $"{title.ToUpper()}        Speeds are in km/h, values in paratheses such as in \"Speed(4)\" refers to loading time in days/minutes.\n" +
                $"{new string('·', headerWidth)}\n" +
                $"{dataString}\n" +
                $"{new string('-', headerWidth)}";
        }

        public string GetDataString(bool isHeader = false) {
            List<object> data;

            // DATA
            if (!isHeader) {
                data =
                    new List<object> {
                        Distance,
                        "║",
                        PrimaryDropTime,
                        PrimaryDropResult,
                        "|",
                    };

                foreach (var loadingTime in loadingTimes)
                    data.Add(GetSpeedForDistanceAndTime(Distance, PrimaryDropTime - loadingTime));

                data.AddRange(
                    new List<object> {
                        "║",
                        SecondaryDropTime,
                        SecondaryDropResult,
                        "|"
                    });

                foreach (var loadingTime in loadingTimes)
                    data.Add(GetSpeedForDistanceAndTime(Distance, SecondaryDropTime - loadingTime));

                data.AddRange(
                    new List<object> {
                        "║",
                        LastResult,
                    });
            }
            // HEADER
            else {
                data =
                    new List<object> {
                        "Distance",
                        "║",
                        "Primary time",
                        "Result",
                        "|",
                    };
                var speedsAtLoad = loadingTimes.Select(loadingTime => $"Speed({loadingTime})").ToList();
                data.AddRange(speedsAtLoad);
                data.AddRange(
                    new[] {
                        "║",
                        "Secondary time",
                        "Result",
                        "|",
                    });
                data.AddRange(speedsAtLoad);
                data.AddRange(
                    new[] {
                        "║",
                        "Last result",
                    });
            }
            if (dataStringFormat == null)
                dataStringFormat = GetDataStringFormat();

            return string.Format(dataStringFormat, data.ToArray());
        }

        private static string GetDataStringFormat() {
            var iter = 0;
            var format = $"{{{iter++},-12}}{{{iter++},-4}}{{{iter++},-16}}{{{iter++},-8}}{{{iter++},-4}}";
            for (var i = 0; i < loadingTimes.Length; i++)
                format += $"{{{iter++},-12}}";
            format += $"{{{iter++},-4}}{{{iter++},-20}}{{{iter++},-8}}{{{iter++},-4}}";
            for (var i = 0; i < loadingTimes.Length; i++)
                format += $"{{{iter++},-12}}";
            format += $"{{{iter++},-4}}{{{iter}}}";
            return format;
        }

        public static double GetSpeedForDistanceAndTime(int distance, double time) {
            var speed = time <= 0 ? double.PositiveInfinity : Math.Round(distance / time / 0.036, 1);
            if (speed < 0)
                speed = double.PositiveInfinity;
            return speed;
        }
    }
}