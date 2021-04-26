using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFT.Market.Bars {

    public enum MarketDataType {
        Ask = 0,
        Bid = 1,
        Last = 2,
        DailyHigh = 3,
        DailyLow = 4,
        DailyVolume = 5,
        LastClose = 6,
        Opening = 7,
        Unknown = 8,
    }
}
