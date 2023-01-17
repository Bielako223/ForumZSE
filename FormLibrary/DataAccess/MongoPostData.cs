using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using System.Data;

namespace FormLibrary.DataAccess;
public class MongoPostData : IPostData
{
   private readonly IDbConnection _db;
   private readonly IuserData _userData;
   private readonly IMemoryCache _cache;
   private readonly IMongoCollection<PostModel> _Posts;
   private const string CacheName = "SuggestionData";

   public MongoPostData(IDbConnection db, IuserData userData, IMemoryCache cache)
   {
      _db = db;
      _userData = userData;
      _cache = cache;
      _Posts = db.PostCollection;
   }

   public async Task<List<PostModel>> GetAllPosts()
   {
      var output = _cache.Get<List<PostModel>>(CacheName);
      if (output is null)
      {
         var result = await _Posts.FindAsync(s => s.Archived == false);
         output = result.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromMinutes(value: 1));
      }

      return output;
   }


   public async Task<List<PostModel>> GetUsersPosts(string userId)
   {
      var output = _cache.Get<List<PostModel>>(userId);
      if (output is null)
      {
         var results = await _Posts.FindAsync(s => s.Author.Id == userId);
         output = results.ToList();

         _cache.Set(userId, output, TimeSpan.FromMinutes(value: 1));
      }

      return output;
   }


   public async Task<List<PostModel>> GetAllApprovedPosts()
   {
      var output = await GetAllPosts();
      return output.Where(x => x.ApprovedForRelease).ToList();
   }

   public async Task<PostModel> GetPost(string id)
   {
      var results = await _Posts.FindAsync(s => s.Id == id);
      return results.FirstOrDefault();
   }

   public async Task<List<PostModel>> GetAllPostsWaitingForApproval()
   {
      var output = await GetAllPosts();

      return output.Where(x => x.ApprovedForRelease == false && x.Rejected == false).ToList();
   }

   public async Task UpdatePost(PostModel post)
   {
      await _Posts.ReplaceOneAsync(s => s.Id == post.Id, post);
      _cache.Remove(CacheName);
   }

   public async Task UpvotePost(string postId, string userId)
   {
      var client = _db.Client;

      using var session = await client.StartSessionAsync();

      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var postsInTransaction = db.GetCollection<PostModel>(_db.PostCollectionName);
         var post = (await postsInTransaction.FindAsync(s => s.Id == postId)).First();

         bool isUpvote = post.UserVotes.Add(userId);
         if (isUpvote == false)
         {
            post.UserVotes.Remove(userId);
         }

         await postsInTransaction.ReplaceOneAsync(session, s => s.Id == postId, post);

         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(userId);

         if (isUpvote)
         {
            user.VotedOnPosts.Add(item: new BasicPostModel(post));
         }
         else
         {
            var postToRemove = user.VotedOnPosts.Where(s => s.Id == postId).First();
            user.VotedOnPosts.Remove(postToRemove);
         }

         await usersInTransaction.ReplaceOneAsync(session, u => u.Id == userId, user);

         await session.CommitTransactionAsync();
         _cache.Remove(CacheName);
      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }

   public async Task CreatePost(PostModel post)
   {
      var client = _db.Client;

      using var session = await client.StartSessionAsync();

      session.StartTransaction();

      try
      {
         var db = client.GetDatabase(_db.DbName);
         var postsInTransaction = db.GetCollection<PostModel>(_db.PostCollectionName);
         await postsInTransaction.InsertOneAsync(session, post);

         var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
         var user = await _userData.GetUser(post.Author.Id);
         user.AuthoredPosts.Add(item: new BasicPostModel(post));
         await usersInTransaction.ReplaceOneAsync(session, u => u.Id == user.Id, user);

         await session.CommitTransactionAsync();
      }
      catch (Exception ex)
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }
}

