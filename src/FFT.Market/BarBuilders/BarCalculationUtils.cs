//using FFT.Market.Instruments;
//using FFT.Market.DataSeries;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static System.Math;
//using FFT.Market.Bars;

//namespace FFT.Market.BarBuilders {

//    public static class BarCalculationUtils {

//        public static bool CanPredictHiLo(this IBars bars) {
//            switch (bars.BarsInfo.Period.Type) {

//                case PeriodType.Tick:
//                case PeriodType.Minute:
//                case PeriodType.Second:
//                    return false;

//                case PeriodType.Range:
//                case PeriodType.Diagnostic:
//                case PeriodType.Diagnostic1WithRoundedFXPrices:
//                case PeriodType.Diagnostic2:
//                case PeriodType.Shadow:
//                case PeriodType.ShadowContinuumV2:
//                case PeriodType.ShadowContinuumV2WithBug:
//                    return true;

//                case PeriodType.ShadowContinuum: // deprecated bars type
//                default:
//                    throw new NotImplementedException();
//            }
//        }

//        public static Direction GetOpenCloseDirection(this IBars bars, int index) {
//            return bars.GetClose(index) >= bars.GetOpen(index) ? Direction.Up : Direction.Down;
//        }

//        /// <summary>
//        /// Predicts the furthest possible high or low for a bar, starting with the bar at "startIndex" and moving forward "barsAhead" bars.
//        /// Use barsAhead == 0 to get values for the bar at startIndex.
//        /// For Diagnostic and Shadow bars, a true future high or low is calculated.
//        /// For range bars we return an ideal high or low if the trend only continues in the given direction.
//        /// For any other type of bars, the current high/low is returned - a cheat's way out.
//        /// This method is cpu-intensive, so avoid using it frequently.
//        /// </summary>
//        /// <remarks>It won't work for startIndex &lt; 1 for Shadow bars.</remarks>
//        public static double PredictFurthestHILO(this IBars bars, Direction direction, int startIndex, int barsAhead) {
//            switch (bars.BarsInfo.Period.Type) {

//                case PeriodType.Tick:
//                case PeriodType.Minute:
//                case PeriodType.Second:
//                    return direction.IsUp ? bars.GetHigh(startIndex) : bars.GetLow(startIndex);

//                case PeriodType.Range: {
//                    var barSizeInTicks = (bars.BarsInfo.Period as RangePeriod).TicksPerBar;
//                    var ticksToAdd = ((int)direction) * (barSizeInTicks + (barsAhead * (barSizeInTicks + 1)));
//                    var start = direction.IsUp ? bars.GetLow(startIndex) : bars.GetHigh(startIndex);
//                    return bars.BarsInfo.Instrument.AddTicks(start, ticksToAdd);
//                }

//                case PeriodType.Diagnostic: {
//                    var barSizeInTicks = (bars.BarsInfo.Period as DiagnosticPeriod).BarSizeInTicks;
//                    var ticksToAdd = ((int)direction) * (barSizeInTicks + (barsAhead * (barSizeInTicks + 1)));
//                    return bars.BarsInfo.Instrument.AddTicks(bars.GetOpen(startIndex), ticksToAdd);
//                }

//                case PeriodType.Diagnostic1WithRoundedFXPrices: {
//                    var barSizeInTicks = (bars.BarsInfo.Period as DiagnosticWithRoundedFXPeriod).BarSizeInTicks;
//                    var ticksToAdd = ((int)direction) * (barSizeInTicks + (barsAhead * (barSizeInTicks + 1)));
//                    return bars.BarsInfo.Instrument.AddTicks(bars.GetOpen(startIndex), ticksToAdd);
//                }

//                case PeriodType.Diagnostic2: {
//                    var trendBarSizeInTicks = (bars.BarsInfo.Period as Diagnostic2Period).TrendBarSizeInTicks;
//                    var existingTrend = startIndex == 0 ? Direction.Up : bars.GetOpenCloseDirection(startIndex - 1);
//                    var ticksToAdd = trendBarSizeInTicks + (barsAhead * (trendBarSizeInTicks + 1));
//                    if (direction != existingTrend) {
//                        // First bar will be a reversal bar. Following bars will be trend bars, so we just have to
//                        // swap out one trend bar for a reversal bar.
//                        var reversalBarSizeInTicks = (bars.BarsInfo.Period as Diagnostic2Period).ReversalBarSizeInTicks;
//                        ticksToAdd += (reversalBarSizeInTicks - trendBarSizeInTicks);
//                    }
//                    ticksToAdd *= (int)direction;
//                    return bars.BarsInfo.Instrument.AddTicks(bars.GetOpen(startIndex), ticksToAdd);
//                }

//                case PeriodType.Diagnostic1A: {
//                    var period = bars.BarsInfo.Period as DiagnosticPeriod1A;
//                    var trend = bars.IsFirstBarOfSession(startIndex) ? Direction.Up : bars.GetOpenCloseDirection(startIndex - 1);
//                    period.CalculateBarParameters(bars.BarsInfo.Instrument, out var barSizeInPoints, out var overlapInPoints, out var reversalSizeInPoints);
//                    if (direction == trend) {
//                        return bars.BarsInfo.Instrument.Round2Tick(bars.GetOpen(startIndex) + (direction * barSizeInPoints) + (direction * barsAhead * (barSizeInPoints - overlapInPoints)));
//                    } else {
//                        return bars.BarsInfo.Instrument.Round2Tick(bars.GetOpen(startIndex) + (direction * reversalSizeInPoints) + (direction * barsAhead * (barSizeInPoints - overlapInPoints)));
//                    }
//                }

//                case PeriodType.Shadow: {
//                    var period = bars.BarsInfo.Period as ShadowPeriod;
//                    var currentBarsTrend = bars.GetOpenCloseDirection(startIndex - 1);
//                    var ticksToAdd = ((int)direction) * ((currentBarsTrend == direction ? period.TrendBodySizeInTicks : period.ReversalBodySizeInTicks) + (barsAhead * (period.TrendBodySizeInTicks + 1)));
//                    return bars.BarsInfo.Instrument.AddTicks(bars.GetOpen(startIndex), ticksToAdd);
//                }

//                case PeriodType.ShadowContinuumV2: {
//                    var period = bars.BarsInfo.Period as ShadowContinuumV2Period;
//                    ShadowContinuumBarBuilder.GetOpeningBarData(bars, startIndex, out var openingIndex, out var openingPrice, out var openingDirection);
//                    var ticksToAdd = ((int)direction) * ((openingDirection == direction ? period.TrendBodySizeInTicks : period.ReversalBodySizeInTicks) + (barsAhead * (period.TrendBodySizeInTicks + 1)));
//                    return bars.BarsInfo.Instrument.AddTicks(openingPrice, ticksToAdd);
//                }

//                case PeriodType.ShadowContinuumV2WithBug: {
//                    var period = bars.BarsInfo.Period as ShadowContinuumV2WithBugPeriod;
//                    var currentBarsTrend = bars.GetOpenCloseDirection(startIndex - 1);
//                    var ticksToAdd = ((int)direction) * ((currentBarsTrend == direction ? period.TrendBodySizeInTicks : period.ReversalBodySizeInTicks) + (barsAhead * (period.TrendBodySizeInTicks + 1)));
//                    return bars.BarsInfo.Instrument.AddTicks(bars.GetOpen(startIndex), ticksToAdd);
//                }

//                case PeriodType.ShadowContinuum: // deprecated bars type
//                default:
//                    throw new NotImplementedException();
//            }
//        }

//        /// <summary>
//        /// CPU-intensive!
//        /// </summary>
//        public static bool IsFirstBarOfSession(this IBars bars, int index) {
//            if (index == 0) return true;
//            var currentSession = bars.BarsInfo.TradingHours.GetActualSessionsFrom(bars.GetTimeStamp(index)).First();
//            return currentSession.SessionStart < bars.GetTimeStamp(index - 1);
//        }
//    }
//}
