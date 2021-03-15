// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Market.DataSeries {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class DataMismatchException : Exception {

        public DataMismatchException(string message) : base(message) { }
        public DataMismatchException(string message, Exception innerException) : base(message, innerException) { }
    }
}
