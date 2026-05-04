namespace PdiCrud.Models.Request;

public record CustomerPatchRequest(string? Name, List<AddressPatchRequest>? Addresses);

public record AddressPatchRequest(
    Guid? Id,
    string? Street,
    string? Number,
    string? City,
    string? State,
    string? ZipCode);