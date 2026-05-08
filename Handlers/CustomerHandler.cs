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

    public static Task<IResult> CreateCustomers(CustomerContext ctx, CustomerRequest request)
    {
        var addresses = (request.Addresses ?? [])
            .Select(x => new AddressModel
            {
                Street = x.Street,
                Number = x.Number,
                City = x.City,
                State = x.State,
                ZipCode = x.ZipCode
            })
            .ToList();

        var customer = new CustomerModel(request.Name, addresses);
        ctx.Customer.Add(customer);
        ctx.SaveChangesAsync();

        var response = new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Addresses.Select(a => new AddressResponse(
                a.Id,
                a.Street,
                a.Number,
                a.City,
                a.State,
                a.ZipCode)).ToList()
        );
        return Task.FromResult(Results.Created(request.Name, response));
        
        
    }

    public static Task<IResult> GetCustomersById(Guid id, CustomerContext ctx)
    {
        var customer = ctx.Customer
            .Include(c => c.Addresses)
            .SingleOrDefault(c => c.Id == id);
        
        if  (customer == null) return Task.FromResult(Results.NotFound());

        var response = new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Addresses
                .Select(a => new AddressResponse(
                    a.Id,
                    a.Street,
                    a.Number,
                    a.City,
                    a.State,
                    a.ZipCode)).ToList()
        );
        
        return Task.FromResult(Results.Ok(response));
    }

    
}