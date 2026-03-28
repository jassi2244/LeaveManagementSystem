using FluentValidation;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Services;

public class ApplyLeaveValidator : AbstractValidator<ApplyLeaveVM>
{
    public ApplyLeaveValidator()
    {
        RuleFor(x => x.LeaveTypeId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x)
            .Must(x => !x.IsHalfDay || x.StartDate.Date == x.EndDate.Date)
            .WithMessage("Half-day leave can be applied only for a single date.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
