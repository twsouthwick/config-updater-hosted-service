using System.Threading;

namespace BackgroundQuerySample
{
    public class SomeOptionsProvider
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private SomeOptions _options;

        public SomeOptionsProvider()
        {
            _options = new SomeOptions(false);
        }

        public SomeOptions Options
        {
            get
            {
                _lock.EnterReadLock();

                try
                {
                    return _options;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();

                try
                {
                    _options = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
    }
}
