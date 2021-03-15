using Apex.Tough.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apex.Tough.Instruments {

    /// <summary>
    /// Contains the correlations between a currency pair and all the future contracts that are affected by the currency pair.
    /// For example, CurrencyPair AUDUSD would contain a correlation with the 6A future contract, and CurrencyPair AUDNZD would 
    /// contain correlations with 6A and 6N.
    /// </summary>
    public class CurrencyPairFutureMasterCorrelations {

        /// <summary>
        ///  The currency pair that future masters were found for.
        /// </summary>
        public ICurrencyPair CurrencyPair;

        /// <summary>
        /// The future masters that are correlated to this currency pair.
        /// </summary>
        public FutureMasterAndCorrelation[] FutureMastersCorrelations;
    }
}
