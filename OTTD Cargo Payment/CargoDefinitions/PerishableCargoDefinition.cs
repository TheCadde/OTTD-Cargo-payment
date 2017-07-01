using System.IO;

using OTTD_Cargo_Payment.Annotations;

using static System.Math;

namespace OTTD_Cargo_Payment.CargoDefinitions {
    /// <summary>
    /// A cargo definition for perishable cargo. Expensive calculations wise, use sparingly.
    /// </summary>
    /// <seealso cref="OTTD_Cargo_Payment.CargoDefinitions.CargoDefinition" />
    public class PerishableCargoDefinition : CargoDefinition {
        /// <summary>
        /// How many time units (2.5 days/minutes each) to offset the primary dropoff curve.
        /// In other words, a value of 4 will make it so the cargo pays maximum for 10 days/minutes and then starts dropping.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Curve offset",
            "How many time units (2.5 days/minutes each) to offset the primary dropoff curve.\n" +
            "In other words, a value of 4 will make it so the cargo pays maximum for 10 days/minutes and then starts dropping.",
            3, 0)]
        public int PrimaryDropoffCurveOffset { get; set; } = 20;

        /// <summary>
        /// For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the short distance span.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Short distance adjustment",
            "For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the short distance span.",
            3, 1)]
        public int PrimaryDropoffCurveShortDistanceAdjustment { get; set; } = 12;

        /// <summary>
        /// Where the short distance span ends and the medium distance span "should" start.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Short distance end", 
            "Where the short distance span ends and the medium distance span \"should\" start.",
            3, 2)]
        public int PrimaryDropoffCurveShortDistanceEnd { get; set; } = 500;

        /// <summary>
        /// Where (when) the medium distance span starts. Should be same as short distance end but can be customized in "strange" ways should there be a need to.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Medium distance start",
            "Where (when) the medium distance span starts. Should be same as short distance end but can be customized in \"strange\" ways should there be a need to.",
            3, 3)]
        public int PrimaryDropoffCurveMediumDistanceStart { get; set; } = 500;

        /// <summary>
        /// For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the medium distance span.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Medium distance adjustment",
            "For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the medium distance span.",
            3, 4)]
        public int PrimaryDropoffCurveMediumDistanceAdjustment { get; set; } = -15;

        /// <summary>
        /// Where the medium distance span ends and the long distance span "should" start.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Medium distance end",
            "Where the medium distance span ends and the long distance span \"should\" start.",
            3, 5)]
        public int PrimaryDropoffCurveMediumDistanceEnd { get; set; } = 1000;

        /// <summary>
        /// Where (when) the long distance span starts. Should be same as medium distance end but can be customized in "strange" ways should there be a need to.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Long distance start",
            "Where (when) the long distance span starts. Should be same as medium distance end but can be customized in \"strange\" ways should there be a need to.",
            3, 6)]
        public int PrimaryDropoffCurveLongDistanceStart { get; set; } = 1000;

        /// <summary>
        /// For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the long distance span.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Long distance adjustment",
            "For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the long distance span.",
            3, 7)]
        public int PrimaryDropoffCurveLongDistanceAdjustment { get; set; } = 200;

        /// <summary>
        /// Where the long distance span ends. From this point onwards, no more adjustments are possible.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Long distance end",
            "Where the long distance span ends. From this point onwards, no more adjustments are possible.",
            3, 8)]
        public int PrimaryDropoffCurveLongDistanceEnd { get; set; } = 2000;

        /// <summary>
        /// How long the primary dropoff curve should last. This is basically [time^2 / longevity] where larger values of longevity makes the dropoff curve last longer.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Longevity",
            "How long the primary dropoff curve should last. This is basically [time^2 / longevity] where larger values of longevity makes the dropoff curve last longer.",
            3, 9)]
        public int PrimaryDropoffCurveLongevity { get; set; } = 500;

        /// <summary>
        /// For every (positive or negative) n'th distance, add (positive) or remove (negative) one longevity unit to the primary dropoff curve longevity value.
        /// This can make the curve drop faster or slower when transporting long distances.
        /// </summary>
        [DynamicBinding("Primary dropoff curve", "Longevity adjustment",
            "For every (positive or negative) n'th distance, add (positive) or remove (negative) one longevity unit to the primary dropoff curve longevity value.\n" +
            "This can make the curve drop faster or slower when transporting long distances.",
            3, 10)]
        public int PrimaryDropoffCurveLongevityAdjustment { get; set; } = 10;

        /// <summary>
        /// How much of the maximum payout to pay during the secondary (I.E for boats) curve.
        /// </summary>
        [DynamicBinding("Profit basics", "Secondary return divisor",
            "How much of the maximum payout to pay during the secondary (I.E for boats) curve.",
            1, 1)]
        public int SecondaryReturnDivisor { get; set; } = 6;

        /// <summary>
        /// How much of the maximum payout to pay as a minimum no matter how long it takes.
        /// </summary>
        [DynamicBinding("Profit basics", "Final return divisor",
            "How much of the maximum payout to pay as a minimum no matter how long it takes.",
            1, 2)]
        public int FinalReturnDivisor { get; set; } = 12;

        /// <summary>
        /// How many time units (2.5 days/minutes each) to offset the secondary dropoff curve.
        /// In other words, a value of 100 will make it so the cargo pays maximum over the SecondaryReturnDivisor until 250 days/minutes and then starts dropping.
        /// </summary>
        [DynamicBinding("Secondary dropoff curve", "Curve offset",
            "How many time units (2.5 days/minutes each) to offset the secondary dropoff curve.\n" +
            "In other words, a value of 100 will make it so the cargo pays maximum over the SecondaryReturnDivisor until 250 days/minutes and then starts dropping.",
            4, 0)]
        public int SecondaryDropoffCurveOffset { get; set; } = 100;

        /// <summary>
        /// For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the secondary curve's distance offset.
        /// </summary>
        [DynamicBinding("Secondary dropoff curve", "Distance adjustment",
            "For every (positive or negative) n'th distance, add (positive) or remove (negative) one time unit in the secondary curve's distance offset.",
            4, 1)]
        public int SecondaryDropoffCurveDistanceAdjustment { get; set; } = -50;

        /// <summary>
        /// How long the secondary dropoff curve should last. This is basically [time^2 / longevity] where larger values of longevity makes the dropoff curve last longer.
        /// </summary>
        [DynamicBinding("Secondary dropoff curve", "Longevity",
            "How long the secondary dropoff curve should last. This is basically [time^2 / longevity] where larger values of longevity makes the dropoff curve last longer.",
            4, 2)]
        public int SecondaryDropoffCurveLongevity { get; set; } = 150;

        /// <summary>
        /// For every (positive or negative) n'th distance, add (positive) or remove (negative) one longevity unit to the secondary dropoff curve longevity value.
        /// This can make the curve drop faster or slower when transporting long distances.
        /// </summary>
        [DynamicBinding("Secondary dropoff curve", "Longevity adjustment",
            "For every (positive or negative) n'th distance, add (positive) or remove (negative) one longevity unit to the secondary dropoff curve longevity value.\n" +
            "This can make the curve drop faster or slower when transporting long distances.",
            4, 3)]
        public int SecondaryDropoffCurveLongevityAdjustment { get; set; } = 10;

        protected override int CalculateCallbackResult(int distance, int time) {

            var diminishingReturns = (100000 - DiminishingReturnsLinearity * 100000 / (Min(distance, MaxDistance) + DiminishingReturnsLinearity)) / (100 - DiminishingReturnsLinearity * 100 / (MaxDistance + DiminishingReturnsLinearity));

            var primaryDropoffCurveOffset = Max(0, time - (PrimaryDropoffCurveOffset +
                                                           Min(PrimaryDropoffCurveShortDistanceEnd, distance) / PrimaryDropoffCurveShortDistanceAdjustment +
                                                           Max(0, Min(PrimaryDropoffCurveMediumDistanceEnd, distance) - PrimaryDropoffCurveMediumDistanceStart) / PrimaryDropoffCurveMediumDistanceAdjustment +
                                                           Max(0, Min(distance, PrimaryDropoffCurveLongDistanceEnd) - PrimaryDropoffCurveLongDistanceStart) / PrimaryDropoffCurveLongDistanceAdjustment));

            var primaryDropoffCurve = primaryDropoffCurveOffset * 100 * primaryDropoffCurveOffset / Max(1, PrimaryDropoffCurveLongevity - distance / PrimaryDropoffCurveLongevityAdjustment);

            var secondaryDropoffCurveOffset = Max(0, time - Max(0, SecondaryDropoffCurveOffset + distance / SecondaryDropoffCurveDistanceAdjustment));
            var secondaryDropoffCurve = secondaryDropoffCurveOffset * 100 * secondaryDropoffCurveOffset / Max(1, SecondaryDropoffCurveLongevity - distance / SecondaryDropoffCurveLongevityAdjustment);

            return Max(1,
                            Min(1000,
                                     Max(
                                         diminishingReturns - primaryDropoffCurve,
                                         Max(diminishingReturns / SecondaryReturnDivisor - secondaryDropoffCurve,
                                                  diminishingReturns / FinalReturnDivisor)
                                     )
                            ));
        }
    }
}