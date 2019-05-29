using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data.Indexing.Interfaces
{
    public interface IIndexDefinition<T> where T : class
    {
        string IndexDirectory { get; set; }

        Document Convert(T entity);
        Term GetIndex(T entity);
        IEnumerable<string> GetSearchableFields();
        string GetIdentityField();
    }
}
