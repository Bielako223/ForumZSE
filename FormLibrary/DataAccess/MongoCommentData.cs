using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Core.Connections;
using System.Data;
using System.Xml.Linq;

namespace FormLibrary.DataAccess;
public class MongoCommentData : ICommentData
{
   private readonly IDbConnection _db;
   private readonly IMongoDatabase _dbs;
   private readonly IuserData _userData;
   private readonly IMemoryCache _cache;
   private readonly IMemoryCache _cache2;
   private readonly IMongoCollection<CommentModel> _comments;
   private readonly IPostData _postData;
   private const string CacheName = "CommentData";


   public MongoCommentData(IDbConnection db, IuserData userData, IMemoryCache cache,IMemoryCache cache2, IPostData post)
   {
      _db = db;
      _userData = userData;
      _cache = cache;
      _cache2 = cache2;
      _postData = post;
      _comments = db.CommentCollection;
   }

   public async Task CreateComment(CommentModel comment)
   {
      var client = _db.Client;

      using var session = await client.StartSessionAsync();

      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var postsInTransaction = db.GetCollection<CommentModel>(_db.CommentCollectionName);
         await postsInTransaction.InsertOneAsync(session, comment);

         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(comment.Author.Id);
         user.Comments.Add(item: new BasicCommentModel(comment.Id, comment.Comment));
         await usersInTransaction.ReplaceOneAsync(session, u => u.Id == user.Id, user);

         var postInTransaction = db.GetCollection<PostModel>(_db.PostCollectionName);
         var post = await _postData.GetPost(comment.Post.Id);
         post.Comment.Add(item: new BasicCommentModel(comment.Id,comment.Comment));
         await postInTransaction.ReplaceOneAsync(session, u => u.Id == post.Id, post);

         await session.CommitTransactionAsync();
      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }

   public async Task UpdateComment(CommentModel comment)
   {
      await _comments.ReplaceOneAsync(s => s.Id == comment.Id, comment);
      _cache.Remove(CacheName);
   }

   public async Task<List<CommentModel>> GetAllcomets(string Id)
   {

      var output = _cache.Get<List<CommentModel>>(Id);
      if (output is null)
      {
         var resoults = await _comments.FindAsync(s => s.Post.Id == Id);
         output = resoults.ToList();

         _cache.Set(Id, output, TimeSpan.FromMinutes(value: 1));
      }

      return output;

   }

 

   public async Task<List<CommentModel>> GetUsersComments(string Id)
   {
      var output = _cache.Get<List<CommentModel>>(CacheName);
      if (output is null)
      {
         var result = await _comments.FindAsync(s => s.Author.Id == Id);
         output = result.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromMinutes(value: 1));
      }

      return output;
   }

   public async Task DeleteCommentData(CommentModel comment)
   {
      var client = _db.Client;

      using var session = await client.StartSessionAsync();

      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(comment.Author.Id);
         user.Comments.Remove(user.Comments.Find(s => s.Id == comment.Id) );
         await usersInTransaction.ReplaceOneAsync(session, u => u.Id == user.Id, user);
         
         var postInTransaction = db.GetCollection<PostModel>(_db.PostCollectionName);
         var post = await _postData.GetPost(comment.Post.Id);
         post.Comment.Remove(post.Comment.Find(s => s.Id == comment.Id));
         await postInTransaction.ReplaceOneAsync(session, u => u.Id == post.Id, post);

         await session.CommitTransactionAsync();
      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }
}
