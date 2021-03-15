using Apex.Market;
using Apex.TimeZones;
using Apex.Tough.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apex.Tough.Instruments {
    public static class SessionSettlementTimes {

        static readonly Dictionary<string, SessionSettlementTime> FuturesSettlementsData;

        static SessionSettlementTimes() {
            FuturesSettlementsData = Resources.FuturesSettlementTimes.Split('\n')
                .Skip(1) // file header
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .Select(l => l.Split('\t'))
                .ToDictionary(parts => parts[0], parts => ParseSessionSettlementTime(parts[1], parts[2]));
        }

        static SessionSettlementTime ParseSessionSettlementTime(string timeOfDayString, string timeZoneString) {
            var timeOfDay = TimeSpan.ParseExact(timeOfDayString, "c", null);
            if (timeOfDay <= TimeSpan.Zero) throw new Exception("TimeOfDay cannot be negative.");
            if (timeOfDay.TotalHours > 24) throw new Exception("TimeOfDay cannot be more than 24 hours. Settlement in early hours of following day is not yet supported.");
            var timeZone = TimeZoneReferences.Get((TimeZoneType)Enum.Parse(typeof(TimeZoneType), timeZoneString));
            return new SessionSettlementTime {
                TimeOfDay = timeOfDay,
                TimeZone = timeZone,
            };
        }


        public static SessionSettlementTime GetSettlementTime(IInstrument instrument) {
            switch (instrument) {
                case IFutureMaster futureMaster:
                    var symbol = futureMaster.NinjaTraderSymbol();
                    if (string.IsNullOrWhiteSpace(symbol)) throw new NotSupportedException($"Futures settlement time is not known.");
                    if (!FuturesSettlementsData.TryGetValue(symbol, out var sessionSettlementTime)) {
                        throw new NotSupportedException($"Futures settlement time is not known for future master '{symbol}'.");
                    }
                    return sessionSettlementTime;
                case IFutureContract futureContract:
                    return futureContract.FutureMaster.SettlementTime;
                case ICurrencyPair currencyPair:
                    return new SessionSettlementTime {
                        TimeZone = TimeZoneReferences.EasternStandardTime,
                        TimeOfDay = TimeSpan.FromHours(17), // 5pm EST
                    };
                case IStock stock:
                    if (stock.Exchange == ExchangeType.NYSE || stock.Exchange == ExchangeType.NASDAQ) {
                        return new SessionSettlementTime {
                            TimeZone = TimeZoneReferences.EasternStandardTime,
                            TimeOfDay = TimeSpan.FromHours(16), // 4pm EST
                        };
                    } else {
                        throw new NotSupportedException($"Stocks from exchange '{stock.Exchange}' don't have a settlement time yet.");
                    }
                default:
                    throw new NotSupportedException($"Instruments of type '{instrument.InstrumentType}' don't have a settlement time yet.");
            }
        }

    }
}
