using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml.Serialization;

using JetBrains.Annotations;

using OTTD_Cargo_Payment.Annotations;
using OTTD_Cargo_Payment.CargoDefinitions.Metrics;

using OxyPlot;

using static System.Math;

namespace OTTD_Cargo_Payment.CargoDefinitions {
    /// <summary>
    /// Base class for all cargo definitions.
    /// </summary>
    [XmlInclude(typeof(PerishableCargoDefinition))]
    [XmlInclude(typeof(BulkCargoDefinition))]
    [Serializable]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public abstract class CargoDefinition : INotifyPropertyChanged {
        private string typeName = "<NOT SET>";
        private string cargoLabel = "UNKN";
        private int priceFactor = 1;
        private Color color = Colors.Magenta;

        private int maxDistance = 2000;
        private int diminishingReturnsLinearity = 1000;

        /// <summary>
        /// The internal number of the cargo. I.E, 0 for Passengers, 1 for Coal, 2 for Mail etc. See the industry mod you are targeting for actual values.
        /// </summary>
        [DynamicBinding("Cargo description", "Item ID",
            "The internal number (ID) of the cargo. I.E, 0 for Passengers, 1 for Coal, 2 for Mail etc. See the industry mod you are targeting for actual values.",
            0, 0)]
        public int ItemID { get; set; } = 0;

        /// <summary>
        /// The full name of the cargo, I.E, "Passengers", "Mail", "Livestock" etc.
        /// </summary>
        [DynamicBinding("Cargo description", "Type name",
             "The full name of the cargo, I.E, \"Passengers\", \"Mail\", \"Livestock\" etc.",
             0, 1)]
        public string TypeName {
            get { return typeName; }
            set {
                typeName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The cargo table name of this cargo. I.E, PASS, OIL_, LVST
        /// </summary>
        [DynamicBinding("Cargo description", "Cargo label",
            "The cargo table name of this cargo. I.E, PASS, OIL_, LVST",
            0, 2)]
        public string CargoLabel {
            get { return cargoLabel; }
            set {
                const string message = @"Cargo label has to be a 4 character string.";
                if (value == null)
                    throw new ArgumentNullException(nameof(CargoLabel), message);
                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(CargoLabel), message);

                cargoLabel = value.ToUpper();
            }
        }

        /// <summary>
        /// The base price (reward) of delivering one unit of cargo any distance. This is then multiplied by amount and callback return.
        /// </summary>
        [DynamicBinding("Profit basics", "Price factor",
             "The base price (reward) of delivering one unit of cargo any distance. This is then multiplied by amount and callback return.",
             1, 0)]
        public int PriceFactor {
            get { return priceFactor; }
            set {
                priceFactor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The color of the cargo in the graph. Not used outside this application.
        /// </summary>
        [DynamicBinding("Cargo description", "Graph color",
             "The color of the cargo in the graph. Not used outside this application.",
             0, 3)]
        public Color Color {
            get { return color; }
            set {
                color = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The maximum effective distance of this cargo. At distances beyond this, the pay is the same as the maximum distance cargo.
        /// Available range 1-8192.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is outside the range of 1 to 8192.</exception>
        [DynamicBinding("Distance parameters", "Maximum distance", 
            "The maximum effective distance of this cargo. At distances beyond this, the pay is the same as the maximum distance cargo.",
            2, 0)]
        public int MaxDistance {
            get { return maxDistance; }
            set {
                const int minValue = 1;
                const int maxValue = 8192;
                if (value < minValue || value > maxValue)
                    throw new ArgumentOutOfRangeException(nameof(MaxDistance), $@"Value has to be in the range of {minValue} to {maxValue}.");
                maxDistance = value;
            }
        }

        [DynamicBinding("Cargo description", "Cargo sprite",
            "The cargo sprite to use for this cargo. Either a fixed hex value or \"NEW_CARGO_SPRITE\", the latter for which you should have a spriteset ready in post edit and have Cargo icon assigned.",
            0, 3)]
        public string CargoSprite { get; set; } = "NEW_CARGO_SPRITE";

        [DynamicBinding("Cargo description", "Cargo icon",
            "The cargo icon to use for this cargo. For this you have to do post editing of the generated NML currently.",
            0, 4)]
        public string CargoIcon { get; set; } = "";

        /// <summary>
        /// Shapes the diminishing returns over distances.
        /// Higher values makes distance distribution more linear whereas lower values makes it so only the first couple of close distances will increase in profits.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is outside the range of 1 to 21474.</exception>

        [DynamicBinding("Distance parameters", "Linearity",
            "Shapes the diminishing returns over distances.\n" +
            "Higher values makes distance distribution more linear whereas lower values makes it so only the first couple of close distances will increase in profits.",
            2, 1)]
        public int DiminishingReturnsLinearity {
            get { return diminishingReturnsLinearity; }
            set {
                const int minValue = 1;
                const int maxValue = 21474;
                if (value < minValue || value > maxValue)
                    throw new ArgumentOutOfRangeException(nameof(DiminishingReturnsLinearity), $@"Value has to be in the range of {minValue} to {maxValue}.");
                diminishingReturnsLinearity = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CargoDefinition"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Calculates the callback return value for the specified distance and time.
        /// </summary>
        /// <param name="distance">The distance in tiles.</param>
        /// <param name="time">The time in internal time units. 1 unit = 2.5 days/minutes.</param>
        /// <returns>The expected callback return value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// distance - Distance cannot be smaller than 1 or greater than 8192.
        /// or
        /// startTime - Start time should not be smaller than 0.
        /// or
        /// startTime - Start time should not be larger than the End time.
        /// </exception>
        protected abstract int CalculateCallbackResult(int distance, int time);

        /// <summary>
        /// Gets callback metrics for the specified distance.
        /// </summary>
        /// <param name="distance">The distance to get metrics for.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>A <see cref="CallbackPerDistanceMetrics"/> result containing the callback returns for each time from start to end.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// distance - Distance cannot be smaller than 1 or greater than 8192.
        /// or
        /// startTime - Start time should not be smaller than 0.
        /// or
        /// startTime - Start time should not be larger than the End time.
        /// </exception>
        public CallbackPerDistanceMetrics GetCallbackResultsForDistance(int distance, int startTime = 0, int endTime = 255) {
            if (distance < 1 || distance > 8192)
                throw new ArgumentOutOfRangeException(nameof(distance), @"Distance cannot be smaller than 1 or greater than 8192.");
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), @"Start time should not be smaller than 0.");
            if (startTime > endTime)
                throw new ArgumentOutOfRangeException(nameof(startTime), @"Start time should not be larger than the End time.");

            var res = new List<DataPoint>();
            for (var time = 0; time <= endTime; time++)
                res.Add(new DataPoint(time * 2.5, CalculateCallbackResult(distance, time)));
            return new CallbackPerDistanceMetrics(res, distance);
        }

        /// <summary>
        /// Gets a range of metrics for the specified start to end distances.
        /// </summary>
        /// <param name="startDistance">The start distance.</param>
        /// <param name="endDistance">The end distance.</param>
        /// <param name="interval">The distance interval. If not specified, a value will be set to report 20 distances in total.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns>A <see cref="CallbackDistanceRangeMetrics"/> result containing the calculations for each distance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">endDistance - End distance should be larger than or equal to the start distance.</exception>
        public CallbackDistanceRangeMetrics GetCallbackResultsForDistances(int startDistance, int? endDistance = null, int? interval = null, int startTime = 0, int endTime = 255) {
            if (!endDistance.HasValue)
                endDistance = startDistance;
            if (endDistance.Value < startDistance)
                throw new ArgumentOutOfRangeException(nameof(endDistance), @"End distance should be larger than or equal to the start distance.");
            if (!interval.HasValue)
                interval = (endDistance.Value - startDistance) / 20;
            interval = Max(1, interval.Value);

            var results = new List<CallbackPerDistanceMetrics>();
            for (var dist = startDistance; dist <= endDistance; dist += interval.Value)
                results.Add(GetCallbackResultsForDistance(dist > 0 ? dist : 1, startTime, endTime));
            return new CallbackDistanceRangeMetrics(results);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}