namespace FormLibrary.DataAccess;

public interface IPostData
{
   Task CreatePost(PostModel post);
   Task<List<PostModel>> GetAllApprovedPosts();
   Task<List<PostModel>> GetAllPosts();
   Task<List<PostModel>> GetAllPostsWaitingForApproval();
   Task<PostModel> GetPost(string id);
   Task<List<PostModel>> GetUsersPosts(string userId);
   Task UpdatePost(PostModel post);
   Task UpvotePost(string postId, string userId);
}