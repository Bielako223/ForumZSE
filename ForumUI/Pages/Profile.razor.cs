namespace ForumUI.Pages;
public partial class Profile
{
   private UserModel loggedInUser;
   private List<PostModel> submissions;
   private List<PostModel> approved;
   private List<PostModel> archived;
   private List<PostModel> pending;
   private List<PostModel> rejected;
   private List<CommentModel> comments;
   private bool kom = false;
   protected async override Task OnInitializedAsync()
   {
      loggedInUser = await authProvider.GetUserFromAuth(userData);
      var results = await postData.GetUsersPosts(loggedInUser.Id);

      var res = await commentData.GetUsersComments(loggedInUser.Id);
      if (loggedInUser is not null && results is not null && res is not null)
      {
         submissions = results.OrderByDescending(s => s.DateCreated).ToList();
         approved = submissions.Where(s => s.ApprovedForRelease && s.Archived == false & s.Rejected == false).ToList();
         archived = submissions.Where(s => s.Archived && s.Rejected == false).ToList();
         pending = submissions.Where(s => s.ApprovedForRelease == false && s.Rejected == false).ToList();
         rejected = submissions.Where(s => s.Rejected).ToList();
         comments = res.OrderByDescending(s => s.DateCreated).ToList();
      }
      else if (loggedInUser is not null && results is not null)
      {
         submissions = results.OrderByDescending(s => s.DateCreated).ToList();
         approved = submissions.Where(s => s.ApprovedForRelease && s.Archived == false & s.Rejected == false).ToList();
         archived = submissions.Where(s => s.Archived && s.Rejected == false).ToList();
         pending = submissions.Where(s => s.ApprovedForRelease == false && s.Rejected == false).ToList();
         rejected = submissions.Where(s => s.Rejected).ToList();
      }
      else if (loggedInUser is not null && res is not null)
      {
         comments = res.OrderByDescending(s => s.DateCreated).ToList();
      }
   }
   private void DeleteComment(CommentModel comment)
   {
      dbConnection.DeleteComment(comment);
      commentData.DeleteCommentData(comment);
      comments.Remove(item: comment);
   }

   private void OpenDetails(string Id)
   {
      navManager.NavigateTo($"/Details/{Id}");
   }

   private void ClosePage()
   {
      navManager.NavigateTo("/");
   }
}