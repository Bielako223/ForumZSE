
namespace FormLibrary.Models;
public class BasicCommentModel
{
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public string Comment { get; set; }
   public BasicCommentModel()
   {

   }
   public BasicCommentModel(string id, string comment)
   {
      Id = id;
      Comment = comment;
   }
}
