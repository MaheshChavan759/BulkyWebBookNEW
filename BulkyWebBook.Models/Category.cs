using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyWebBook.Models
{
    public class Category
    {
        [Key]
        
        public int CategoryId { get; set; }
        [Required]
        [MaxLength(20)]
        [DisplayName("Category Name")]
        public String CategoryName { get; set; }
        [Range(1,100),DisplayName("Category Order")]       
        public int DisplaeyCategoryOrder { get; set; }
    }
}
