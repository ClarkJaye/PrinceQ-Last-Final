$(document).ready(function () {
    loadTotalServing();
});

//Load TotalServing
var ServingChart;
var ServingLabels = [];
var ServingDatasets = [];
function loadTotalServing() {
    var rangeDropdown = document.getElementById('selectedRange').value;
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
                    categorySpacing: 1,
                    ticks: {
                        autoSkip: true,
                        maxRotation: 40,
                        minRotation: 40
                    }
                },
                y: {
                    min: 0,
                },
            },
        },
        plugins: [plugin]
    });
    LoadWaitingData(rangeDropdown, yearDropdown, monthDropdown);
}

// Load the Data
function LoadWaitingData(rangeData, yearData, monthData) {

    $.ajax({
        type: 'GET',
        url: '/Admin/GetAllWaitingTimeData',
        dataType: 'json',
        data: { range:rangeData, year: yearData, month: monthData },
        success: function (data) {
            console.log(data)
            if (data && data.byMonth == true) {
                data = data.value.sort((a, b) => a.month - b.month);
                ServingLabels = data.map(item => {
                    const monthIndex = parseInt(item.month) - 1;
                    const monthName = new Date(0, monthIndex).toLocaleString('en-US', { month: 'long' });
                    return monthName;
                });
            }
            else if (data && data.byMonth == false) {
                data = data.value.sort((a, b) => a.day - b.day);
                ServingLabels = data.map(item => {
                    const year = item.generateDate.substring(0, 4);
                    const month = item.generateDate.substring(4, 6);
                    const day = item.generateDate.substring(6, 8);
                    const formattedDate = `${day}\n${new Date(year, month - 1, day).toLocaleString('en-US', { weekday: 'long' })}`;
                    return formattedDate;
                });
            }

            ServingDatasets = [
                {
                    label: 'Category A',
                    data: data.map(item => item.categoryAAvg),
                    borderColor: "#157347",
                    backgroundColor: "#157347",
                    pointBackgroundColor: "#157347",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Category B',
                    data: data.map(item => item.categoryBAvg),
                    borderColor: "#ffca2c",
                    backgroundColor: "#ffca2c",
                    pointBackgroundColor: "#ffca2c",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Category C',
                    data: data.map(item => item.categoryCAvg),
                    borderColor: "#dc3545",
                    backgroundColor: "#dc3545",
                    pointBackgroundColor: "#dc3545",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Category D',
                    data: data.map(item => item.categoryDAvg),
                    borderColor: "#0b5ed7",
                    backgroundColor: "#0b5ed7",
                    pointBackgroundColor: "#0b5ed7",
                    pointRadius: 5,
                    fill: false
                }
            ];


            ServingChart.data.labels = ServingLabels;
            ServingChart.data.datasets = ServingDatasets;
            ServingChart.update();
        },
        error: function (error) {
            console.log("Error:", error);
        }
    });
}

//Plugin
const plugin = {
    beforeInit(chart) {
        const originalFit = chart.legend.fit;
        chart.legend.fit = function fit() {
            originalFit.bind(chart.legend)();
            this.height += 30;
        }
    }
}






