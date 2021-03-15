//using FFT.Market.DataSeries;
//using FFT.Market.Instruments;
//using FFT.Market.Sessions.TradingHoursSessions;
//using FFT.Market.Ticks;
//using FFT.Market.TradingSimulation;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace FFT.Market.ProcessingContexts {

//    // This class is declared partial because every engine adds a method to this class for getting/creating an engine of its own type.
//    public partial class EngineProvider {


//        readonly object sync = new object();
//        public ProcessingContext ProcessingContext { get; }
//        public List<EngineBase> Engines { get; }

//        public EngineProvider(ProcessingContext processingContext) {
//            ProcessingContext = processingContext;
//            Engines = new List<EngineBase>();
//        }

//        public T GetOrCreateEngine<T>(Func<T, bool> search, Func<ProcessingContext, T> create) where T : EngineBase {
//            lock (sync) {
//                var engine = Engines.OfType<T>().SingleOrDefault(search);
//                if (null == engine) {
//                    engine = create(ProcessingContext);
//                    Engines.Add(engine);
//                }
//                return engine;
//            }
//        }

//        /// <summary>
//        /// Causes each of the engines to process the given tick in order of their dependence on each other.
//        /// This method is intended to be called by ProcessingContext objects
//        /// </summary>
//        public void ProcessTick(Tick tick) {
//            foreach (var engine in Engines)
//                engine.OnTick(tick);
//        }
//    }
//}
