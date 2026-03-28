$(function () {
    const pieCtx = document.getElementById("leaveStatusPie");
    const barCtx = document.getElementById("monthlyLeaveBar");
    if (!pieCtx && !barCtx) return;

    fetch("/Dashboard/AdminChartData")
        .then(r => r.ok ? r.json() : Promise.reject("Unable to fetch chart data"))
        .then(data => {
            if (pieCtx) {
                new Chart(pieCtx, {
                    type: "pie",
                    data: {
                        labels: ["Pending", "Approved", "Rejected"],
                        datasets: [{
                            data: [data.pendingCount || 0, data.approvedCount || 0, data.rejectedCount || 0],
                            backgroundColor: ["#ffc107", "#28a745", "#dc3545"]
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false
                    }
                });
            }

            if (barCtx) {
                new Chart(barCtx, {
                    type: "bar",
                    data: {
                        labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                        datasets: [{
                            label: "Requests",
                            data: data.monthlyRequestCounts || Array(12).fill(0),
                            backgroundColor: "#0d6efd"
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false
                    }
                });
            }
        })
        .catch(() => {
            console.error("Dashboard chart data could not be loaded.");
        });
});
