using DinkToPdf.Contracts;
using DinkToPdf;
using DocumentGeneration.Endpoints;

//Microsoft.Playwright.Program.Main(["install"]);
//return;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConverter, SynchronizedConverter>(sp => new SynchronizedConverter(new PdfTools()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapQuestPdfGenerate();
app.MapDinkToPdfGenerate();
app.MapPdfSharpPdfGenerate();
app.MapPuppeteerPdfGenerate();
app.MapPlaywrightPdfGenerate();

app.Run();
