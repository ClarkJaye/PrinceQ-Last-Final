$(document).ready(function () {
    loadTotalServing();
});

// Load TotalServing
var ServingChart;
var ServingLabels = [];
var ServingDatasets = [];

function loadTotalServing() {
    var yearDropdown = document.getElementById('selectedYear').value;
    var monthDropdown = document.getElementById('selectedMonth').value;

    const xValues = ["Months"];

    ServingChart = new Chart("waitingChart", {
        type: "line",
        data: {
            labels: xValues,
            datasets: []
        },
        options: {
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (tooltipItem) {
                            const dataValue = tooltipItem.raw;
                            return `Time: ${formatTooltipTime(dataValue)}`;
                        }
                    }
                }
            },
            legend: {
                display: true
            },
            tooltips: {
                mode: 'index',
                intersect: false
            },
            elements: {
                line: {
                    tension: 0.1
                }
            },
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Month'
                    },
                    categorySpacing: 1,
                    ticks: {
                        autoSkip: true,
                        maxRotation: 40,
                        minRotation: 40
                    }
                },
                y: {
                    min: 0,
                    title: {
                        display: true,
                        text: 'Waiting Time (hh.mm.ss)'
                    },
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return formatTooltipTime(value);
                        }
                    }
                },
            },
        },
        plugins: [plugin]
    });

    LoadWaitingData(yearDropdown, monthDropdown);
}

// Load the Data
function LoadWaitingData(yearData, monthData) {
    $.ajax({
        type: 'GET',
        url: '/Admin/GetAllWaitingTimeData',
        dataType: 'json',
        data: { year: yearData, month: monthData },
        success: function (data) {
            if (data && data.byMonth === true) {
                data.value.sort((a, b) => a.month - b.month);
                ServingLabels = data.value.map(item => {
                    const monthIndex = parseInt(item.month) - 1;
                    return new Date(0, monthIndex).toLocaleString('en-US', { month: 'long' });
                });
            } else if (data && data.byMonth === false) {
                data.value.sort((a, b) => a.day - b.day);
                ServingLabels = data.value.map(item => {
                    const year = item.generateDate.substring(0, 4);
                    const month = item.generateDate.substring(4, 6);
                    const day = item.generateDate.substring(6, 8);
                    return `${day}\n${new Date(year, month - 1, day).toLocaleString('en-US', { weekday: 'long' })}`;
                });
            }

            if (data.value.length > 0) {
                ServingDatasets = [
                    {
                        label: 'Category A',
                        data: data.value.map(item => parseSeconds(item.categoryAAvg)),
                        borderColor: "#157347",
                        backgroundColor: "#157347",
                        pointBackgroundColor: "#157347",
                        pointRadius: 5,
                        fill: false
                    },
                    {
                        label: 'Category B',
                        data: data.value.map(item => parseSeconds(item.categoryBAvg)),
                        borderColor: "#ffca2c",
                        backgroundColor: "#ffca2c",
                        pointBackgroundColor: "#ffca2c",
                        pointRadius: 5,
                        fill: false
                    },
                    {
                        label: 'Category C',
                        data: data.value.map(item => parseSeconds(item.categoryCAvg)),
                        borderColor: "#dc3545",
                        backgroundColor: "#dc3545",
                        pointBackgroundColor: "#dc3545",
                        pointRadius: 5,
                        fill: false
                    },
                    {
                        label: 'Category D',
                        data: data.value.map(item => parseSeconds(item.categoryDAvg)),
                        borderColor: "#0b5ed7",
                        backgroundColor: "#0b5ed7",
                        pointBackgroundColor: "#0b5ed7",
                        pointRadius: 5,
                        fill: false
                    }
                ];
            } else {
                ServingDatasets = [];
            }

            ServingChart.data.labels = ServingLabels;
            ServingChart.data.datasets = ServingDatasets;
            ServingChart.update();
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}

// Helper function to convert hh.mm.ss to total seconds
function parseSeconds(formattedTime) {
    const parts = formattedTime.split('.');
    const hours = parseInt(parts[0], 10) || 0;
    const minutes = parseInt(parts[1], 10) || 0;
    const seconds = parseInt(parts[2], 10) || 0;

    return (hours * 3600) + (minutes * 60) + seconds;
}


function formatTooltipTime(seconds) {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    return `${hours > 0 ? `${hours}h ` : ''}${minutes > 0 ? `${minutes}m ` : ''}${secs > 0 ? `${secs}s` : ''}`;
}





// Plugin to adjust legend spacing
const plugin = {
    beforeInit(chart) {
        const originalFit = chart.legend.fit;
        chart.legend.fit = function fit() {
            originalFit.bind(chart.legend)();
            this.height += 30;
        };
    }
};