$(document).ready(function () {
    load_Data();
});

function load_Data() {
    $.ajax({
        type: 'GET',
        url: '/Admin/Waiting_GetAllServedData',
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

function populateTable(data) {
    var table = $("#waitingReportTable").DataTable({
        data: data,
        destroy: true,
        columns: [
            {
                data: 'generateDate',
                render: function (data) {
                    return data ? formatDate(data) : 'N/A';
                }
            },
            {
                data: null,
                render: function (data) {
                    return getCategoryLetter(data.categoryId) + '-' + data.queueNumber;
                }
            },
            { data: 'generatedStart' },
            { data: 'callForFilling' },
            { data: 'callForReleasing' },
            { data: 'releasingEnd' },
            { data: 'averageTime' },
        ],
        
    });

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
