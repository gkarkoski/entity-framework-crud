namespace PdiCrud.Data.Entities;

public class AddressModel()
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }

    public Guid CustomerId { get; init; }
    public CustomerModel Customer { get; set; } = null!;
}