using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Interface for updating throughput.
    /// </summary>
    public interface ICosmosDbConfiguration
    {
        /// <summary>
        /// Apply throughput that was specified in constructor.
        /// </summary>
        /// <param name="collectionName">Collection to apply throughput value to.</param>
        /// <param name="throughPutSize">The throughtput amount, in RUs.</param>
        /// <returns>New throughtput.</returns>
        Task<int?> ApplyNewCollectionThroughputAsync(string collectionName, int? throughPutSize);

        /// <summary>
        /// Gets a value indicating whether throughput can be changed programatically.
        /// </summary>
        bool ProgramaticallyChangeThroughput { get; }
    }
}