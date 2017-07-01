using OTTD_Cargo_Payment.Annotations;

using static System.Math;

namespace OTTD_Cargo_Payment.CargoDefinitions {
    public class BulkCargoDefinition : CargoDefinition {
        /// <summary>
        /// The maximum delivery time in internal units. 1 unit = 2.5 days/minutes.
        /// </summary>
        [DynamicBinding("Timeout parameters", "Maximum delivery time",
            "The maximum delivery time in internal units. 1 unit = 2.5 days/minutes.",
            2, 0)]
        public int MaxDeliveryTime { get; set; } = 256;

        /// <summary>
        /// The late cargo divider
        /// </summary>
        [DynamicBinding("Timeout parameters", "Late cargo divider",
            "How much of the maximum payout to pay as a minimum even if it's late.",
            2, 1)]
        public int LateCargoDivider { get; set; } = 6;

        protected override int CalculateCallbackResult(int distance, int time) {
            var diminishingReturns = (100000 - DiminishingReturnsLinearity * 100000 / (Min(distance, MaxDistance) + DiminishingReturnsLinearity)) / (100 - DiminishingReturnsLinearity * 100 / (MaxDistance + DiminishingReturnsLinearity));

            var res = time <= MaxDeliveryTime ?
                          diminishingReturns :
                          diminishingReturns / LateCargoDivider;
            return res;
        }
    }
}