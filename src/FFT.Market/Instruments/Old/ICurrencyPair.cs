using Apex.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apex.Tough.Instruments {

    public interface ICurrencyPair : IInstrument {

        CurrencyType BaseCurrency { get; }
        CurrencyType QuoteCurrency { get; }

    }

    public static class ICurrencyPairExtensions {

        public static bool IsUSDBase(this ICurrencyPair pair) => pair.BaseCurrency == CurrencyType.UsDollar;
        public static bool IsUSDQuote(this ICurrencyPair pair) => pair.QuoteCurrency == CurrencyType.UsDollar;
        public static bool IsUSDCrossFor(this ICurrencyPair pair, CurrencyType currency) => (pair.IsUSDBase() && pair.QuoteCurrency == currency) || (pair.IsUSDQuote() && pair.BaseCurrency == currency);
    }
}
