window.renderCalendarHeatmap = function(activityDates) {
    // Use CalHeatMap or a simple SVG grid for demo
    var container = document.getElementById('calendarHeatmap');
    if (!container) return;
    container.innerHTML = '';
    var today = new Date();
    var year = today.getFullYear();
    var firstDay = new Date(year, 0, 1);
    var daysInYear = (new Date(year+1, 0, 1) - firstDay) / (1000*60*60*24);
    var activitySet = new Set(activityDates.map(d => d.split('T')[0]));
    for (let i = 0; i < daysInYear; i++) {
        let d = new Date(firstDay.getTime() + i * 24*60*60*1000);
        let key = d.toISOString().split('T')[0];
        let active = activitySet.has(key);
        let cell = document.createElement('span');
        cell.style.display = 'inline-block';
        cell.style.width = '10px';
        cell.style.height = '10px';
        cell.style.margin = '1px';
        cell.style.background = active ? '#36A2EB' : '#e0e0e0';
        cell.title = key;
        container.appendChild(cell);
        if (d.getDay() === 6) container.appendChild(document.createElement('br'));
    }
};
