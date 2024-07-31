$(document).ready(function () {
    loadServingReport();
    TotalClerk();
    load_Clerks();
});

//Load TotalServing
var servedChart;
var servedLabels = [];
var servedDatasets = [];
function loadServingReport() {
    var clerkDropdown = document.getElementById('selectedClerk').value;
    var yearDropdown = document.getElementById('selectedYear').value;
    var monthDropdown = document.getElementById('selectedMonth').value;
    const xValues = ["Months"];

    servedChart = new Chart("servedChart", {
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
    LoadServeData(clerkDropdown, yearDropdown, monthDropdown);
}

// Load the Data
function LoadServeData(clerkData, yearData, monthData) {

    $.ajax({
        type: 'GET',
        url: '/Admin/GetServingDataClerk',
        dataType: 'json',
        data: { clerkId: clerkData, year: yearData, month: monthData },
        success: function (data) {
            // console.log(data);
            var month = $('#selectedMonth option:selected').data("month");
            $('#monthSelect').text(month);

            if (data && data.byMonth) {
                data = data.value.sort((a, b) => a.month - b.month);
                servedLabels = data.map(item => {
                    const monthIndex = parseInt(item.month) - 1;
                    const monthName = new Date(0, monthIndex).toLocaleString('en-US', { month: 'long' });
                    return monthName;
                });
            } else if (data && !data.byMonth) {
                data = data.value.sort((a, b) => a.day - b.day);
                servedLabels = data.map(item => {
                    const year = item.generateDate.substring(0, 4);
                    const month = item.generateDate.substring(4, 6);
                    const day = item.generateDate.substring(6, 8);
                    const formattedDate = `${day}\n${new Date(year, month - 1, day).toLocaleString('en-US', { weekday: 'long' })}`;
                    return formattedDate;
                });
            }

            servedDatasets = [
                {
                    label: 'Category A',
                    data: data.map(item => item.categoryASum),
                    borderColor: "#157347",
                    backgroundColor: "#157347",
                    pointBackgroundColor: "#157347",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Category B',
                    data: data.map(item => item.categoryBSum),
                    borderColor: "#ffca2c",
                    backgroundColor: "#ffca2c",
                    pointBackgroundColor: "#ffca2c",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Category C',
                    data: data.map(item => item.categoryCSum),
                    borderColor: "#dc3545",
                    backgroundColor: "#dc3545",
                    pointBackgroundColor: "#dc3545",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Category D',
                    data: data.map(item => item.categoryDSum),
                    borderColor: "#0b5ed7",
                    backgroundColor: "#0b5ed7",
                    pointBackgroundColor: "#0b5ed7",
                    pointRadius: 5,
                    fill: false
                }
            ];

            console.log(servedDatasets)

            servedChart.data.labels = servedLabels;
            servedChart.data.datasets = servedDatasets;
            servedChart.update();
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

//Total Clerk
function TotalClerk() {
    $.ajax({
        type: "GET",
        url: "/admin/CountClerk",
        success: function (response) {
            var Clerk = document.getElementById("totalClerk");
            if (response) {
                Clerk.textContent = response;
            }
            else {
                Clerk.textContent = 0;
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}


//Load Clerks
function load_Clerks() {
    $.ajax({
        type: 'GET',
        url: '/Admin/GetClerks_Categories',
        dataType: 'json',
        success: function (response) {
            // Set selected clerks
            $.each(response.clerks, function (i, data) {
                $('#selectedClerk').append('<option value=' + data.id + '>' + data.userName + '</option>');
            });
        },
        error: function (error) {
            console.log("Unable to get the data. " + error);
        }
    })
}


