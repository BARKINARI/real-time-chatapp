namespace ChatAppServer.Api.Dtos;

public record UserDto(
    string Name,
    IFormFile File
);
