using Apex.Tough.Instruments;

namespace Apex.Tough.Instruments {

    /// <summary>
    /// Represents the correlation in price movement between a currency future contract and its underlying currency pair.
    /// For example, it can represent the relationship 6A has with not only AUDUSD but all other AUD crosses.
    /// </summary>
    public class FutureMasterAndCorrelation {

        /// <summary>
        /// The future master that is correlated to a currency pair.
        /// </summary>
        public IFutureMaster FutureMaster;

        /// <summary>
        /// This value is 1 if the future master's value increases when the currency pair's value increases. -1 otherwise.
        /// </summary>
        public int Correlation;
    }
}
