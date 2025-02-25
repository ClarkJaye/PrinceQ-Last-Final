﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrinceQ.Models.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime Created_At { get; set; }
    }
}