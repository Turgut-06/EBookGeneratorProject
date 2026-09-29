using Application.Common.Interfaces;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using System.IO;
using System.Net.Http;
using Persistence.Context;
using System.Threading.RateLimiting;

string fontUrl = "https://raw.githubusercontent.com/google/fonts/main/ofl/roboto/Roboto-Regular.ttf";

try
{
    using var httpClient = new HttpClient();
    // Fontu internet üzerinden byte dizisi olarak indiriyoruz
    byte[] fontBytes = await httpClient.GetByteArrayAsync(fontUrl);

    using var fontStream = new MemoryStream(fontBytes);
    // Bellekteki fontu QuestPDF FontManager'a yüklüyoruz
    FontManager.RegisterFont(fontStream);
}
catch (Exception ex)
{
    Console.WriteLine($"Font internetten yüklenirken hata oluştu: {ex.Message}");
}


var builder = WebApplication.CreateBuilder(args);

// Rate Limiting Configuration
builder.Services.AddRateLimiter(options =>
{
    // Limite takılan istemcilere 429 Too Many Requests statüsü dön
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // 1. Politika: Fixed Window (Sabit Pencere)
    // Örnek: Aynı IP adresinden 1 dakika içinde en fazla 5 e-kitap oluşturma isteği atılabilsin.
    options.AddPolicy("EBookGeneratePolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // İstemci IP adresine göre ayırt et
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,                  // 1 dakikada izin verilen istek sayısı
                QueueLimit = 0,                   // Kuyrukta bekletme yapılmasın
                Window = TimeSpan.FromMinutes(1)  // Zaman penceresi
            }));

    // Opsiyonel: Limite takılan kullanıcıya detaylı JSON mesajı dönme
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\": \"Çok fazla e-kitap dönüştürme isteğinde bulundunuz. Lütfen 1 dakika sonra tekrar deneyin.\"}",
            cancellationToken: token);
    };
});

// DbContext Kayd?
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// MediatR Kayd?
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly));

// Servis Kay?tlar?
builder.Services.AddScoped<IContactCleanerService, ContactCleanerService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// React CORS Politikas?
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Book Generator API V1");
        c.RoutePrefix = "swagger"; // http://localhost:5000/swagger adresinden erişim sağlar
    });
}

app.UseHttpsRedirection();

// wwwroot alt?ndaki statik dosyalar? (uploads/pdfs, uploads/docx) sunma
app.UseStaticFiles();

var webRootPath = app.Environment.WebRootPath
    ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");

// Uploads klasörlerini otomatik olu?turma
var docxPath = Path.Combine(webRootPath, "uploads", "docx");

var pdfsPath = Path.Combine(webRootPath, "uploads", "pdfs");

if (!Directory.Exists(docxPath)) Directory.CreateDirectory(docxPath);

if (!Directory.Exists(pdfsPath)) Directory.CreateDirectory(pdfsPath);

app.UseCors("AllowReactApp");
app.UseRateLimiter(); 
app.UseAuthorization();

//app.MapControllers();

try
{
    app.MapControllers();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    foreach (var loaderEx in ex.LoaderExceptions)
    {
        // Visual Studio Output penceresine detaylı hatayı basar
        System.Diagnostics.Debug.WriteLine($"---> EKSİK KÜTÜPHANE DETAYI: {loaderEx?.Message}");
        Console.WriteLine($"---> EKSİK KÜTÜPHANE DETAYI: {loaderEx?.Message}");
    }
    throw;
}
app.Run();