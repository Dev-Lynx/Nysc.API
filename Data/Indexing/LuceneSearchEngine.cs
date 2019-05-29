using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Nysc.API.Data.Indexing.Interfaces;
using Nysc.API.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data.Indexing
{
    public class LuceneSearchEngine<TEntity> : ISearchEngine<TEntity>
    where TEntity : class
    {
        #region Properties
        public IIndexDefinition<TEntity> IndexDefinition { get; }

        #region Internals
        Analyzer Analyzer { get; }
        MMapDirectory MapDirectory { get; }

        IndexWriterConfig _writerConfiguration;
        IndexWriterConfig WriterConfiguration => (IndexWriterConfig)_writerConfiguration.Clone();
        IndexSearcher Searcher { get; set; }
        MultiFieldQueryParser Parser { get; set; }

        IndexReader Reader { get; set; }
        bool SearchActive { get; set; }
        #endregion

        #endregion

        #region Constructors
        public LuceneSearchEngine()
        {
            IndexDefinition = new EntityIndexDefinition<TEntity>();

            MapDirectory = new MMapDirectory(IndexDefinition.IndexDirectory);
            
            Analyzer = new StandardAnalyzer(Core.LuceneVersion);
            Parser = new MultiFieldQueryParser(Core.LuceneVersion, 
                IndexDefinition.GetSearchableFields().ToArray(), Analyzer);
            _writerConfiguration = new IndexWriterConfig(Core.LuceneVersion, Analyzer);
        }

        public LuceneSearchEngine(IIndexDefinition<TEntity> indexDefinition)
        {
            IndexDefinition = indexDefinition;
            if (IndexDefinition == null)
                IndexDefinition = new EntityIndexDefinition<TEntity>();

            MapDirectory = new MMapDirectory(IndexDefinition.IndexDirectory);
            Analyzer = new StandardAnalyzer(Core.LuceneVersion);
            Parser = new MultiFieldQueryParser(Core.LuceneVersion, 
                IndexDefinition.GetSearchableFields().ToArray(), Analyzer);
            _writerConfiguration = new IndexWriterConfig(Core.LuceneVersion, Analyzer);
        }
        #endregion


        public void Index(TEntity entity)
        {
            var document = IndexDefinition.Convert(entity);
            
            using (IndexWriter writer = new IndexWriter(MapDirectory, WriterConfiguration))
                writer.AddDocument(document);
        }

        public void Index(IEnumerable<TEntity> entities)
        {
            using (IndexWriter writer = new IndexWriter(MapDirectory, WriterConfiguration))
                foreach (var entity in entities)
                    writer.AddDocument(IndexDefinition.Convert(entity));
        }

        public IEnumerable<string> Find(string query, int max = 1000)
        {
            IEnumerable<string> results = null;

            if (!SearchActive)
            {
                Reader = DirectoryReader.Open(MapDirectory);
                SearchActive = true;
            }
            
            Searcher = new IndexSearcher(Reader);

            Query q = Parser.Parse("*" + query + "*");
            TopDocs docs = Searcher.Search(q, max);

            string identity = IndexDefinition.GetIdentityField();

            results = docs.ScoreDocs.Select(sd => Searcher
                .Doc(sd.Doc)).Select(d => d.GetField(identity)
                .GetStringValue());

            return results;
        }

        public void ClearIndexes() => Core.ClearDirectory(IndexDefinition.IndexDirectory);
    }
}
