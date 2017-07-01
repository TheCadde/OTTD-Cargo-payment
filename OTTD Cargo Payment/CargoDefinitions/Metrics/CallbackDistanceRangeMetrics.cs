using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using OxyPlot;

namespace OTTD_Cargo_Payment.CargoDefinitions.Metrics {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CallbackDistanceRangeMetrics {
        private IReadOnlyList<List<DataPoint>> allDataPointsSeries;
        private IReadOnlyList<int> allDistances;
        private int startDistance;
        private int endDistance;

        public int StartDistance {
            get {
                if (allDistances == null)
                    InitializeCache();
                return startDistance;
            }
        }

        public int EndDistance {
            get {
                if (allDistances == null)
                    InitializeCache();
                return endDistance;
            }
        }

        public IReadOnlyList<int> AllDistances {
            get {
                if (allDistances == null)
                    InitializeCache();
                return allDistances;
            }
        }

        public int Interval => (EndDistance - StartDistance) / CallbackMetricses.Count;

        public double StartTime => CallbackMetricses.First().FirstTime;
        public double EndTime => CallbackMetricses.First().LastTime;

        public readonly IReadOnlyList<CallbackPerDistanceMetrics> CallbackMetricses;

        public IReadOnlyList<List<DataPoint>> AllDataPointsSeries {
            get {
                if (allDataPointsSeries == null)
                    InitializeCache();
                return allDataPointsSeries;
            }
        }

        public CallbackDistanceRangeMetrics(List<CallbackPerDistanceMetrics> callbackMetricses) {
            foreach (var metrics in callbackMetricses) {
                metrics.ParentRange = this;
            }
            CallbackMetricses = callbackMetricses.AsReadOnly();
        }

        public string GetDebugInfo(string title) {
            var sb = new StringBuilder();
            var iter = 0;
            foreach (var metrics in CallbackMetricses) {
                if (iter == 0)
                    sb.AppendLine(metrics.GetHeaderString(title));
                sb.AppendLine(metrics.GetDataString());
                iter++;
            }
            return sb.ToString();
        }

        private void InitializeCache() {
            var dataPoints = new List<List<DataPoint>>();
            var distances = new List<int>();
            foreach (var metrics in CallbackMetricses) {
                dataPoints.Add(metrics.DataPoints);
                distances.Add(metrics.Distance);
                startDistance = Math.Min(startDistance, metrics.Distance);
                endDistance = Math.Max(endDistance, metrics.Distance);
            }
            allDataPointsSeries = dataPoints.AsReadOnly();
            allDistances = distances.AsReadOnly();
        }
    }
}