namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;

    public class TimeBlock
    {
        #region private fields

        private DateTimeOffset _blockStart = DateTimeOffset.MinValue;
        private DateTimeOffset _blockEnd = DateTimeOffset.MaxValue;

        #endregion

        #region properties

        public DateTimeOffset BlockStart { get { return _blockStart; } }

        public DateTimeOffset BlockEnd { get { return _blockEnd; } }

        #endregion

        #region public methods

        public bool IsInBlock(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (_blockStart == DateTimeOffset.MinValue)
                return true;

            if (startTime > _blockEnd)
                return false;

            return true;
        }

        public void Add(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (_blockStart == DateTimeOffset.MinValue)
                _blockStart = startTime;

            if (_blockEnd == DateTimeOffset.MaxValue || endTime > _blockEnd)
                _blockEnd = endTime;
        }

        public TimeSpan GetBlockElapsed() => _blockEnd.Subtract(_blockStart);

        #endregion
    }
}
