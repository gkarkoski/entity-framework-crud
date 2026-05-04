using Microsoft.EntityFrameworkCore;
using PdiCrud.Data;

namespace PdiCrud.Models.Request;

public record CustomerRequest(
    string? Name,
    List<AddressRequest>? Addresses
);