namespace PdiCrud.Models;

public record AddressResponse(
    Guid Id,
    string? Street,
    string? Number,
    string? City,
    string? State,
    string? ZipCode
);