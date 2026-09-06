(() => {
    const canvas = document.getElementById('attendanceOverviewChart');
    if (!canvas || typeof Chart === 'undefined') {
        return;
    }

    let chartData;
    try {
        chartData = JSON.parse(canvas.dataset.chart || '[]');
    } catch {
        return;
    }

    new Chart(canvas, {
        type: 'bar',
        data: {
            labels: chartData.map(item => item.lectureDate),
            datasets: [
                {
                    label: 'Present',
                    data: chartData.map(item => item.presentCount),
                    backgroundColor: '#198754'
                },
                {
                    label: 'Absent',
                    data: chartData.map(item => item.absentCount),
                    backgroundColor: '#dc3545'
                },
                {
                    label: 'Late',
                    data: chartData.map(item => item.lateCount),
                    backgroundColor: '#f0ad00'
                },
                {
                    label: 'Excused',
                    data: chartData.map(item => item.excusedCount),
                    backgroundColor: '#009ADA'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    stacked: true
                },
                y: {
                    stacked: true,
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
})();
