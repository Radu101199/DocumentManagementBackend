using DocumentManagementBackend.Application.Common.Exceptions;
using DocumentManagementBackend.Application.Common.Interfaces;
using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.Interfaces;
using DocumentManagementBackend.Domain.ValueObjects;
using MediatR;

namespace DocumentManagementBackend.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken)
                   ?? throw new NotFoundException("User", request.Email);

        if (!user.CanLogin())
            throw new DomainException("Account is locked or suspended");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResult(
            Token: token,
            Email: user.Email.Value,
            FullName: user.FullName,
            Roles: user.Roles.Select(r => r.ToString()));
    }
}