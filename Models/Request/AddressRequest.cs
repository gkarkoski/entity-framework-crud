namespace PdiCrud.Models.Request;

public abstract record AddressRequest(
    Guid Id,
    string? Street,
    string? Number,
    string? City,
    string? State,
    string? ZipCode
    );
