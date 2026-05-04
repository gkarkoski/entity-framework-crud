using Microsoft.EntityFrameworkCore;
using PdiCrud.Data;
using PdiCrud.Data.Entities;
using PdiCrud.Models;
using PdiCrud.Models.Request;

namespace PdiCrud.Routes;

public static class CustomerRoute
{
    public static void MapCustomerRoute(this WebApplication app)
    {
        // Create a new Customer
        app.MapPost("/customers",
            async (CustomerRequest request, CustomerContext context) =>
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
                context.Customer.Add(customer);
                await context.SaveChangesAsync();

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
                return Results.Created($"customers/{customer.Id}", response);
            });

        // Retrieve a list of all users in dB
        app.MapGet("/customers",
            async (CustomerContext context) =>
            {
                var clients = await context.Customer
                    .Include(c => c.Addresses)
                    .OrderBy(a => a.Name)
                    .Select(c => new CustomerResponse(
                        c.Id,
                        c.Name,
                        c.Addresses.Select(a => new AddressResponse(
                            a.Id,
                            a.Street,
                            a.Number,
                            a.City,
                            a.State,
                            a.ZipCode)).ToList()
                    )).ToListAsync();
                return Results.Ok(new { succes = true, data = clients });
            });

        // Retrieve one customer by Id
        app.MapGet("/customers/{id:guid}", async (Guid id, CustomerContext context) =>
        {
            var customer = await context.Customer.Include(c => c.Addresses)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (customer is null) return Results.NotFound($"Customer Id:{id}, not found.");

            var response = new CustomerResponse(
                customer.Id,
                customer.Name, customer.Addresses.Select(x =>
                        new AddressResponse(
                            x.Id,
                            x.Street,
                            x.Number,
                            x.City,
                            x.State,
                            x.ZipCode))
                    .ToList());
            return Results.Ok(new { succes = true, data = response });
        });


        // Delete one customer by Id
        app.MapDelete("/customers/{id:guid}",
            async (Guid id, CustomerContext context) =>
            {
                var a = await context.Customer.FindAsync(id);
                if (a is null) return Results.NotFound($"Client: {id}, not found.");
                context.Customer.Remove(a);
                await context.SaveChangesAsync();
                return Results.Ok($"Customer Id:{id} has been deleted.");
            });

        // Update a customer data by Id 
        app.MapPatch("/customers/{id:guid}",
            async (Guid id, CustomerPatchRequest patchRequest, CustomerContext context) =>
            {
                var customer = await context.Customer
                    .Include(c => c.Addresses)
                    .AsTracking()
                    .SingleOrDefaultAsync(c => c.Id == id);

                if (customer is null) return Results.NotFound($"Customer id:{id}, not found.");

                if (patchRequest.Name is not null) customer.Name = patchRequest.Name;

                if (patchRequest.Addresses is not null)
                {
                    var existsById = customer.Addresses.ToDictionary(a => a.Id);

                    foreach (var address in patchRequest.Addresses)
                    {
                        if (address.Id is null || address.Id == Guid.Empty)
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

                        if (existsById.TryGetValue(address.Id.Value, out var addressUpdate))
                        {
                            if (address.Street is not null) addressUpdate.Street = address.Street;
                            if (address.Number is not null) addressUpdate.Number = address.Number;
                            if (address.City is not null) addressUpdate.City = address.City;
                            if (address.State is not null) addressUpdate.State = address.State;
                            if (address.ZipCode is not null) addressUpdate.ZipCode = address.ZipCode;
                        }
                        else
                        {
                            return Results.NotFound($"Address {address.Id.Value} not found");
                        }
                    }
                }
                
                // Used this try/catch exception because had some issues with DbUpdateConcurrency
                // Kept in code, the problem was just a wrong table in PATCH context. 
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    var entries = context.ChangeTracker.Entries()
                        .Select(e => new
                        {
                            Entity = e.Entity.GetType().Name,
                            State = e.State.ToString(),
                            Keys = e.Properties.Where(p => p.Metadata.IsPrimaryKey())
                                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
                        }).ToList();

                    return Results.Problem(
                        detail: System.Text.Json.JsonSerializer.Serialize(entries),
                        statusCode: 409,
                        title: "Concurrency_Error.");
                }

                return Results.Ok($"Customer Id:{customer.Id}, updated successfully.");
            });
        
        // Delete a customer address by AddressId, used in edit modal
        app.MapDelete("/addresses/{addressId:guid}",
            async (Guid addressId, CustomerContext context) =>
            {
                var address = await context.Address.SingleOrDefaultAsync(a => a.Id == addressId);

                if (address is null)
                    return
                        Results.NotFound($"Address: {addressId}, not found.");

                context.Address.Remove(address);
                await context.SaveChangesAsync();
                return Results.Ok($"Address: {addressId} deleted successfully.");
            });
    }
}