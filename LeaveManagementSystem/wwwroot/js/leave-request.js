$(function () {
    const $startDate = $("#StartDate, #startDate");
    const $endDate = $("#EndDate, #endDate");

    const refreshDays = function () {
        const startDate = $startDate.first().val();
        const endDate = $endDate.first().val();
        if (!startDate || !endDate) return;
        $.get("/LeaveRequest/CalculateWorkingDays", { startDate: startDate, endDate: endDate }, function (res) {
            $("#workingDays").text(res.workingDays);
            $("#workingDaysCount").text(res.workingDays);
        });
    };

    $startDate.add($endDate).on("change input", refreshDays);
    refreshDays();

    $("#resetApplyForm").on("click", function () {
        const form = $("#leaveApplyForm")[0];
        if (!form) return;

        form.reset();
        $("#workingDays").text("0");
        $("#workingDaysCount").text("0");

        const summary = $("#leaveApplyForm").find(".validation-summary-errors");
        summary.removeClass("validation-summary-errors").addClass("validation-summary-valid");
        summary.find("ul").empty();
    });

    $("#leaveApplyForm").on("submit", function (e) {
        e.preventDefault();
        const form = this;
        Swal.fire({
            title: "Submit leave request?",
            icon: "question",
            showCancelButton: true
        }).then((result) => {
            if (result.isConfirmed) form.submit();
        });
    });

    $("#isHalfDayLeave").on("change", function () {
        if (!this.checked) return;
        const start = $startDate.first().val();
        if (start) {
            $endDate.val(start);
            refreshDays();
        }
    });
});
