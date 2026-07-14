namespace ProductApi.Application.DTOs;

public class ErrorResponseDto
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? TraceId { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}
