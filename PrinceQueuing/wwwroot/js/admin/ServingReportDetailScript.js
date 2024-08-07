$(document).ready(function () {
    load_Data();
});

//Load All Data
function load_Data() {
    $.ajax({
        type: 'GET',
        url: '/Admin/Serving_GetAllServedData',
        dataType: 'json',
        success: function (response) {
            console.log(response);

            if (response.isSuccess) {
                populateTable(response.data);
            } else {
                console.error(response.message);
            }
        },
        error: function (error) {
            console.log("Unable to get the data. " + error);
        }
    });
}

// Populate table with data
function populateTable(data) {
    var tbody = $("#waitingReportTable tbody");
    tbody.empty();

    data.forEach(function (item) {
        var highestAverageServedTime = formatTime(item.HighestAverageServedTime);
        var lowestServedTime = formatTime(item.LowestServedTime);

        var row = $("<tr>");
        row.append($("<td>").text(item.username));
        row.append($("<td>").text(item.totalNumberServed));
        row.append($("<td>").text(highestAverageServedTime));
        row.append($("<td>").text(lowestServedTime));
        row.append($("<td>").text(formatDate(item.date)));
        row.append($("<td>").append($("<button>").addClass("btn btn-sm btn-primary").text("View All")));

        tbody.append(row);
    });
}

function formatTime(seconds) {
    var minutes = Math.floor(seconds / 60);
    var remainingSeconds = Math.floor(seconds % 60);
    return `${minutes} mins and ${remainingSeconds} secs`;
}

function formatDate(date) {
    var year = date.slice(0, 4);
    var month = date.slice(4, 6);
    var day = date.slice(6, 8);
    return `${month}/${day}/${year}`;
}
