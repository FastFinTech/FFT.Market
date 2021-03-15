using Apex.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apex.Tough.Instruments {

    public interface IStock : IInstrument {

        ExchangeType Exchange { get; }
    }
}
