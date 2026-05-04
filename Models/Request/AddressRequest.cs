namespace PdiCrud.Models.Request;

public record AddressRequest(
    string? Street,
    string? Number,
    string? City,
    string? State,
    string? ZipCode
    );
