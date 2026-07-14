namespace ProductApi.Domain.Entities;

public class Product : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}
