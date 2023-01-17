using MongoDB.Driver;

namespace FormLibrary.DataAccess;
public interface IDbConnection
{
   IMongoCollection<CategoryModel> CategoryCollection { get; }
   string CategoryCollectionName { get; }
   MongoClient Client { get; }
   IMongoCollection<CommentModel> CommentCollection { get; }
   string CommentCollectionName { get; }
   string DbName { get; }
   IMongoCollection<PostModel> PostCollection { get; }
   string PostCollectionName { get; }
   IMongoCollection<UserModel> UserCollection { get; }
   string UserCollectionName { get; }
   Task DeleteComment(CommentModel comm);
}