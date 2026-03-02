using MediatR;

namespace DocumentManagementBackend.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(string Token, string Email, string FullName, IEnumerable<string> Roles);