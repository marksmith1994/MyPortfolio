window.renderTotalTimeChart = function(names, minutes) {
    if (!window.Chart) return;
    var ctx = document.getElementById('totalTimeChart');
    if (!ctx) return;
    if (window.totalTimeChartInstance) {
        window.totalTimeChartInstance.destroy();
    }
    window.totalTimeChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: names,
            datasets: [{
                label: 'Total Time (minutes)',
                data: minutes,
                backgroundColor: 'rgba(54, 162, 235, 0.6)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false },
                title: { display: false }
            },
            scales: {
                x: { title: { display: false } },
                y: { beginAtZero: true, title: { display: true, text: 'Minutes' } }
            }
        }
    });
};
