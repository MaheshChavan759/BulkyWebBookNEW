using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Author { get; set; }
        [Required,DisplayName("List Prise")]
        public int ListPrise { get; set; }
        [Required,DisplayName("Prise  for 1-50")]
        public int Prise50 { get; set; }

        [Required,DisplayName("Prise For 1-100")]
        public int Prise100 { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }


        [ValidateNever]
        
        public string ImageUrl { get; set; }

    }
}
