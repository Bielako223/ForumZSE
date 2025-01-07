namespace ForumUI.Pages;
public partial class AdminApproval
{
   private List<PostModel> submissions;
   private PostModel editingModel;
   private string currentEditingTitle = "";
   private string editedTitle = "";
   private string currentEditingDescription = "";
   private string editedDescription = "";
   protected override async Task OnInitializedAsync()
   {
      submissions = await postData.GetAllPostsWaitingForApproval();
   }

   private async Task ApprovSubmission(PostModel submission)
   {
      submission.ApprovedForRelease = true;
      submissions.Remove(submission);
      await postData.UpdatePost(submission);
   }

   private async Task RejectSubmission(PostModel submission)
   {
      submission.Rejected = true;
      submissions.Remove(submission);
      await postData.UpdatePost(submission);
   }

   private void EditTitle(PostModel model)
   {
      editingModel = model;
      editedTitle = model.Post;
      currentEditingTitle = model.Id;
      currentEditingDescription = "";
   }

   private async Task SaveTitle(PostModel model)
   {
      currentEditingTitle = string.Empty;
      model.Post = editedTitle;
      await postData.UpdatePost(model);
   }

   private void EditDescription(PostModel model)
   {
      editingModel = model;
      editedDescription = model.Description;
      currentEditingTitle = "";
      currentEditingDescription = model.Id;
   }

   private async Task SaveDescription(PostModel model)
   {
      currentEditingDescription = string.Empty;
      model.Description = editedDescription;
      await postData.UpdatePost(model);
   }

   private void ClosePage()
   {
      navManager.NavigateTo("/");
   }
}