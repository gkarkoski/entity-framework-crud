using Microsoft.EntityFrameworkCore;
using PdiCrud.Data;
using PdiCrud.Data.Entities;
using PdiCrud.Models;
using PdiCrud.Models.Request;

namespace PdiCrud.Handlers;

public static class CustomerHandler
{
    
    public static Task<IResult> GetCustomers(CustomerContext ctx)
    {
        var customers = ctx.Customer
            .Include(c => c.Addresses)
            .OrderBy(n => n.Name)
            .Select(x => new CustomerResponse(
                x.Id,
                x.Name,
                x.Addresses.Select(a => new AddressResponse(
                        a.Id,
                        a.Street,
                        a.Number,
                        a.City,
                        a.State,
                        a.ZipCode))
                    .ToList()));

        return Task.FromResult(Results.Ok(new { succes = true, data = customers }));
    }
}