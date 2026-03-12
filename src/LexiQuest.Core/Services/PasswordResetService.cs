using System.Security.Cryptography;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IStringLocalizer<PasswordResetService> _localizer;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPasswordHasher<User> passwordHasher,
        IStringLocalizer<PasswordResetService> localizer)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _localizer = localizer;
    }

    public async Task<Result> RequestResetAsync(RequestPasswordResetDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        // Vždy vracíme success, abychom neodhalili existenci emailu
        if (user == null)
        {
            return Result.Success();
        }

        // Generovat secure random token
        var token = GenerateSecureToken();
        
        // Vytvořit token entitu (platnost 1 hodina)
        var resetToken = PasswordResetToken.Create(user.Id, token, TimeSpan.FromHours(1));
        
        await _tokenRepository.AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Poslat email
        await _emailService.SendPasswordResetEmailAsync(request.Email, token, cancellationToken);
        
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenRepository.GetByTokenAsync(request.Token, cancellationToken);
        
        if (token == null)
        {
            return Result.Failure(new Error("Token.Invalid", _localizer["Error.InvalidToken"]));
        }

        if (token.IsExpired)
        {
            return Result.Failure(new Error("Token.Expired", _localizer["Error.ExpiredToken"]));
        }

        if (token.IsUsed)
        {
            return Result.Failure(new Error("Token.Used", _localizer["Error.UsedToken"]));
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure(new Error("Token.Invalid", _localizer["Error.InvalidToken"]));
        }

        // Kontrola zda nové heslo není stejné jako staré
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.NewPassword);
        if (verificationResult == PasswordVerificationResult.Success)
        {
            return Result.Failure(new Error("Password.SameAsOld", _localizer["Error.SamePassword"]));
        }

        // Hash nového hesla
        var newPasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.SetPasswordHash(newPasswordHash);
        
        // Označit token jako použitý
        token.MarkAsUsed();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }

    private static string GenerateSecureToken()
    {
        // Generovat 32 bajtů (64 hex znaků)
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLower();
    }
}
