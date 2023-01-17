

namespace FormLibrary.Models;
public class UserModel
{
   [BsonId]
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public string ObjectIdentifier { get; set; }
   public string FirstName { get; set; }
   public string LastName { get; set; }
   public string DisplayName { get; set; }
   public string EmailAddress { get; set; }
   public List<BasicPostModel> AuthoredPosts { get; set; } = new();
   public List<BasicPostModel> VotedOnPosts { get; set; } = new();
   public List<BasicCommentModel> Comments { get; set; } = new();
}
