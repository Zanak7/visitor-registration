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

    [Function("checkin")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "checkin")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        CheckInRequest data;
        try
        {
            data = JsonSerializer.Deserialize<CheckInRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid JSON.");
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid JSON.");
            return bad;
        }

        if (data == null || string.IsNullOrWhiteSpace(data.Name))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Name is required.");
            return bad;
        }

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

            var cmd = new SqlCommand(@"INSERT INTO dbo.VisitorLog (Name, Email, TimestampUtc) VALUES (@name, @email, SYSUTCDATETIME());", conn);
            cmd.Parameters.AddWithValue("@name", data.Name);
            cmd.Parameters.AddWithValue("@email", (object?)data.Email ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("Visitor '{Name}' checked in.", data.Name);

            var ok = req.CreateResponse(HttpStatusCode.Created);
            ok.Headers.Add("Content-Type", "application/json");
            await ok.WriteStringAsync(JsonSerializer.Serialize(new { ok = true }));
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DB insert failed.");
            var srv = req.CreateResponse(HttpStatusCode.InternalServerError);
            await srv.WriteStringAsync("Failed to save.");
            return srv;
        }
    }

    private record CheckInRequest(string Name, string? Email);
}
