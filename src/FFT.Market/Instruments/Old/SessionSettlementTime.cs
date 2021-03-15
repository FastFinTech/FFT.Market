using Apex.TimeZones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apex.Tough.Instruments {

    /// <summary>
    /// Describes the settlement time of an instrument, with respect to its session date.
    /// </summary>
    public class SessionSettlementTime {

        /// <summary>
        /// The timezone that the instrument's settlement time is expressed in.
        /// </summary>
        public TimeZoneInfo TimeZone;

        /// <summary>
        /// The settlement time of day, in the timezone given by <see cref="TimeZone"/>.
        /// When settlement occurs in the morning of the next day, this field contains a value that is greater than 24 hours. 
        /// For example, if Friday's settlement is at 2am on Saturday, this field will contain the value '26 hours'
        /// </summary>
        public TimeSpan TimeOfDay;
   }
}
