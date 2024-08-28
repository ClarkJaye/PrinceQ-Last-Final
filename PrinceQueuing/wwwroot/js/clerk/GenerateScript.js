$(document).ready(function () {
    //If disable
    var userDbld = $("#isDisabledUser").val()
    if (userDbld) {
        $('#notificationModal').modal('show');
    }


    displayDateTime();
    setInterval(displayDateTime, 1000);

    //Show Modal
    $('.generateBtn').on("click", GenerateQueue);
    //Print
    $('#printBtn').on("click", PrintQueueForm);
    //Recently History
    $('#historyBtn').on("click", RecentlyGenerated);
    //Print From Recently History
    $('#historyTable').on("click", ".recentPrintBtn", PrintQueueFormFromRecentHistory);


    $('#cutOffBtn').on("click", function () {
        var btn = $(this)
        $.ajax({
            type: 'GET',
            url: "/Clerk/AnnounceCutOff",
            dataType: "json",
            success: function (response) {
                btn.prop("disabled", true);
                setTimeout(function () {
                    btn.prop("disabled", false);
                }, 1000);
            },
        });
    });
});

const displayDateTime = () => {
    var currentTime = new Date();
    var options = { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true };
    var formattedTime = currentTime.toLocaleString('en-US', options);
    var optionsDate = { year: 'numeric', month: 'long', day: 'numeric' };
    var formattedDate = currentTime.toLocaleString('en-US', optionsDate);

    $('#time').text(formattedTime);
    $('#date').text(formattedDate);
};
function GenerateQueue() {
    var categoryId = $(this).attr('id');
    $.ajax({
        type: 'POST',
        url: "/Clerk/GenerateQueueNumber",
        dataType: "json",
        data: { categoryId: categoryId },
        success: function (response) {
            console.log(response);

            if (response && response.isSuccess) {
                // Handle success
                var queueItem = response.obj1;
                var category = response.obj2;
                var queueNumber;

                $('#modalCategoryName').text(category.categoryName);

                switch (queueItem.categoryId) {
                    case 1:
                        queueNumber = "A - " + queueItem.queueNumber;
                        break;
                    case 2:
                        queueNumber = "B - " + queueItem.queueNumber;
                        break;
                    case 3:
                        queueNumber = "C - " + queueItem.queueNumber;
                        break;
                    case 4:
                        queueNumber = "D - " + queueItem.queueNumber;
                        break;
                    default:
                        queueNumber = "Error";
                }

                $('#modalQueueNumber').text(queueNumber);

                var generatedAt = new Date(
                    queueItem.queueId.substr(0, 4),
                    queueItem.queueId.substr(4, 2) - 1,
                    queueItem.queueId.substr(6, 2)
                );
                var options = { year: 'numeric', month: 'long', day: 'numeric' };
                var formattedDateTime = generatedAt.toLocaleString('en-US', options);
                $('#modalGeneratedAt').text(formattedDateTime);

                $('#gDate').val(queueItem.queueId);
                $('#categoryId').val(queueItem.categoryId);
                $('#qNumber').val(queueItem.queueNumber);

                $('#queueNumberModal').modal('show');
                $('.modal-header').removeClass().addClass('modal-header m-header-category-' + categoryId);
            } else {
                alert(response.message);
            }
        },
        error: function (xhr, status, error) {
            alert("An error occurred while generating the queue number.");
        }
    });
}
function PrintQueueForm() {
    var date = document.getElementById("gDate").value;
    var categoryId = document.getElementById("categoryId").value;
    var queueNumber = document.getElementById("qNumber").value;

    $.ajax({
        type: 'GET',
        url: "/Clerk/Print_QueueNumber",
        data: { date: date, categoryId: categoryId, queueNumber: queueNumber },
        success: function (response) {
            if (response && response.isSuccess === false) {
                alert(response.message);
            } else if (response && response.isSuccess === true) {
                window.location.href = "/Clerk/Print_Form?date=" + date + "&categoryId=" + categoryId + "&queueNumber=" + queueNumber;
            }
        },
        error: function (xhr, status, error) {
            // Handle error
            alert("An error occurred while generating the queue number.");
        }
    });
}

function PrintQueueFormFromRecentHistory() {
    var date = $(this).data("gendate");
    var categoryId = $(this).data("category");
    var queueNumber = $(this).data("qnumber");
    $.ajax({
        type: 'GET',
        url: "/Clerk/Print_QueueNumber",
        data: { date: date, categoryId: categoryId, queueNumber: queueNumber },
        success: function (response) {
            if (response && response.isSuccess === false) {
                alert(response.message);
            } else if (response && response.isSuccess === true) {
                window.location.href = "/Clerk/Print_Form?date=" + date + "&categoryId=" + categoryId + "&queueNumber=" + queueNumber;
            }
        },
        error: function (xhr, status, error) {
            // Handle error
            alert("An error occurred while generating the queue number.");
        }
    });
}

function RecentlyGenerated() {
    $.ajax({
        type: 'GET',
        url: '/Clerk/Get_RecentDataQueue',
        dataType: 'json',
        success: function (response) {
            var recentQ = response.obj.data;
            var categoryCounts = response.obj.categoryCounts || {};
            $("#totalA").text(categoryCounts.categoryA || 0);
            $("#totalB").text(categoryCounts.categoryB || 0);
            $("#totalC").text(categoryCounts.categoryC || 0);
            $("#totalD").text(categoryCounts.categoryD || 0);
            recentQ.sort(function (a, b) {
                return new Date(b.generate_at) - new Date(a.generate_at);
            });
            var tbody = $("#historyTable");
            tbody.empty();

            if (recentQ.length > 0) {
                var tableRows = recentQ.map(function (queue) {
                    return createTableRecently(queue);
                });
                tbody.append(tableRows);
            } else {
                var noReserveRow = $("<tr>").append($("<td colspan='3'>").text("No data yet"));
                tbody.append(noReserveRow);
            }
        },
        error: function (error) {
            console.log("Unable to get the data. " + error);
        }
    });

    $('#historyModal').modal('show');
    $('.modal-header').removeClass().addClass('modal-header bg-secondary');
}

//Function Create Table
function createTableRecently(queue) {
    var queueNumber = $("<th>").addClass("align-middle").text(queue.category + "-" + queue.queueNumber);

    var reserve_At = new Date(queue.generate_at);
    var formattedDateTime = formatDate(reserve_At);

    var generateAtCell = $("<td>")
        .addClass("align-middle")
        .text(formattedDateTime);

    var actionCell = $("<td>");

    //Button
    var serveButton = $("<button>")
        .addClass("btn btn-sm btn-success recentPrintBtn")
        .attr("data-genDate", queue.generateDate)
        .attr("data-category", queue.categoryId)
        .attr("data-qnumber", queue.queueNumber)
        .text("Print");
  
    actionCell.append(serveButton);

    var tableRow = $("<tr>").append(generateAtCell, queueNumber, actionCell);
    return tableRow;
}

function formatDate(date) {
    var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleString('en-US', options).toUpperCase();
}
