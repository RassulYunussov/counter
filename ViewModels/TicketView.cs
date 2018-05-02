using System;
using System.ComponentModel.DataAnnotations;

namespace counter.ViewModels
{
    public class TicketView
    {
        [Required]
        public int BusinessPointId { get; set; }
        [Required]
        public int Amount { get; set; }
    }
}