using System;
using System.ComponentModel.DataAnnotations;

namespace counter.ViewModels
{
    public class Operator {
        public string Id { get; set; }
        [Required]
        public string OperatorName { get; set; }
        [Required]
        public string OperatorPassword {get;set; }
    }
}