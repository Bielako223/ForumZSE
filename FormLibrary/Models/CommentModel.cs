
using MongoDB.Driver.Core.Operations;

namespace FormLibrary.Models;
public class CommentModel
{
   [BsonId]
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; }
   public BasicPostModel Post { get; set; }
   public string Comment { get; set; }
   public DateTime DateCreated { get; set; } = DateTime.UtcNow;
   public BasicUserModel Author { get; set; }
 
}
