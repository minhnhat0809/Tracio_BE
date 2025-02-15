using Azure;
using Azure.AI.ContentSafety;
using ContentService.Application.DTOs;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ContentService.Application.Services;

public class ModerationService : IModerationService
{
    private readonly ContentSafetyClient _client;

    public ModerationService(IOptions<ContentSafetySettings> settings)
    {
        var config = settings.Value;
        _client = new ContentSafetyClient(new Uri(config.Endpoint), new AzureKeyCredential(config.ApiKey));
    }
    
    public async Task<ContentModerationResponse> ProcessModerationResult(string text)
    {
        var request = new AnalyzeTextOptions(text);
        var result = await _client.AnalyzeTextAsync(request);
        
        var response = new ContentModerationResponse();

        if (!result.HasValue)
        {
            return response;
        }

        // Iterate through each category and check risk score
        foreach (var category in result.Value.CategoriesAnalysis)
        {
            if (category.Severity >= 1.5) // Set threshold for harmful content
            {
                response.FlaggedCategories.Add(category.Category.ToString());
            }
        }

        response.IsSafe = response.FlaggedCategories.Count == 0;
        return response;
    }

}