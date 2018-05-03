namespace counter.Models
{
    public interface IBusinessObject
    {
        ApplicationUser Owner { get; set; }
    }
}