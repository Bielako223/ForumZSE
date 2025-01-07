using ForumUI.Models;
using Microsoft.AspNetCore.Components;

namespace ForumUI.Pages;
public partial class Details
{
   [Parameter]
   public string Id { get; set; }
   private bool user;
   private string currentEditingDescription = "";
   private string editedDescription = "";
   private CommentModel editingModel;
   private PostModel post;
   private BasicPostModel basicPost;
   private CreateCommentModel comment = new();
   private UserModel loggedInUser;
   private List<CommentModel> comments;
   private string settingStatus = "";
   private string urlText = "";
   private bool writeCommet = false;
   protected async override Task OnInitializedAsync()
   {
      post = await postData.GetPost(Id);
      basicPost = new BasicPostModel(post);
      comments = await commentData.GetAllcomets(Id);
      await LoadAndVerifyUser();
      comments = comments.OrderByDescending(s => s.DateCreated).ToList();

   }

   private async Task LoadAndVerifyUser()
   {
      var authState = await authProvider.GetAuthenticationStateAsync();
      string objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;
      if (objectId == null)
      {
         user = false;
      }
      else
      {
         loggedInUser = await userData.GetUserFromAuthentication(objectId);
         user = true;
      }

   }

   private void ClosePage()
   {
      navManager.NavigateTo("/");
   }

   private string GetupvotetopText()
   {
      if (post.UserVotes?.Count > 0)
      {
         return post.UserVotes.Count.ToString("00");
      }
      else
      {
         if (post.Author.Id == loggedInUser?.Id)
         {
            return "Awaiting";
         }
         else
         {
            return "Kliknij by daæ";
         }
      }
   }

   private string GetUpvoteBottomtext()
   {
      if (post.UserVotes?.Count > 1)
      {
         return "Upvotes";
      }
      else
      {
         return "Upvote";
      }
   }

   private async Task VoteUp()
   {
      if (loggedInUser is not null)
      {
         if (post.Author.Id == loggedInUser.Id)
         {
            return;
         }

         if (post.UserVotes.Add(loggedInUser.Id) == false)
         {
            post.UserVotes.Remove(loggedInUser.Id);
         }

         await postData.UpvotePost(post.Id, loggedInUser.Id);
      }
      else
      {
         navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
      }
   }

   private string GetVoteClass()
   {
      if (post.UserVotes is null || post.UserVotes.Count == 0)
      {
         return "post-detail-no-votes";
      }
      else if (post.UserVotes.Contains(loggedInUser?.Id))
      {
         return "post-detail-voted";
      }
      else
      {
         return "post-detail-not-voted";
      }
   }

   private void CreateComment()
   {
      CommentModel s = new();
      s.Comment = comment.Comment;
      s.Author = new BasicUserModel(loggedInUser);
      s.Post = basicPost;
      commentData.CreateComment(s);
      comments.Add(item: s);
      comment = new();

      writeCommet = false;
   }

   private void EditDescription(CommentModel model)
   {
      editingModel = model;
      editedDescription = model.Comment;
      currentEditingDescription = model.Id;
   }

   private async Task SaveDescription(CommentModel model)
   {
      currentEditingDescription = string.Empty;
      model.Comment = editedDescription;
      await commentData.UpdateComment(model);
   }

   private void DeleteComment(CommentModel comment)
   {
      dbConnection.DeleteComment(comment);
      commentData.DeleteCommentData(comment);
      comments.Remove(item: comment);
   }

   private void CheckComment()
   {
      if (loggedInUser is not null)
      {
         writeCommet = true;
      }
      else
      {
         navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
      }
   }
}