namespace AestheticStudySpace.Application.DTOs.Report;

/// <summary>Request payload for creating a new report.</summary>
public record CreateReportRequestDto(
    string Title,
    string Content,
    string Type,
    string? AttachmentUrl = null,
    string? AttachmentBase64 = null
);

/// <summary>Response DTO after a report is submitted.</summary>
public record ReportResponseDto(
    Guid Id,
    string Title,
    string Content,
    string Type,
    string? AttachmentUrl,
    string Status,
    DateTime CreatedAt
);
