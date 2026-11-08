using System.Net;
using System.Text;
using System.Text.Json;
using Polly;
using PurchaseApp.Data;
using PurchaseApp.Models;

namespace PurchaseApp.Services;

public sealed class PurchaseRequestApiService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public PurchaseRequestApiService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task ProcessAsync(int requestId)
    {
        var request = await _context.PurchaseRequests.FindAsync(requestId);
        if (request == null) return;

        var jitterer = new Random();
        var retryPolicy = Policy
            .Handle<Exception>()
            .OrResult<HttpResponseMessage>(response =>
                response.StatusCode == HttpStatusCode.RequestTimeout
                || (int)response.StatusCode == 429
                || (int)response.StatusCode >= 500)
            .WaitAndRetryAsync(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                async (outcome, timeSpan, retryCount, context) =>
                {
                    double waitSeconds = Math.Round(timeSpan.TotalSeconds, 1);
                    var failure = outcome.Exception == null
                        ? $"API returned HTTP {(int)outcome.Result.StatusCode}"
                        : "API is unavailable";
                    request.ApiStatusMessage = $"{failure}. Retrying ({retryCount}/5) in {waitSeconds}s...";

                    _context.AuditLogs.Add(new AuditLog
                    {
                        PurchaseRequestId = requestId,
                        Action = $"API_Retry_{retryCount}",
                        PerformedBy = "System",
                        Details = $"API call failed. Retrying in {waitSeconds}s... (Attempt {retryCount}/5)",
                        Timestamp = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();

                    if (outcome.Exception == null) outcome.Result.Dispose();
                });

        try
        {
            var payload = new
            {
                Title = request.Title,
                Amount = request.Amount,
                Department = request.Department,
                ProposedSupplier = request.ProposedSupplier,
                ApplicantName = request.ApplicantName
            };

            using var response = await retryPolicy.ExecuteAsync(async () =>
            {
                using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync("api/purchase-requests", content);
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                request.Status = "Failed";
                request.ApiStatusMessage = $"API returned HTTP {(int)response.StatusCode}.";
                _context.AuditLogs.Add(new AuditLog
                {
                    PurchaseRequestId = request.Id,
                    Action = "API_Error",
                    PerformedBy = "External API",
                    Details = $"API request failed with status code {(int)response.StatusCode}: {errorContent}",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("purchaseOrderNumber", out var orderNumber))
            {
                request.PurchaseOrderNumber = orderNumber.GetString();
            }
            else if (document.RootElement.TryGetProperty("id", out var id))
            {
                request.PurchaseOrderNumber = id.ToString();
            }

            request.Status = "Approved";
            request.ApiStatusMessage = null;
            _context.AuditLogs.Add(new AuditLog
            {
                PurchaseRequestId = request.Id,
                Action = "API_Success",
                PerformedBy = "External API",
                Details = $"Order number received successfully: {request.PurchaseOrderNumber}",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            request.Status = "Failed";
            request.ApiStatusMessage = $"API call failed: {exception.Message}";
            _context.AuditLogs.Add(new AuditLog
            {
                PurchaseRequestId = request.Id,
                Action = "API_Error",
                PerformedBy = "System",
                Details = $"Exception after all retry attempts: {exception.Message}",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
