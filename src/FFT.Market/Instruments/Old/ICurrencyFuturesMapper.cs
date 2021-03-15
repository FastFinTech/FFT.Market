using Apex.Tough;
using Apex.Tough.Instruments;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apex.Tough.Instruments {

    public interface ICurrencyFuturesMapper {

        IFutureContract[] GetContinuousFutureContracts(ICurrencyPair currencyPair);
        CurrencyPairFutureMasterCorrelations GetCurrencyFuturesCorrelations(ICurrencyPair currencyPair);
    }
}
