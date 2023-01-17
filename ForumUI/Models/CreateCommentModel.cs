using System.ComponentModel.DataAnnotations;

namespace ForumUI.Models;

public class CreateCommentModel
{
   [Required]
   [MaxLength(length: 300)]
   public string Comment { get; set; }
}
