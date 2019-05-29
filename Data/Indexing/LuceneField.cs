using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data.Indexing
{
    public class LuceneField : Attribute
    {
        #region Properties
        public string Name { get; set; }
        public bool Store { get; set; } = true;
        public bool Index { get; set; } = true;
        public bool IsIdentity { get; set; } = false;
        #endregion

        #region Constructors
        public LuceneField() { }
        public LuceneField(bool store, bool index)
        {
            Store = store;
            Index = index;
        }
        #endregion
    }
}
