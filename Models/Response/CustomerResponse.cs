namespace PdiCrud.Models;

public record CustomerResponse(
    Guid Id,
    string Name,
    List<AddressResponse> AddressResponse
);