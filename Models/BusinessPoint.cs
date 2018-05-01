using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace counter.Models
{
    public class BusinessPoint
    {
        public int Id { get; set; }
        [Required]
        public ApplicationUser Owner { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; } 
        [Required]
        public decimal Price { get; set; }
        [Required]
        public TimeSpan Duration { get; set; }
    }
}