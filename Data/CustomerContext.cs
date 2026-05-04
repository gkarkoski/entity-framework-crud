using Microsoft.EntityFrameworkCore;
using PdiCrud.Data.Entities;

namespace PdiCrud.Data;

public class CustomerContext(DbContextOptions<CustomerContext> options) : DbContext(options)
{
    public DbSet<CustomerModel> Customer { get; set; }
    public DbSet<AddressModel> Address { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseSqlite("Data Source=customer.db");
    //     base.OnConfiguring(optionsBuilder);
    // }
}