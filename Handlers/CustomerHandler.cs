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

        if (customer == null) return Task.FromResult(Results.NotFound());

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

    public static Task<IResult> DeleteCustomer(Guid id, CustomerContext ctx)
    {
        var customer = ctx.Customer.Find(id);

        if (customer is null) return Task.FromResult(Results.NotFound());

        ctx.Customer.Remove(customer);
        ctx.SaveChangesAsync();

        return Task.FromResult(Results.Ok());
    }

    public static async Task<IResult> UpdateCustomer(Guid id, CustomerContext context, CustomerRequest patchRequest)
    {
        var customer = await context.Customer
            .Include(c => c.Addresses)
            .AsTracking()
            .SingleOrDefaultAsync(c => c.Id == id);

        if (customer is null) return Results.NotFound($"Customer id:{id}, not found.");

        if (!string.IsNullOrWhiteSpace(patchRequest.Name)) customer.Name = patchRequest.Name;

        if (patchRequest.Addresses is not null)
        {
            var existsById = customer.Addresses.ToDictionary(a => a.Id);

            foreach (var address in patchRequest.Addresses)
            {
                if (address.Id == Guid.Empty)
                {
                    var newAddress = new AddressModel
                    {
                        Id = Guid.NewGuid(),
                        Street = address.Street ?? "",
                        Number = address.Number ?? "",
                        City = address.City ?? "",
                        State = address.State ?? "",
                        ZipCode = address.ZipCode ?? "",
                        CustomerId = customer.Id
                    };
                    context.Address.Add(newAddress);
                    continue;
                }

                if (existsById.TryGetValue(address.Id, out var addressUpdate))
                {
                    if (string.IsNullOrWhiteSpace(address.Street)) addressUpdate.Street = address.Street;
                    if (string.IsNullOrWhiteSpace(address.Number)) addressUpdate.Number = address.Number;
                    if (string.IsNullOrWhiteSpace(address.City)) addressUpdate.City = address.City;
                    if (string.IsNullOrWhiteSpace(address.State)) addressUpdate.State = address.State;
                    if (string.IsNullOrWhiteSpace(address.ZipCode)) addressUpdate.ZipCode = address.ZipCode;
                }
                else
                {
                    return Results.NotFound($"Address {address.Id} not found");
                }
            }
        }
        
        await context.SaveChangesAsync();

        return Results.Ok($"Customer id:{id} updated successfully.");
    }
}