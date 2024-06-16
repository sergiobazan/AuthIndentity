namespace Contracts;

public sealed record AuthenticationResponse(
    Guid UserId,
    string Token);


