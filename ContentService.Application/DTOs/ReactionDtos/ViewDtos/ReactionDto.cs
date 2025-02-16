﻿using ContentService.Domain.Enums;

namespace ContentService.Application.DTOs.ReactionDtos.ViewDtos;

public class ReactionDto
{
    public int ReactionId { get; set; }
    
    public int CyclistId { get; set; }
    
    public string CyclistName { get; set; } = null!;

    public string CyclistAvatar { get; set; } = null!;
    
    public DateTime? CreatedAt { get; set; }
}