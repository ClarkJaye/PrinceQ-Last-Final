$(document).ready(function () {
    load_Data();


    $('#toExcelBtn').click(function () {
        convertDetailsToExcel();
    });
});

function convertDetailsToExcel() {
    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.table_to_sheet(document.getElementById('detailsTable')); 

    ws['!cols'] = [{ wch: 20 }]; 

    XLSX.utils.book_append_sheet(wb, ws, 'Details');

    XLSX.writeFile(wb, 'DetailedData.xlsx');
}


function load_Data() {
    $.ajax({
        type: 'GET',
        url: '/Admin/Serving_GetAllServedData',
        dataType: 'json',
        success: function (response) {
            if (response.isSuccess) {
                populateTable(response.obj);
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
        order: [[1, 'desc']], 
        columnDefs: [
            {
                targets: 5, 
                className: 'text-center' 
            }
        ]
    });

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
                    populateModalViewDetails(response.obj);
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
        var serveStart = item.serveStart && item.serveStart !== '00:00:00' ? new Date(`1970-01-01T${item.serveStart}Z`) : null;
        var serveEnd = item.serveEnd && item.serveEnd !== '00:00:00' ? new Date(`1970-01-01T${item.serveEnd}Z`) : null;
        var servingTime = (serveEnd && serveStart) ? (serveEnd - serveStart) / 1000 : "N/A";

        var stage;
        var stageClass;
        if (item.categoryId !== 4) {
            stage = item.stageId === 1 ? "Filling" : "Releasing";
            stageClass = item.stageId === 1 ? "text-primary" : "text-danger";
        } else {
            stage = "Inquiry";
            stageClass = "text-muted"
        }


        var category = getCategoryLetter(item.categoryId);
        var generateDate = item.generateDate ? formatDate(item.generateDate) : 'N/A';
        var row = `<tr>
            <td>${generateDate}</td>
            <td>${item.username}</td>
            <td>${category}-${item.queueNumber}</td>
            <td class="${stageClass}">${stage}</td>
            <td>${item.total_Cheque} pcs</td>
            <td>${serveStart ? formatTime(serveStart) : 'N/A'}</td>
            <td>${serveEnd ? formatTime(serveEnd) : 'N/A'}</td>
            <td>${typeof servingTime === 'number' ? formatTime(servingTime) : 'N/A'}</td>
        </tr>`;
        tableBody.append(row);
    });
}


//Helper
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
    if (!date) return 'N/A'; 
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
