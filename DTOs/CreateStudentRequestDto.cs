namespace TmsApi.DTOs;

public record CreateStudentRequestDto(
    string RegistrationNumber,
    string Name,
    Decimal GPA,
    int Age
);