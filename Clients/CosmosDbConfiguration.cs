using ApplicationLogger;
using System;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Contains configuration about the cosmos db service.
    /// </summary>
    public class CosmosDbConfiguration : ICosmosDbConfiguration
    {
        private readonly ICosmosDocumentClient _cosmosDocumentClient;
        private readonly ILogger _logger;
        private readonly int _scaleWaitTime;

        /// <summary>
        /// Gets a value indicating whether throughput can be changed programatically.
        /// </summary>
        public bool ProgramaticallyChangeThroughput { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbConfiguration"/> class.
        ///  Constructor - instantiate instance of CosmosDbConfiguration.
        /// </summary>
        /// <param name="cosmosDocumentClient">Cosmos Document client.</param>
        /// <param name="logger">Application logger.</param>
        /// <param name="scaleWaitTime">Time to wait for throughput to take effect.</param>
        /// <param name="programaticallyChangeThroughput">programatically change throughput whilst running feed reader.</param>
        public CosmosDbConfiguration(ICosmosDocumentClient cosmosDocumentClient, ILogger logger, int scaleWaitTime, bool programaticallyChangeThroughput)
        {
            _cosmosDocumentClient = cosmosDocumentClient;
            _logger = logger;
            _scaleWaitTime = scaleWaitTime;
            ProgramaticallyChangeThroughput = programaticallyChangeThroughput;
        }

        /// <summary>
        /// Apply throughput at container level.
        /// </summary>
        /// <param name="collectionName">Collection to apply throughput value to.</param>
        /// <param name="throughPutSize">The throughtput amount, in RUs.</param>
        /// <returns>The throughput that has been set.</returns>
        public async Task<int?> ApplyNewCollectionThroughputAsync(string collectionName, int? throughPutSize)
        {
            if (!ProgramaticallyChangeThroughput || !throughPutSize.HasValue)
            {
                return 0;
            }

            var currentThroughput = await _cosmosDocumentClient.GetCurrentThroughputForCollection(collectionName);

            if (currentThroughput.HasValue && currentThroughput.Value == throughPutSize)
            {
                _logger.LogTrace($"{collectionName}: Already at throughput {throughPutSize}");
            }
            else if (currentThroughput.HasValue && await _cosmosDocumentClient.ChangeThroughputForCollection(throughPutSize.Value, collectionName))
            {
                _logger.LogTrace($"{collectionName}:  Waiting to scale throughput from {currentThroughput} to {throughPutSize}. Scale wait-time {_scaleWaitTime} ms");

                await Task.Delay(_scaleWaitTime);

                var newThroughput = await _cosmosDocumentClient.GetCurrentThroughputForCollection(collectionName);

                if (throughPutSize != newThroughput)
                {
                    var message = $"{collectionName}: Unable to change throughput from {currentThroughput.Value} to {throughPutSize} {_scaleWaitTime} ms";
                    var ex = new Exception(message);

                    _logger.LogException(ex, message);
                    throw ex;
                }

                _logger.LogTrace($"{collectionName}: Scaled throughput up from {currentThroughput} to {throughPutSize}");
            }
            else
            {
                var message = $"{collectionName}: Handle case whilst setting throughput to {throughPutSize}. Current = {currentThroughput.Value} {_scaleWaitTime} ms";
                var ex = new Exception(message);

                _logger.LogException(ex, message);
                throw ex;
            }

            return currentThroughput;
        }
    }
}