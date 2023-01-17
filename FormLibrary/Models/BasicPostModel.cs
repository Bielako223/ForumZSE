namespace FormLibrary.Models;
public class BasicPostModel
{
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public string Post { get; set; }
   public BasicPostModel()
   {

   }
   public BasicPostModel(PostModel post)
   {
      Id = post.Id;
      Post = post.Post;
   }
}
