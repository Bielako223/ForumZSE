using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Data;

namespace FormLibrary.DataAccess;
public class DbConnection : IDbConnection
{
   private readonly IConfiguration _config;
   private readonly IMongoDatabase _db;
   private readonly ICommentData commentData;
   private string _connectionId = "MongoDB";
   public string DbName { get; private set; }
   public string CategoryCollectionName { get; private set; } = "categories";
   public string UserCollectionName { get; private set; } = "users";
   public string PostCollectionName { get; private set; } = "Post";
   public string CommentCollectionName { get; private set; } = "Comment";

   public MongoClient Client { get; private set; }
   public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }
   public IMongoCollection<UserModel> UserCollection { get; private set; }
   public IMongoCollection<PostModel> PostCollection { get; private set; }
   public IMongoCollection<CommentModel> CommentCollection { get; private set; }

   public DbConnection(IConfiguration config)
   {
      _config = config;
      Client = new MongoClient(_config.GetConnectionString(_connectionId));
      DbName = _config[key: "DatabaseName"];
      _db = Client.GetDatabase(DbName);

      CategoryCollection = _db.GetCollection<CategoryModel>(CategoryCollectionName);
      UserCollection = _db.GetCollection<UserModel>(UserCollectionName);
      PostCollection = _db.GetCollection<PostModel>(PostCollectionName);
      CommentCollection = _db.GetCollection<CommentModel>(CommentCollectionName);
   }
   public async Task DeleteComment(CommentModel comm)
   {
      var table = _db.GetCollection<CommentModel>(CommentCollectionName);
      var comment = table.Find(c => c.Id == comm.Id).FirstOrDefault();
      table.DeleteOne(s => s.Id == comment.Id);
   }
}
