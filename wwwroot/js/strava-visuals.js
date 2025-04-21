window.renderPieChart = function(labels, data) {
    if (!window.Chart) return;
    var ctx = document.getElementById('typePieChart');
    if (!ctx) return;
    if (window.typePieChartInstance) window.typePieChartInstance.destroy();
    window.typePieChartInstance = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: [
                    '#36A2EB', '#FF6384', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40', '#B2DFDB'
                ]
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { position: 'bottom' } }
        }
    });
};

window.renderLineChart = function(labels, data) {
    if (!window.Chart) return;
    var ctx = document.getElementById('distanceLineChart');
    if (!ctx) return;
    if (window.distanceLineChartInstance) window.distanceLineChartInstance.destroy();
    window.distanceLineChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Total Distance (km)',
                data: data,
                fill: false,
                borderColor: '#36A2EB',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'km' } }
            }
        }
    });
};
