namespace ChatAppServer.Api.Dtos;

public record SendMessageDto(
    Guid UserId,
    Guid ToUserId,
    string Message
);
