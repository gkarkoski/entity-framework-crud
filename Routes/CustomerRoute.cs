using Microsoft.EntityFrameworkCore;
using PdiCrud.Data;
using PdiCrud.Data.Entities;
using PdiCrud.Models;
using PdiCrud.Models.Request;
using PdiCrud.Handlers;

namespace PdiCrud.Routes;

public static class CustomerRoute
{
    public static void MapCustomerRoute(this WebApplication app)
    {
        // Create a new Customer
        app.MapPost("/customers",CustomerHandler.CreateCustomers);
        
        // Retrieve a list of all users in dB
        app.MapGet("/customers", CustomerHandler.GetCustomers);
        
        // Retrieve one customer by Id
        app.MapGet("/customers/{id:guid}", CustomerHandler.GetCustomersById);
        
        // Delete one customer by Id
        app.MapDelete("/customers/{id:guid}", CustomerHandler.DeleteCustomer);
        
        // Update a customer data by Id 
        app.MapPatch("/customers/{id:guid}", CustomerHandler.UpdateCustomer);
        
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

    