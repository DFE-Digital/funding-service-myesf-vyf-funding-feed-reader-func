using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Extension methods for maing some Cosmos DB SDK tasks async.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Run a Cosmos DB SDK query async.
        /// </summary>
        /// <typeparam name="T">The task return type.</typeparam>
        /// <param name="queryable">The queryable query (from DocumentQuery or similar).</param>
        /// <returns>A task holding a list of type T.</returns>
        public static async Task<List<T>> ToListAsync<T>(this IDocumentQuery<T> queryable)
        {
            var list = new List<T>();

            while (queryable.HasMoreResults)
            {
                //Note that ExecuteNextAsync can return many records in each call
                var response = await queryable.ExecuteNextAsync<T>().ConfigureAwait(false);
                list.AddRange(response);
            }

            return list;
        }

        /// <summary>
        /// Run a Cosmos DB SDK query async (or sync if its not of the correct type).
        /// </summary>
        /// <typeparam name="T">The task return type.</typeparam>
        /// <param name="query">The queryable query (from DocumentQuery or similar).</param>
        /// <returns>A task holding a list of type T.</returns>
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> query)
        {
            if (query is IDocumentQuery)
            {
                var docQuery = query.AsDocumentQuery();
                return await docQuery.ToListAsync().ConfigureAwait(false);
            }

            return query.ToList();
        }
    }
}