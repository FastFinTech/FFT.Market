using Apex.Market;
using Apex.TimeStamps;
using Apex.TimeZones;
using ApexInvesting.Platform;
using Apex.Tough.Instruments;
using Apex.Tough.Sessions.TradingHoursSessions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using FFT.TimeStamps;

namespace Apex.Tough.Instruments {

    public interface IInstrument : IEquatable<IInstrument> {
        Guid Id { get; }
        InstrumentType InstrumentType { get; }
        string GetSymbol(SymbolProviderType symbolProvider);
        double TickSize { get; }
        double TickValue { get; }
        double PointValue { get; }
        CurrencyType Currency { get; }
        TradingSessions TradingSessions { get; }
        SessionSettlementTime SettlementTime { get; }
    }

    public static class IInstrumentExtensions {


        public static string GenericSymbol(this IInstrument instrument) => instrument.GetSymbol(SymbolProviderType.Generic);
        public static string NinjaTraderSymbol(this IInstrument instrument) => instrument.GetSymbol(SymbolProviderType.NinjaTrader8);

        public static bool IsStock(this IInstrument instrument) => instrument.InstrumentType == InstrumentType.Stock;
        public static bool IsCurrencyPair(this IInstrument instrument) => instrument.InstrumentType == InstrumentType.CurrencyPair;
        public static bool IsFutureMaster(this IInstrument instrument) => instrument.InstrumentType == InstrumentType.FutureMaster;
        public static bool IsFutureContract(this IInstrument instrument) => instrument.InstrumentType == InstrumentType.Future;

        public static DateStamp SkipWeekendAndHolidaysMovingForward(this IInstrument instrument, DateStamp date) {
            return date.SkipWeekendAndTheseDatesMovingForward(instrument.HolidayDates());
        }

        public static DateStamp SkipWeekendAndHolidaysMovingBackward(this IInstrument instrument, DateStamp date) {
            return date.SkipWeekendAndTheseDatesMovingBackward(instrument.HolidayDates());
        }

        public static bool IsMarketDay(this IInstrument instrument, DateStamp date) {
            return !(date.IsWeekend() || instrument.HolidayDates().Contains(date));
        }

        public static DateStamp AddMarketDays(this IInstrument instrument, DateStamp date, int numMarketDays) {
            if (numMarketDays == 0) return date;
            for (var i = 1; i <= numMarketDays; i++) {
                do {
                    date = date.AddDays(1);
                } while (!instrument.IsMarketDay(date));
            }
            for (var i = -1; i >= numMarketDays; i--) {
                do {
                    date = date.AddDays(-1);
                } while (!instrument.IsMarketDay(date));
            }
            return date;
        }

        public static IEnumerable<DateStamp> HolidayDates(this IInstrument instrument)
            => instrument.TradingSessions.Holidays.Select(h => h.ExchangeDate);

        public static double Round2Tick(this IInstrument instrument, double value) {
            return value.Round2Tick(instrument.TickSize);
        }

        public static int ConvertPointsToTicks(this IInstrument instrument, double points) {
            return (int)Round(points / instrument.TickSize, MidpointRounding.AwayFromZero);
        }

        public static double ConvertTicksToPoints(this IInstrument instrument, int ticks) {
            return instrument.Round2Tick(ticks * instrument.TickSize);
        }

        public static double AddTicks(this IInstrument instrument, double price, int numTicks) {
            return instrument.Round2Tick(price + numTicks * instrument.TickSize);
        }

        public static string FormatPrice(this IInstrument instrument, double price) {
            /// Since most instruments have the same tick sizes, it was decided to optimize by using a cache for the format string.
            /// There should be a total of less than 20 keys in the cache.
            if (!formatCache.TryGetValue(instrument.TickSize, out var format)) {
                /// This can be optimized by removing the call to decimal.GetBits, but it was decided not to pursue it
                /// since code now rarely gets to this point thanks to the caching employed above. Here's the optimization:
                /// https://stackoverflow.com/questions/13477689/find-number-of-decimal-places-in-decimal-value-regardless-of-culture/13493771#13493771
                var numDecimals = decimal.GetBits((decimal)instrument.TickSize)[3] >> 16;
                format = numDecimals == 0 ? "0" : "0." + new string('0', numDecimals);
                formatCache[instrument.TickSize] = format;
            }
            return price.ToString(format, CultureInfo.InvariantCulture);
        }
        static readonly Dictionary<double, string> formatCache = new Dictionary<double, string>();


        #region InstrumentNameForSpeechSynthesizer
        static Dictionary<string, string> instrumentNameMappings = new Dictionary<string, string>()
        {
                {"$AUDUSD", "Aussie Dollar"},
                {"$EURUSD", "EURO Dollar"},
                {"$GBPUSD", "Pound Dollar"},
                {"$USDJPY", "Dollar Yen"},
                {"$GBPJPY", "Pound Yen"},
                {"$EURJPY", "Euro Yen"},
                {"$USDCHF", "Dollar Franc"},
                {"$USDCAD", "Dollar CAD"},
                {"$EURGBP", "Euro Pound"},
                {"$AUDJPY", "Aussie Yen"},
                {"SI", "Silver"},
                {"GC", "Gold"},
                {"HG", "Copper"},
                {"NG", "Natural Gas"},
                {"CL", "Oil"},
                {"FDAX", "Dax"},
                {"NU", "Nikkei"},
                {"NK", "Nikkei"},
                {"NN", "Nikkei"},
                {"ZC", "Corn"},
                {"ZS", "Soy Beans"}
        };
        public static string InstrumentNameForSpeechSynthesizer(this IInstrument instrument) {
            string result;
            var ninjaTraderSymbol = instrument.GetSymbol(SymbolProviderType.NinjaTrader);
            if (instrumentNameMappings.TryGetValue(ninjaTraderSymbol, out result)) {
                return result;
            }
            if (instrument.IsFutureContract()) {
                // insert spaces between each letter of the master instrument symbol so the voice spells out the 
                // symbol of the futures master
                var masterSymbol = (instrument as IFutureContract).FutureMaster.GetSymbol(SymbolProviderType.NinjaTrader);
                return string.Join(" ", masterSymbol.Select(c => c.ToString()).ToArray());
            }
            // some other instrument .. just do what we can
            return ninjaTraderSymbol;
        }
        #endregion

    }
}
