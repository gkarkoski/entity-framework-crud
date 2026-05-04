using PdiCrud.Data;
using PdiCrud.Routes;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=customer.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowed = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ?? "")
    .Split(';', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(c =>
{
    c.AddPolicy("Frontend", p =>
        p.WithOrigins(allowed)
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.UseHttpsRedirection();
app.UseCors("Frontend");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/healthz", () => Results.Ok("ok"));


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    context.Database.Migrate();
}

app.MapCustomerRoute();
app.Run();