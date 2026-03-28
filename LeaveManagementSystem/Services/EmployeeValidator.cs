using FluentValidation;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Services;

public class EmployeeValidator : AbstractValidator<EmployeeVM>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
