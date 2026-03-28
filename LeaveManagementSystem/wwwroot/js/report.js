$(function () {
    if ($(".data-table").length) {
        $(".data-table").DataTable();
    }

    const chartEl = document.getElementById("reportSummaryChart");
    if (!chartEl || typeof Chart === "undefined") return;

    const formParams = new URLSearchParams(window.location.search || "");
    const summaryUrl = "/Report/LeaveReportSummary?" + formParams.toString();

    fetch(summaryUrl)
        .then(r => r.ok ? r.json() : null)
        .then(summary => {
            if (!summary) return;
            new Chart(chartEl, {
                type: "doughnut",
                data: {
                    labels: ["Approved", "Pending", "Rejected", "Cancelled"],
                    datasets: [{
                        data: [summary.approved, summary.pending, summary.rejected, summary.cancelled],
                        backgroundColor: ["#28a745", "#ffc107", "#dc3545", "#6c757d"]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: "bottom" },
                        title: { display: true, text: `Total: ${summary.total}` }
                    }
                }
            });
        })
        .catch(() => { });
});
