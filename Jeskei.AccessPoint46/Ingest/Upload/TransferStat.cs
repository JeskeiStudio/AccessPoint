namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;

    public class TransferStat
    {
        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public long ElapsedMs { get; set; }
    }
}
