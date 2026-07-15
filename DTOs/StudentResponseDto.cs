namespace TmsApi.DTOs;

public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    decimal GPA,
    int Age,
    bool IsActive
);