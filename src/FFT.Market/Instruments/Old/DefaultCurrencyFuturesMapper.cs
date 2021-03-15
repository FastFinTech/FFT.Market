using Apex.Market;
using Apex.Tough.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Apex.Tough.DependencyInjection.Services;
using static Apex.Market.CurrencyType;

namespace Apex.Tough.Instruments {

    internal class DefaultCurrencyFuturesMapper : ICurrencyFuturesMapper {

        public IFutureContract[] GetContinuousFutureContracts(ICurrencyPair currencyPair) {
            var futureMasters = GetCurrencyFuturesCorrelations(currencyPair).FutureMastersCorrelations.Select(c => c.FutureMaster);
            return futureMasters.Select(fm => InstrumentProvider.GetContinuousFutureContract(fm)).ToArray();
        }

        public CurrencyPairFutureMasterCorrelations GetCurrencyFuturesCorrelations(ICurrencyPair currencyPair) {

            var result = new CurrencyPairFutureMasterCorrelations { CurrencyPair = currencyPair };

            /// If this currency pair is a major currency cross, eg AUD/USD, then we'll be able to find a single future master.
            var correlation = GetSingleContractCorrelation(currencyPair.BaseCurrency, currencyPair.QuoteCurrency);
            if (null != correlation) {
                result.FutureMastersCorrelations = new[] { correlation };
                return result;
            }

            /// If this currency pair is NOT a major currency cross, eg AUD/NZD, we'll need to find a combination of future masters using a common major currency cross.
            /// TODO: At the moment, there is no preference given to the order in which major currencies are searched.
            var majors = _futuresMap.Select(m => m.FutureMasterQuote).Distinct().Select(s => CurrencyTypeExtensions.FromForexCode(s));
            foreach (var major in majors) {

                /// Note that it's important to get the input parameters in the correct order for each of these two lines below.
                var correlation1 = GetSingleContractCorrelation(currencyPair.BaseCurrency, major);
                var correlation2 = GetSingleContractCorrelation(major, currencyPair.QuoteCurrency);

                if (null != correlation1 && null != correlation2) {
                    result.FutureMastersCorrelations = new[] { correlation1, correlation2 };
                    return result;
                }
            }

            throw new Exception($"Unable to find future master correlation for currency pair '{currencyPair.BaseCurrency}/{currencyPair.QuoteCurrency}'.");
        }

        static FutureMasterAndCorrelation GetSingleContractCorrelation(CurrencyType baseCurrency, CurrencyType quoteCurrency) {
            var item = _futuresMap.SingleOrDefault(m => m.CurrencyPairBase == baseCurrency.ToForexCode() && m.CurrencyPairQuote == quoteCurrency.ToForexCode());
            if (null != item) return new FutureMasterAndCorrelation {
                FutureMaster = InstrumentProvider.GetFutureMaster(SymbolProviderType.Generic, item.FutureMasterName),
                Correlation = 1,
            };
            item = _futuresMap.SingleOrDefault(m => m.CurrencyPairBase == quoteCurrency.ToForexCode() && m.CurrencyPairQuote == baseCurrency.ToForexCode());
            if (null != item) return new FutureMasterAndCorrelation {
                FutureMaster = InstrumentProvider.GetFutureMaster(SymbolProviderType.Generic, item.FutureMasterName),
                Correlation = -1,
            };
            return null;
        }

        static readonly FutureMap[] _futuresMap = new[] {
            new FutureMap { FutureMasterName = "6A", FutureMasterQuote = "USD", CurrencyPairBase = "AUD", CurrencyPairQuote = "USD"},
            new FutureMap { FutureMasterName = "6B", FutureMasterQuote = "USD", CurrencyPairBase = "GBP", CurrencyPairQuote = "USD"},
            new FutureMap { FutureMasterName = "6C", FutureMasterQuote = "USD", CurrencyPairBase = "USD", CurrencyPairQuote = "CAD"},
            new FutureMap { FutureMasterName = "6E", FutureMasterQuote = "USD", CurrencyPairBase = "EUR", CurrencyPairQuote = "USD"},
            new FutureMap { FutureMasterName = "6J", FutureMasterQuote = "USD", CurrencyPairBase = "USD", CurrencyPairQuote = "JPY"},
            new FutureMap { FutureMasterName = "6N", FutureMasterQuote = "USD", CurrencyPairBase = "NZD", CurrencyPairQuote = "USD"},
            new FutureMap { FutureMasterName = "6S", FutureMasterQuote = "USD", CurrencyPairBase = "USD", CurrencyPairQuote = "CHF"},
            /// TODO: expand this system by adding future contracts traded in different currencies and based on currency pairs that are not USD-crosses.
        };

        class FutureMap {

            /// <summary>
            /// The generic symbol of the future contract.
            /// </summary>
            public string FutureMasterName;

            /// <summary>
            /// The currency in which a trader would profit or loss while trading the future contract.
            /// </summary>
            public string FutureMasterQuote;

            /// <summary>
            /// The base currency of the currency pair. ie, "AUD" in AUD/USD.
            /// </summary>
            public string CurrencyPairBase;

            /// <summary>
            /// The quote currency of the currency pair. ie, "USD" in AUD/USD.
            /// </summary>
            public string CurrencyPairQuote;
        }
    }
}
