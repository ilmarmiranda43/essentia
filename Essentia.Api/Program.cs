var builder = WebApplication.CreateBuilder(args);

var githubOrigin = "https://ilmarmiranda43.github.io";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGithub", policy =>
    {
        policy
            .WithOrigins(githubOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowGithub");

var uploadRoot = Path.Combine(app.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadRoot))
{
    Directory.CreateDirectory(uploadRoot);
}

app.MapPost("/api/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Formulário inválido.");

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    if (file == null || file.Length == 0)
        return Results.BadRequest("Nenhum arquivo enviado.");

    var filePath = Path.Combine(uploadRoot, file.FileName);

    using (var stream = File.Create(filePath))
    {
        await file.CopyToAsync(stream);
    }

    return Results.Ok(new { detail = "Arquivo salvo com sucesso.", file = file.FileName });
});

app.MapGet("/api/summary", () =>
{
    var totalFiles = Directory.GetFiles(uploadRoot).Length;
    var totalPagar = 3500;
    var totalReceber = 4200;

    return Results.Ok(new
    {
        total_files = totalFiles,
        total_pagar = totalPagar,
        total_receber = totalReceber
    });
});

app.MapGet("/api/cashflow", () =>
{
    var result = new
    {
        labels = new[] { "Jan", "Fev", "Mar", "Abr" },
        pagar = new[] { 1000, 1200, 900, 1500 },
        receber = new[] { 1500, 1100, 1300, 1600 }
    };
    return Results.Ok(result);
});

app.MapGet("/api/files", () =>
{
    var files = Directory
        .GetFiles(uploadRoot)
        .Select(f => new
        {
            name = Path.GetFileName(f),
            size = new FileInfo(f).Length,
            url = $"/api/files/download/{Uri.EscapeDataString(Path.GetFileName(f))}"
        })
        .ToList();

    return Results.Ok(files);
});

app.MapGet("/api/files/download/{name}", (string name) =>
{
    var filePath = Path.Combine(uploadRoot, name);
    if (!File.Exists(filePath))
        return Results.NotFound();

    return Results.File(filePath, "application/octet-stream", name);
});

app.MapGet("/api/reports", (string? from, string? to) =>
{
    var report = new
    {
        from,
        to,
        total_registros = 12,
        itens = new[]
        {
            new { id = 1, nome = "Arquivo1.xlsx", data = "2025-11-01" },
            new { id = 2, nome = "Arquivo2.xlsx", data = "2025-11-02" }
        }
    };
    return Results.Ok(report);
});

app.Run();
