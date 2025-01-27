using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GwentCardDownloader.Utils
{
    public class ErrorHandler
    {
        private readonly Logger _logger;
        private readonly Dictionary<Type, Func<Exception, Task>> _handlers;

        public ErrorHandler(Logger logger)
        {
            _logger = logger;
            _handlers = new Dictionary<Type, Func<Exception, Task>>
            {
                { typeof(HttpRequestException), HandleHttpError },
                { typeof(IOException), HandleIOError },
                { typeof(ImageProcessingException), HandleImageError }
            };
        }

        public async Task HandleError(Exception ex)
        {
            var handler = _handlers.GetValueOrDefault(ex.GetType());
            if (handler != null)
            {
                await handler(ex);
            }
            else
            {
                _logger.Error(ex, "Unhandled error occurred");
            }
        }

        private Task HandleHttpError(Exception ex)
        {
            _logger.Error(ex, "HTTP error occurred");
            return Task.CompletedTask;
        }

        private Task HandleIOError(Exception ex)
        {
            _logger.Error(ex, "IO error occurred");
            return Task.CompletedTask;
        }

        private Task HandleImageError(Exception ex)
        {
            _logger.Error(ex, "Image processing error occurred");
            return Task.CompletedTask;
        }
    }
}
