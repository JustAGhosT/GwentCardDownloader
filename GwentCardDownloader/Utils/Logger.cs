using NLog;

namespace GwentCardDownloader.Utils
{
    public class Logger
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Warn(string message)
        {
            logger.Warn(message);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Error(Exception ex, string message)
        {
            logger.Error(ex, message);
        }
    }
}
