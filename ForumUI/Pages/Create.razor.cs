using ForumUI.Models;

namespace ForumUI.Pages;
public partial class Create
{
   private CreatePostModel post = new();
   private List<CategoryModel> categories;
   private UserModel loggedInUser;
   protected async override Task OnInitializedAsync()
   {
      categories = await categoryData.GetAllCategories();
      loggedInUser = await authProvider.GetUserFromAuth(userData);
   }

   private void ClosePage()
   {
      navManager.NavigateTo("/");
   }

   private async Task CreatePost()
   {
      PostModel s = new();
      s.Post = post.Post;
      s.Description = post.Description;
      s.Author = new BasicUserModel(loggedInUser);
      s.Category = categories.Where(c => c.Id == post.CategoryId).FirstOrDefault();
      if (s.Category is null)
      {
         post.CategoryId = "";
         return;
      }

      await postData.CreatePost(s);
      post = new();
      ClosePage();
   }
}