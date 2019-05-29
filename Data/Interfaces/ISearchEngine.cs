using Lucene.Net.Analysis;
using Nysc.API.Data.Indexing.Interfaces;
using Nysc.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data.Interfaces
{
    public interface ISearchEngine<T> where T : class
    {
        #region Properties
        IIndexDefinition<T> IndexDefinition { get; }
        #endregion

        #region Methods
        void Index(T entity);
        void Index(IEnumerable<T> entities);
        IEnumerable<string> Find(string query, int max = 1000);
        void ClearIndexes();
        #endregion
    }
}
