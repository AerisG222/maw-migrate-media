namespace MawDbMigrate.Models.Target;

public class Rating
{
    public Guid MediaId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public short Score { get; set; }
}