using DTEK_API.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OutageCheckerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/search", async (string city, string street, string houseNumber, OutageCheckerService service) =>
    {
        if (string.IsNullOrWhiteSpace(city))
            return Results.BadRequest(new { Error = "Required parameter 'city' is missing" });
        if (string.IsNullOrWhiteSpace(street))
            return Results.BadRequest(new { Error = "Required parameter 'street' is missing" });
    
        if (string.IsNullOrWhiteSpace(houseNumber))
            return Results.BadRequest(new { Error = "Required parameter 'houseNumber' is missing" });

        try
        {
            var result = await service.CheckOutages(city, street, houseNumber);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Error while checking outages"
            );
        }
    })
    .WithName("GetOutage")
    .WithOpenApi();
    

app.Run();