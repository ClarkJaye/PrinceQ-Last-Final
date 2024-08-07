$(document).ready(function () {
    load_Data();
});

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
//creating the Table using Datatable
function populateTable(data) {
    var table = $("#servingReportTable").DataTable({
        data: data,
        destroy: true,
        columns: [
            { data: 'username' },
            { data: 'totalNumberServed' },
            {
                data: 'longestServedTime',
                render: function (data) {
                    return formatTime(data);
                }
            },
            {
                data: 'shortestServedTime',
                render: function (data) {
                    return formatTime(data);
                }
            },
            {
                data: 'date',
                render: function (data) {
                    return formatDate(data);
                }
            },
            {
                data: null,
                render: function (data) {
                    return `<button class="btn btn-sm btn-primary view-details" data-userid="${data.user.clerkId}" data-date="${data.user.generateDate}">View All</button>`;
                }
            }
        ],
        columnDefs: [
            {
                targets: 1, 
                className: 'text-leftt' 
            }
        ]
    });


    // Event listener for the "View All" button
    $('#servingReportTable tbody').on('click', 'button.view-details', function () {
        var button = $(this);
        var userId = button.data('userid');
        var date = button.data('date');

        $.ajax({
            type: 'GET',
            url: '/Admin/GetDetailedData',
            data: { clerkId: userId, generateDate: date },
            dataType: 'json',
            success: function (response) {
                if (response.isSuccess) {
                    populateModalViewDetails(response.data);
                    $('#detailsModal').modal('show');
                } else {
                    console.error(response.message);
                }
            },
            error: function (error) {
                console.log("Unable to get the detailed data. " + error);
            }
        });
    });
}

//creating the Table inside the Modal 
function populateModalViewDetails(data) {
    var tableBody = $("#detailsTable tbody");
    tableBody.empty();

    data.forEach(item => {
        var serveStart = new Date(`1970-01-01T${item.serveStart}Z`);
        var serveEnd = new Date(`1970-01-01T${item.serveEnd}Z`);
        var servingTime = (serveEnd - serveStart) / 1000;

        var category = getCategoryLetter(item.categoryId);
        var row = `<tr>
            <td>${formatDate(item.generateDate)}</td>
            <td>${item.username}</td>
            <td>${category}-${item.queueNumber}</td>
             <td>${item.total_Cheque} pcs</td>
            <td>${formatTime(serveStart)}</td>
            <td>${formatTime(serveEnd)}</td>
            <td>${formatTime(servingTime)}</td>
        </tr>`;
        tableBody.append(row);
    });
}

function formatTime(secondsOrDate) {
    if (typeof secondsOrDate === 'number') {
        // Formatting seconds
        var minutes = Math.floor(secondsOrDate / 60);
        var remainingSeconds = Math.floor(secondsOrDate % 60);
        return `${minutes} mins and ${remainingSeconds} secs`;
    } else if (secondsOrDate instanceof Date) {
        // Formatting Date object
        var hours = secondsOrDate.getUTCHours();
        var minutes = secondsOrDate.getUTCMinutes();
        var secs = secondsOrDate.getUTCSeconds();
        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    } else {
        return '';
    }
}

function formatDate(date) {
    var year = date.slice(0, 4);
    var month = date.slice(4, 6);
    var day = date.slice(6, 8);
    return `${month}/${day}/${year}`;
}
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