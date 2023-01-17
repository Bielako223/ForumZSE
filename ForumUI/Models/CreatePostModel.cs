using System.ComponentModel.DataAnnotations;

namespace ForumUI.Models;

public class CreatePostModel
{
   [Required]
   [MaxLength(length: 75)]
   public string Post { get; set; }

   [Required]
   [MinLength(length: 1)]
   [Display(Name = "Category")]
   public string CategoryId { get; set; }

   [MaxLength(length: 500)]
   public string Description { get; set; }

}