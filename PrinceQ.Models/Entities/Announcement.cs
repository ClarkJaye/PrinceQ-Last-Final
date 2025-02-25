﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrinceQ.Models.Entities
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        [ValidateNever]
        public string AddedBy { get; set; }

        [ValidateNever]
        public DateTime? Created_At { get; set; }


    }
}
