using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apex.Tough.Instruments {

    public enum InstrumentType {
        Future = 0,
        Stock = 1,
        Index = 2,
        Option = 3,
        CurrencyPair = 4,
        FutureMaster = 5,
        Unknown = 99
    }
}
