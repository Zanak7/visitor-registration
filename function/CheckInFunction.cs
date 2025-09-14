using System.Net;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;

public class CheckInFunction
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public CheckInFunction(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<CheckInFunction>();
        _config = config;
    }
    // Funktion som körs när någon skickar ett POST-anrop till /api/checkin
    [Function("checkin")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "checkin")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        CheckInRequest data;
        try
        {
            // Försöker läsa in JSON från anropet
            data = JsonSerializer.Deserialize<CheckInRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid JSON.");
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid JSON.");
            return bad;
        }

        if (data == null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid JSON.");
            return bad;
        }

        // --- NEW: normalize + immediate input logs (with "empty") ---
        var name = (data.Name ?? string.Empty).Trim();
        var email = string.IsNullOrWhiteSpace(data.Email) ? null : data.Email!.Trim();

        _logger.LogInformation("Input received name is '{Name}'.", string.IsNullOrEmpty(name) ? "empty" : name);
        _logger.LogInformation("Input received email is '{Email}'.", string.IsNullOrEmpty(email) ? "empty" : email);
        // ------------------------------------------------------------

        // Kollar så att det faktiskt finns ett namn
        if (string.IsNullOrWhiteSpace(name))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            _logger.LogWarning("Name is missing."); // 
            await bad.WriteStringAsync("Name is required.");
            return bad;
        }
        // Hämtar anslutningssträngen till databasen från inställningarna
        var connStr = _config["SqlConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
        {
            _logger.LogError("SqlConnectionString missing.");
            var srv = req.CreateResponse(HttpStatusCode.InternalServerError);
            await srv.WriteStringAsync("Server configuration error.");
            return srv;
        }

        try
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            // Skriver in besökaren i databasen
            var cmd = new SqlCommand(@"INSERT INTO dbo.VisitorLog (Name, Email, TimestampUtc) VALUES (@name, @email, SYSUTCDATETIME());", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@email", (object?)email ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("Visitor '{Name}' checked in.", name);
            _logger.LogInformation("Input received email is : '{Email}'.", email);

            // Svarar med 201 Created om allt gick bra
            var ok = req.CreateResponse(HttpStatusCode.Created);
            ok.Headers.Add("Content-Type", "application/json");
            await ok.WriteStringAsync(JsonSerializer.Serialize(new { ok = true }));
            return ok;
        }
        catch (Exception ex)
        {   
            // Om något går fel när vi skriver till databasen
            _logger.LogError(ex, "DB insert failed.");
            var srv = req.CreateResponse(HttpStatusCode.InternalServerError);
            await srv.WriteStringAsync("Failed to save.");
            return srv;
        }
    }

    // Enkel modell för att ta emot namn och (valfri) e-postadress
    private record CheckInRequest(string Name, string? Email);
}
