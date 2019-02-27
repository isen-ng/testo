using System.Net.Http;
using System.Threading.Tasks;
using ArangoDB.Client;
using ArangoDB.Client.Data;
using ArangoDB.Client.Http;
using ArangoDB.Client.Serialization;
using ArangoDB.Client.Utility;
using DocumentDbTest.Arango.Models;

namespace DocumentDbTest.Arango
{
    // Arango error numbers: https://docs.arangodb.com/3.4/Manual/Appendix/ErrorCodes.html
    public static class ArangoDatabaseExtensions
    {
        public static Task<CreateCollectionResult> CreateCollectionAsync<T>(this IArangoDatabase db)
        {
            var collectionName = db.SharedSetting.Collection.ResolveCollectionName<T>();
            return db.CreateCollectionAsync(collectionName);
        }

        public static async Task<(bool success, CreateCollectionResult result)> TryCreateCollectionAsync<T>(
            this IArangoDatabase db)
        {
            var result = await db.CreateCollectionAsync<T>();
            if (result.HasError() && (result.Code != 409 || result.ErrorNum != 1207))
            {
                return (false, result);
            }

            return (true, result);
        }

        public static async Task<GetCollectionResult> GetCollectionAsync<T>(this IArangoDatabase db)
        {
            var collectionName = db.SharedSetting.Collection.ResolveCollectionName<T>();

            var httpCommand = new HttpCommand(db)
            {
                Api = CommandApi.Collection, 
                Method = HttpMethod.Post,
                Command = StringUtils.Encode(collectionName)
            };
            
            var commandResult = await httpCommand.RequestMergedResult<GetCollectionResult>()
                .ConfigureAwait(false);
            
            return commandResult.Result;
        }

        public static async Task<(bool success, DocumentIdentifierBaseResult result)> TryInsertAsync<T>(
            this IArangoDatabase db, T document)
        {
            var result = (DocumentIdentifierBaseResult) await db.InsertAsync<T>(document);
            if (!result.HasError())
            {
                return (true, result);
            }
            
            return (false, result);
        }
        
        public static async Task<(bool success, DocumentIdentifierBaseResult result)> TryUpsertDocumentAsync<T>(
            this IArangoDatabase db, T document)
        {
            var result = (DocumentIdentifierBaseResult) await db.InsertAsync<T>(document);
            if (!result.HasError())
            {
                return (true, result);
            }

            if (result.Code != 409 || (result.ErrorNum != 1207 && result.ErrorNum != 1210))
            {
                return (false, result);
            }

            var keyOrId = FindKeyOrId(db, document);
            if (keyOrId == null)
            {
                return (false, new DocumentIdentifierBaseResult
                {
                    Code = 400,
                    ErrorNum = 1219,
                    ErrorMessage = "Document must have a key or id"
                });
            }

            var replaceResult = (DocumentIdentifierBaseResult) await db.ReplaceByIdAsync<T>(keyOrId, document);
            return !replaceResult.HasError() ? (true, replaceResult) : (false, replaceResult);
        }

        public static async Task<(bool success, T result)> TryGetDocumentAsync<T>(this ArangoDatabase db, string key)
        {
            try
            {
                var wrapped = await db.DocumentAsync<T>(key);
                return (true, wrapped);
            }
            catch (ArangoServerException e)
            {
                if (e.Message.Contains("404"))
                {
                    return (false, default(T));
                }

                throw;
            }
        }

        private static string FindKeyOrId<T>(IArangoDatabase db, T document)
        {
            var serializer = new DocumentSerializer(db);
            var jObject = serializer.FromObject(document);

            return jObject.Value<string>("_id") ?? jObject.Value<string>("_key");
        }
    }
}