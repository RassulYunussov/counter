using System;
using Microsoft.AspNetCore.Identity;

namespace counter.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public IdentityUser Operator { get; set; }
        public BusinessPoint BusinessPoint { get; set; }
        public DateTime OperationDate { get; set; }
        public decimal Amount { get; set; }
    }
}