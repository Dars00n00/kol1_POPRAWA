//w nuget biblioteki:
//Microsoft.Data.SqlClient
//Swashbuckle.AspNetCore.SwaggerGen
//Swashbuckle.AspNetCore.SwaggerUI


using kol1.Services;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.json", 
    optional: false, 
    reloadOnChange: true
);

builder.Services.AddScoped<IDeliveriesService, DeliveriesService>();

builder.Services.AddAuthorization(); 
builder.Services.AddControllers(); 

builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new()
    {
        Title = "kol1_apbd_przykladowe", 
        Version = "v1",
        Description = "kol1_apbd",
        Contact = new()
        {
            Name = "Dariusz",
            Email = "xxxxx@gmail.com",
            Url = new Uri("https://github.com/Dars00n00")
        },
        // License = new()
        // {
        //     
        // }
    });
});

var app = builder.Build(); 

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "przykladowe_kol1_apbd");
    c.DocExpansion(DocExpansion.List);
    c.DefaultModelExpandDepth(0);
    c.DisplayRequestDuration();
    c.EnableFilter();
});


app.UseAuthorization();
app.MapControllers(); 

app.Run();



// aby pobrać conn str należy użyć Iconfiguration
// dot net automatycznie wstrzykuje zależność
// IConfiguration configuration;
// var connStr = configuration.GetConnectionString("nazwa");
