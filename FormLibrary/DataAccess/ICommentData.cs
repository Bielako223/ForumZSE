namespace FormLibrary.DataAccess;

public interface ICommentData
{
   Task CreateComment(CommentModel comment);
   Task UpdateComment(CommentModel comment);
   Task<List<CommentModel>> GetAllcomets(string Id);
   Task<List<CommentModel>> GetUsersComments(string Id);
   Task DeleteCommentData(CommentModel comment);
}