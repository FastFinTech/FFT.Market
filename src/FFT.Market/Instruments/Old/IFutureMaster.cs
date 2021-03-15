// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Apex.Tough.Instruments
{
  using System.Linq;
  using FFT.TimeStamps;

  public interface IFutureMaster : IInstrument
  {
    Rollover[] Rollovers { get; }
  }

  public static class FutureMasterExtensions
  {
    public static IFutureContract GetContractAtDate(this IFutureMaster futureMaster, DateStamp sessionDate)
    {
      var deliveryMonth = futureMaster.Rollovers.Where(x => sessionDate >= x.RolloverDate).OrderByDescending(x => x.RolloverDate).First().DeliveryMonth;
      // Can also use this, since the code exists in the Apex.Market library, but I thought it's probably easier for the reader to see what's going on if we
      // manually write out the symbol calculation instead.
      //var ntSymbol = MarketSymbol.GetFutureContractSymbol(Market.SymbolProviderType.NinjaTrader8, futureMaster.NinjaTraderSymbol(), deliveryMonth.Month, deliveryMonth.Year);
      var ntSymbol = $"{futureMaster.NinjaTraderSymbol()} {deliveryMonth.DateTime:MM-yy}";
      return Services.InstrumentProvider.GetFutureContract(Market.SymbolProviderType.NinjaTrader8, ntSymbol);
    }
  }
}
