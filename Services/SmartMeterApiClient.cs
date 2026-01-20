using System.Text;
using System.Text.Json;
using SolarMicronet.Web.Models;

namespace SolarMicronet.Web.Services;

public interface ISmartMeterApiClient
{
    Task<SignatureApiResponse> GenerateSignatureAsync(string participant, string amount);
    Task<SignatureApiResponse> ConsumeSignatureAsync(string participant, string amount);
    Task<SignatureApiResponse> CustomSignatureAsync(string participant, string amount, int operationType);
    Task<string> GetMeterAddressAsync();
    Task<bool> HealthCheckAsync();
}

public class SmartMeterApiClient : ISmartMeterApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SmartMeterApiClient> _logger;

    public SmartMeterApiClient(HttpClient httpClient, ILogger<SmartMeterApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(BlockchainConfig.SMART_METER_API_BASE);
    }

    public async Task<SignatureApiResponse> GenerateSignatureAsync(string participant, string amount)
    {
        try
        {
            var request = new { Participant = participant, Amount = amount };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/signature/generate", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SignatureApiResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new Exception("Failed to deserialize signature response");
            }

            _logger.LogInformation("Generate signature obtained for {Participant}, amount {Amount}, nonce {Nonce}",
                participant, amount, result.Data.Nonce);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SmartMeter API /signature/generate");
            throw;
        }
    }

    public async Task<SignatureApiResponse> ConsumeSignatureAsync(string participant, string amount)
    {
        try
        {
            var request = new { Participant = participant, Amount = amount };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/signature/consume", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SignatureApiResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new Exception("Failed to deserialize signature response");
            }

            _logger.LogInformation("Consume signature obtained for {Participant}, amount {Amount}, nonce {Nonce}",
                participant, amount, result.Data.Nonce);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SmartMeter API /signature/consume");
            throw;
        }
    }

    public async Task<SignatureApiResponse> CustomSignatureAsync(string participant, string amount, int operationType)
    {
        try
        {
            var request = new { Participant = participant, Amount = amount, OperationType = operationType };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/signature/custom", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SignatureApiResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new Exception("Failed to deserialize signature response");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SmartMeter API /signature/custom");
            throw;
        }
    }

    public async Task<string> GetMeterAddressAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/meter/address");
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MeterAddressResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Address ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling SmartMeter API /meter/address");
            throw;
        }
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SmartMeter API health check failed");
            return false;
        }
    }

    private class MeterAddressResponse
    {
        public string Address { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
