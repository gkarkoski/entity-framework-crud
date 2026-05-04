using PdiCrud.Models;

namespace PdiCrud.Data.Entities;

public class CustomerModel
{
    protected CustomerModel()
    {
    }

    public CustomerModel(string? name, List<AddressModel> address)
    {
        Name = name;
        Addresses = address.ToList() ?? new();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<AddressModel> Addresses { get; set; } = new List<AddressModel>();
}