using Microsoft.EntityFrameworkCore;
using VectisBackend.Data;
using VectisBackend.Middleware;
using VectisBackend.Parsers;
using VectisBackend.Services;
using VectisBackend.Services.Impl;
using VectisBackend.Services.ImplMappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<VectisDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VECTIS"), sql =>
        {
            sql.UseCompatibilityLevel(100);
            sql.CommandTimeout(120);
        }));

builder.Services.AddScoped<IExcelSheetParser, FicheGeneraleParser>();
builder.Services.AddScoped<IExcelSheetParser, BordereauxParser>();
builder.Services.AddScoped<IExcelSheetParser, OrdreServiceParser>();
builder.Services.AddScoped<IExcelSheetParser, EngagementParser>();
builder.Services.AddScoped<IExcelSheetParser, LivraisonParser>();
builder.Services.AddScoped<IExcelSheetParser, ReceptionParser>();
builder.Services.AddScoped<IExcelSheetParser, FactureParser>();

builder.Services.AddScoped<IFicheGeneraleMapperService, FicheGeneraleMapperService>();
builder.Services.AddScoped<IBorderauxMapperService, BorderauxMapperService>();
builder.Services.AddScoped<IOrdreServiceMapperService, OrdreServiceMapperService>();
builder.Services.AddScoped<IEngagementMapperService, EngagementMapperService>();
builder.Services.AddScoped<ILivraisonMapperService, LivraisonMapperService>();
builder.Services.AddScoped<IReceptionMapperService, ReceptionMapperService>();
builder.Services.AddScoped<IFactureMapperService, FactureMapperService>();

builder.Services.AddScoped<IImportDepenseService, ImportDepenseService>();
builder.Services.AddScoped<IReferentielService, ReferentielService>();

builder.Services.AddScoped<IExportTemplateService, ExportTemplateService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

app.UseGlobalJsonExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularDev");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
