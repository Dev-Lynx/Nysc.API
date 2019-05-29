using Lucene.Net.Documents;
using Lucene.Net.Index;
using Nysc.API.Data.Indexing.Interfaces;
using Nysc.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Nysc.API.Data.Indexing
{
    public class EntityIndexDefinition<T> : IIndexDefinition<T> where T : class
    {
        #region Properties
        public string IndexDirectory { get; set; }
        #endregion

        public EntityIndexDefinition()
        {
            if (string.IsNullOrWhiteSpace(IndexDirectory))
                IndexDirectory = Path.Combine(Core.INDEX_DIR, typeof(T).Name);

            Core.CreateDirectories(IndexDirectory);
        }

        #region Methods
        public Document Convert(T entity)
        {
            Document document = new Document();

            var properties = typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.GetProperty | BindingFlags.SetProperty |
                BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes<LuceneField>(false)
                .Any());

            string name = string.Empty;
            string value = string.Empty;

            foreach (var prop in properties)
            {
                var attribute = prop.GetCustomAttribute<LuceneField>();

                
                name = string.IsNullOrWhiteSpace(attribute.Name) ? prop.Name : attribute.Name;
                value = string.Empty;

                try { value = prop.GetValue(entity).ToString(); }
                catch (Exception ex) { Core.Log.Error($"An error occured while working with {this}.\n\nProperty: {prop.Name}\n{ex}"); }

                FieldType fieldType = new FieldType()
                {
                    IsIndexed = attribute.Index,
                    IsStored = attribute.Store
                };

                document.Add(new Field(name, value, fieldType));
            }

            return document;
        }

        public Term GetIndex(T entity)
        {
            var properties = typeof(T).GetProperties(
               BindingFlags.Public | BindingFlags.NonPublic |
               BindingFlags.GetProperty | BindingFlags.SetProperty |
               BindingFlags.Instance)
               .Where(x => x.GetCustomAttributes<LuceneField>(false)
               .Any());

            string name = string.Empty;
            string value = string.Empty;

            foreach (var prop in properties)
            {
                var attribute = prop.GetCustomAttribute<LuceneField>();

                if (!attribute.IsIdentity) continue;

                name = string.IsNullOrWhiteSpace(name) ? prop.Name : name;

                try { value = prop.GetValue(entity).ToString(); }
                catch (Exception ex) { Core.Log.Error($"An error occured while working with {this}.\n{ex}"); }

                return new Term(name, value);
            }

            throw new InvalidOperationException($"Identity was not set to true in any of the properties of {entity.GetType()}, " +
                $"Please specify an indentity property before indexing it.");
        }

        public IEnumerable<string> GetSearchableFields()
        {
            return typeof(T).GetProperties(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.GetProperty |
                BindingFlags.SetProperty | BindingFlags.Instance)
                .Where(x =>
                {
                    var a = x.GetCustomAttribute<LuceneField>(true);
                    if (a == null) return false;
                    return a.Index;
                })
                .Select(x =>
                {
                    var a = x.GetCustomAttribute<LuceneField>(true);
                    string name = string.IsNullOrWhiteSpace(a.Name) ? 
                        x.Name : a.Name;
                    return name;
                });
        }

        public string GetIdentityField()
        {
            return typeof(T).GetProperties(
               BindingFlags.Public | BindingFlags.NonPublic |
               BindingFlags.GetProperty | BindingFlags.SetProperty |
               BindingFlags.Instance)
               .Where(x => 
               {
                   var a = x.GetCustomAttribute<LuceneField>(true);
                   return a != null && a.IsIdentity;
               })
               .Select(x =>
               {
                   var a = x.GetCustomAttribute<LuceneField>(true);
                   string name = string.IsNullOrWhiteSpace(a.Name) ?
                       x.Name : a.Name;
                   return name;
               }).FirstOrDefault();
        }
        #endregion
    }
}
