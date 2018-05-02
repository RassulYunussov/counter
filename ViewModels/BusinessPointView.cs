using System;
using System.ComponentModel.DataAnnotations;

namespace counter.ViewModels
{
    public class BusinessPointView
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; } 
        [Required]
        public int Price { get; set; }
        [Required]
        public int Duration { get; set; }
    }
}