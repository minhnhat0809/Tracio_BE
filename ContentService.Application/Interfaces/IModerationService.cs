using Azure.AI.ContentSafety;
using ContentService.Application.DTOs;

namespace ContentService.Application.Interfaces;

public interface IModerationService
{
    Task<ContentModerationResponse> ProcessModerationResult(string text);
}