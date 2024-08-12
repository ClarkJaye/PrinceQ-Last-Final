$(document).ready(function () {
    load_Data();
    $('#export-to-excel').on('click', function () {
        exportToExcel();
    });
});
//Export to Excel
function exportToExcel() {
    var selectedData = [];
    $('#waitingReportTable tbody input[type="checkbox"].row-select:checked').each(function () {
        var row = $(this).closest('tr');
        var rowData = $('#waitingReportTable').DataTable().row(row).data();
        selectedData.push(rowData);
    });

    if (selectedData.length > 0) {
        var ws = XLSX.utils.json_to_sheet(selectedData);
        var wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Selected Rows");

        XLSX.writeFile(wb, "SelectedRows.xlsx");
    } else {
        alert("Please select at least one row to export.");
    }
}
//Create and Load Data
function load_Data() {
    $.ajax({
        type: 'GET',
        url: '/Admin/Waiting_GetAllServedData',
        dataType: 'json',
        success: function (response) {
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
                data: null,
                render: function (data, type, row) {
                    return '<div class="d-flex justify-content-center align-items-center w-100 h-100"><input type="checkbox" class="row-select select-checkbox" value="' + row.queueNumber + '" /></div>';
                },
                orderable: false,
            },
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
            { data: 'callForFilling_Reserved' },
            { data: 'callForReleasing' },
            { data: 'callForReleasing_Reserved' },
            { data: 'releasingEnd' },
            { data: 'averageTime' },
        ],
        order: [[1, 'asc']], 
    });

    $('#select-all').on('click', function () {
        var rows = table.rows({ 'search': 'applied' }).nodes();
        $('input[type="checkbox"].row-select', rows).prop('checked', this.checked).trigger('change');
    });

    $('#waitingReportTable tbody').on('change', 'input[type="checkbox"].row-select', function () {
        if (!this.checked) {
            var el = $('#select-all').get(0);
            if (el && el.checked && ('indeterminate' in el)) {
                el.indeterminate = true;
            }
        }
    });
}

//Helper
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
