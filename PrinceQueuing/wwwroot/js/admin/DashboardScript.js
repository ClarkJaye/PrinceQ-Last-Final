$(document).ready(function () {
    loadTotalServing();
    TotalGeneratedNumber();
    TotalReserveNumber();
    TotalCancelNumber();
    TotalQueueServe();
    GetAllRecentQueue();
});

//Load TotalServing
var ServingChart;
var ServingLabels = [];
var ServingDatasets = [];
function loadTotalServing() {
    var yearDropdown = document.getElementById('selectedYear').value;
    var monthDropdown = document.getElementById('selectedMonth').value;
    const xValues = ["Months"];

    ServingChart = new Chart("myChart", {
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
                    tension: 0.2
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
    LoadData(yearDropdown, monthDropdown);
}

// Load the Data
function LoadData(yearData, monthData) {

    $.ajax({
        type: 'GET',
        url: '/Admin/GetDataByYearAndMonth',
        dataType: 'json',
        data: { year: yearData, month: monthData },
        success: function (data) {
             //console.log(data);
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
                    label: 'Trade',
                    data: data.map(item => item.categoryASum),
                    borderColor: "#157347",
                    backgroundColor: "#157347",
                    pointBackgroundColor: "#157347",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Non-Trade',
                    data: data.map(item => item.categoryBSum),
                    borderColor: "#ffca2c",
                    backgroundColor: "#ffca2c",
                    pointBackgroundColor: "#ffca2c",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Special',
                    data: data.map(item => item.categoryCSum),
                    borderColor: "#dc3545",
                    backgroundColor: "#dc3545",
                    pointBackgroundColor: "#dc3545",
                    pointRadius: 5,
                    fill: false
                },
                {
                    label: 'Inquiry',
                    data: data.map(item => item.categoryDSum),
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

//Total Clerk
function TotalGeneratedNumber() {
    $.ajax({
        type: "GET",
        url: "/admin/TotalQueueNumber",
        success: function (response) {
            var generateQ = document.getElementById("totalGeneratedQ");
            if (response) {
                generateQ.textContent = response.obj.value;
            }
            else {
                generateQ.textContent = 0;
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}

//Total Clerk
function TotalQueueServe() {
    $.ajax({
        type: "GET",
        url: "/admin/totalServed",
        success: function (response) {
            console.log(response)
            var totalQ = document.getElementById("totalServed");
            if (response) {
                totalQ.textContent = response.obj.value;
            }
            else {
                totalQ.textContent = 0;
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}

//Total Reserved Queue Nubmer
function TotalReserveNumber() {
    $.ajax({
        type: "GET",
        url: "/admin/totalReservedNumber",
        success: function (response) {
            var totalQ = document.getElementById("totalReserved");
            if (response) {
                totalQ.textContent = response.obj.value;
            }
            else {
                totalQ.textContent = 0;
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}

//Total Cancel Queue Nubmer
function TotalCancelNumber() {
    $.ajax({
        type: "GET",
        url: "/admin/totalCancelNumber",
        success: function (response) {
            var totalQ = document.getElementById("totalCancel");
            if (response) {
                totalQ.textContent = response.obj.value;
            }
            else {
                totalQ.textContent = 0;
            }
        },
        error: function (error) {
            console.log(error)
        }
    })
}

// Load Reserve Table
function GetAllRecentQueue() {
    $.ajax({
        type: 'GET',
        url: "/admin/GetQueueServed",
        dataType: "json",
        success: function (response) {
            var items = response?.obj?.data || []; 
            var tbody = $("#recentlyQServes");
            tbody.empty();
            if (items.length > 0) {
                var tableRows = items.map(createTableRecent);
                tbody.append(tableRows);
            } else {
                var noItemRow = $("<tr class='text-center'>").append($("<td colspan='4'>").text("No Serving yet"));
                tbody.append(noItemRow);
            }
        },
        error: function (error) {
            var tbody = $("#recentlyQServes");
            tbody.empty();
            var errorRow = $("<tr class='text-center text-danger'>").append($("<td colspan='4'>").text("Failed to load data"));
            tbody.append(errorRow);
        }
    });
}
// Function to Create Table Row
function createTableRecent(queue) {
    var category = getCategoryLetter(queue.category)
    var queueNumber = $("<th>").text(category + " - " + queue.qNumber);
    var servingByCell = $("<td>").text(queue.servingBy);
    var stageColor = queue.stage === 2 ? "text-danger" : "text-success"
    var stageCell = $("<td>").addClass(stageColor).text(queue.stage === 2 ? "Releasing" : "Filling");
    var dateTimeCell = $("<td>").text(queue.dateTime);

    var tableRow = $("<tr>").append(queueNumber, servingByCell, stageCell, dateTimeCell);
    return tableRow;
}
//Helper
function getCategoryLetter(categoryId) {
    switch (categoryId) {
        case 1:
            return 'A';
        case 2:
            return 'B';
        case 3:
            return 'C';
        case 4:
            return 'D';
        default:
            return '';
    }
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