using FluentValidation;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Services;

public class RegisterValidator : AbstractValidator<RegisterVM>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty();
    }
}
