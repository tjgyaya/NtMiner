﻿namespace NTMiner.MinerClient {
    public class GpuData : IGpuStaticData {
        public GpuData() { }

        public int Index { get; set; }
        public string Name { get; set; }

        public int CoreClockDeltaMin { get; set; }

        public int CoreClockDeltaMax { get; set; }

        public int MemoryClockDeltaMin { get; set; }

        public int MemoryClockDeltaMax { get; set; }
        public int CoolMin { get; set; }
        public int CoolMax { get; set; }
        public double PowerMin { get; set; }
        public double PowerMax { get; set; }
        public int TempLimitMin { get; set; }
        public int TempLimitDefault { get; set; }
        public int TempLimitMax { get; set; }
    }
}
