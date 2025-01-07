namespace ForumUI.Pages;
public partial class Index
{

   private UserModel loggedInUser;
   private List<PostModel> post;
   private List<CategoryModel> categories;
   private PostModel archivingSuggestiopn;
   private string selectedCategory = "All";
   private string searchtext = "";
   bool isSortedByNew = true;
   private bool showCategories = false;
   protected async override Task OnInitializedAsync()
   {
      categories = await categoryData.GetAllCategories();
      await LoadAndVerifyUser();
   }

   private async Task Archivepost()
   {
      archivingSuggestiopn.Archived = true;
      await postData.UpdatePost(archivingSuggestiopn);
      post.Remove(archivingSuggestiopn);
      archivingSuggestiopn = null;
      //await Filterpost();
   }

   private void LoadCreatePage()
   {
      if (loggedInUser is not null)
      {
         navManager.NavigateTo("/Create");
      }
      else
      {
         navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
      }
   }

   private async Task LoadAndVerifyUser()
   {
      var authState = await authProvider.GetAuthenticationStateAsync();
      string objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;
      if (string.IsNullOrWhiteSpace(objectId) == false)
      {
         loggedInUser = await userData.GetUserFromAuthentication(objectId) ?? new();
         string firstName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("givenname"))?.Value;
         string lastName = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("surname"))?.Value;
         string displayName = authState.User.Claims.FirstOrDefault(c => c.Type.Equals("name"))?.Value;
         string email = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
         bool isDirty = false;
         if (objectId.Equals(loggedInUser.ObjectIdentifier) == false)
         {
            isDirty = true;
            loggedInUser.ObjectIdentifier = objectId;
         }

         if (firstName.Equals(loggedInUser.FirstName) == false)
         {
            isDirty = true;
            loggedInUser.FirstName = firstName;
         }

         if (lastName.Equals(loggedInUser.LastName) == false)
         {
            isDirty = true;
            loggedInUser.LastName = lastName;
         }

         if (displayName.Equals(loggedInUser.DisplayName) == false)
         {
            isDirty = true;
            loggedInUser.DisplayName = displayName;
         }

         if (email.Equals(loggedInUser.EmailAddress) == false)
         {
            isDirty = true;
            loggedInUser.EmailAddress = email;
         }

         if (isDirty)
         {
            if (string.IsNullOrWhiteSpace(loggedInUser.Id))
            {
               await userData.CreateUser(loggedInUser);
            }
            else
            {
               await userData.UpdateUser(loggedInUser);
            }
         }
      }
   }

   protected async override Task OnAfterRenderAsync(bool firstRender)
   {
      if (firstRender)
      {
         // await LoadFilterState();
         await FilterPosts();
         StateHasChanged();
      }
   }

   private async Task LoadFilterState()
   {
      var stringResults = await sessionStorage.GetAsync<string>(nameof(selectedCategory));
      selectedCategory = stringResults.Success ? stringResults.Value : "All";
      stringResults = await sessionStorage.GetAsync<string>(nameof(searchtext));
      searchtext = stringResults.Success ? stringResults.Value : "";
      var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
      isSortedByNew = boolResults.Success ? boolResults.Value : true;
   }

   private async Task SaveFilterState()
   {
      await sessionStorage.SetAsync(nameof(selectedCategory), selectedCategory);
      await sessionStorage.SetAsync(nameof(searchtext), searchtext);
      await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
   }

   private async Task FilterPosts()
   {
      var output = await postData.GetAllApprovedPosts();
      if (selectedCategory != "All")
      {
         output = output.Where(s => s.Category?.CategoryName == selectedCategory).ToList();
      }

      if (string.IsNullOrWhiteSpace(searchtext) == false)
      {
         output = output.Where(s => s.Post.Contains(searchtext, StringComparison.InvariantCultureIgnoreCase) || s.Description.Contains(searchtext, StringComparison.InvariantCultureIgnoreCase)).ToList();
      }

      if (isSortedByNew)
      {
         output = output.OrderByDescending(s => s.DateCreated).ToList();
      }
      else
      {
         output = output.OrderByDescending(s => s.UserVotes.Count).ThenByDescending(s => s.DateCreated).ToList();
      }

      post = output;
      await SaveFilterState();
   }

   private async Task OrderByNew(bool isNew)
   {
      isSortedByNew = isNew;
      await FilterPosts();
   }

   private async Task OnSearchInput(string searchInput)
   {
      searchtext = searchInput;
      await FilterPosts();
   }

   private async Task OnCategoryClick(string category = "All")
   {
      selectedCategory = category;
      showCategories = false;
      await FilterPosts();
   }

   private async Task VoteUp(PostModel post)
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
         if (isSortedByNew == false)
         {
            // post = post.OrderByDescending(s => s.UserVotes.Count).ThenByDescending(s => s.DateCreated).ToList();
         }
      }
      else
      {
         navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
      }
   }

   private string GetupvotetopText(PostModel post)
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

   private string GetUpvoteBottomtext(PostModel post)
   {
      if (post.UserVotes?.Count > 1)
      {
         return "likes";
      }
      else
      {
         return "like";
      }
   }

   private void OpenDetails(PostModel post)
   {
      navManager.NavigateTo($"/Details/{post.Id}");
   }

   private string SortedByNewClass(bool isNew)
   {
      if (isNew == isSortedByNew)
      {
         return "sort-selected";
      }
      else
      {
         return "";
      }
   }

   private string GetVoteClass(PostModel post)
   {
      if (post.UserVotes is null || post.UserVotes.Count == 0)
      {
         return "post-entry-no-votes";
      }
      else if (post.UserVotes.Contains(loggedInUser?.Id))
      {
         return "post-entry-voted";
      }
      else
      {
         return "post-entry-not-voted";
      }
   }



   private string GetSelectedCategory(string category = "All")
   {
      if (category == selectedCategory)
      {
         return "selected-category";
      }
      else
      {
         return "";
      }
   }
}