$(function () {
    const uiAnnouncements = $("#uiAnnouncements");
    const announce = function (message) {
        if (!uiAnnouncements.length || !message) return;
        uiAnnouncements.text("");
        setTimeout(function () { uiAnnouncements.text(message); }, 30);
    };

    const antiForgeryToken = $('meta[name="request-verification-token"]').attr("content");
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            const method = (settings.type || "").toUpperCase();
            if (antiForgeryToken && (method === "POST" || method === "PUT" || method === "DELETE" || method === "PATCH")) {
                xhr.setRequestHeader("RequestVerificationToken", antiForgeryToken);
            }
        }
    });

    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: "4000"
    };

    if ($(".data-table").length) {
        $(".data-table").DataTable();
    }

    if ($('[data-toggle="tooltip"]').length) {
        $('[data-toggle="tooltip"]').tooltip();
    }

    $(document).ajaxStart(function () {
        $("#globalLoader").show();
        $("body").attr("aria-busy", "true");
        announce("Loading");
    }).ajaxStop(function () {
        $("#globalLoader").hide();
        $("body").attr("aria-busy", "false");
    });

    const observeToasts = function () {
        const toastContainer = document.querySelector("#toast-container");
        if (!toastContainer || !window.MutationObserver) return;
        const observer = new MutationObserver(function () {
            const lastToast = toastContainer.querySelector(".toast:last-child .toast-message");
            if (lastToast) announce(lastToast.textContent.trim());
        });
        observer.observe(toastContainer, { childList: true, subtree: true });
    };
    observeToasts();

    $(document).on("click", ".reject-btn", function () {
        const id = $(this).data("id");
        Swal.fire({
            title: "Reject this leave?",
            input: "text",
            inputLabel: "Comments",
            showCancelButton: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.post("/LeaveRequest/Reject", { id: id, comments: result.value || "Rejected" }, function (res) {
                    if (res.success) {
                        toastr.success(res.message);
                        location.reload();
                    } else {
                        toastr.error(res.message);
                    }
                });
            }
        });
    });

    $(document).on("click", ".cancel-btn", function () {
        const id = $(this).data("id");
        Swal.fire({ title: "Cancel request?", showCancelButton: true }).then((r) => {
            if (r.isConfirmed) {
                $.post("/LeaveRequest/Cancel", { id: id }, function (res) {
                    if (res.success) {
                        toastr.success(res.message);
                        location.reload();
                    } else {
                        toastr.error(res.message);
                    }
                });
            }
        });
    });

    $(document).on("submit", ".swal-confirm-form", function (e) {
        e.preventDefault();
        const form = this;
        const title = $(form).data("title") || "Are you sure?";
        const text = $(form).data("text") || "";

        Swal.fire({
            title: title,
            text: text,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, continue",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });
});
