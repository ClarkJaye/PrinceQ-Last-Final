var dataTable;
$(document).ready(function () {
    GetAllCategories();
    GetCategoryCount();

    $("#addCategoryForm").on("submit", function (e) {
        AddCategory(e);
    });
    $("#updateCategoryForm").on("submit", function (e) {
        UpdateCategory(e);
    });

    // Reset the modal
    const categoryModal = document.getElementById('categoryModal');
    categoryModal.addEventListener('hidden.bs.modal', function (event) {
        const form = categoryModal.querySelector('form');
        const span = categoryModal.querySelectorAll('span');
        span.forEach(function (element) {
            element.textContent = "";
        });
        form.reset();
    });
});
//Total Category
function GetCategoryCount() {
    $.ajax({
        url: '/admin/GetCategoryCount',
        type: 'GET',
        success: function (response) {
            $('#categoryCount').text(response.totalCount);
        },
        error: function (err) {
            console.log('Unable to fetch the category count.', err);
        }
    });
}
//Format DateTime
function formatDate(date) {
    var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    return date.toLocaleString('en-US', options).toUpperCase();
}
//Load Tables
function GetAllCategories() {
    $.ajax({
        url: '/admin/GetAllCategories',
        type: 'Get',
        async: false,
        success: function (response) {
            GetCategoryCount();
            var categoryData = response.data.map(function (value) {
                console.log(value)
                var created_At = new Date(value.created_At);
                var formattedDateTime = formatDate(created_At);

                var editButton = $('<button>')
                    .addClass('btn btn-sm btn-primary d-flex align-items-center gap-1')
                    .attr('id', value.categoryId)
                    .attr('onclick', `EditCategory('${value.categoryId}')`)
                    .html('<i class="lni lni-pencil mr-2"></i><span>Edit</span>');

                var deleteButton = $('<button disabled>')
                    .addClass('btn btn-sm btn-danger d-flex align-items-center gap-1')
                    .attr('id', value.categoryId)
                    .attr('onclick', `DeleteCategory('${value.categoryId}')`)
                    .html('<i class="lni lni-trash-can mr-2"></i><span> Delete </span>');

                var buttonsContainer = $('<div>').addClass('d-flex justify-content-center gap-2');
                buttonsContainer.append(editButton, deleteButton);

                return [
                    value.categoryName,
                    value.description,
                    formattedDateTime,
                    `<a class=" btn-sm ${value.isActiveId === 1 ? 'btn-success' : 'btn-danger'} custom-btn-font">${value.isActiveId === 1 ? 'Active' : 'Inactive'}</a>`,
                    buttonsContainer.prop('outerHTML')
                ];
            });

            if (dataTable) {
                dataTable.clear().rows.add(categoryData).draw();
            } else {
                dataTable = new DataTable("#categoryTable", {
                    data: categoryData
                });
            }

        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }
    });
}
//ADD
function AddCategory(e) {
    e.preventDefault();
    $.ajax({
        url: '/admin/AddCategory',
        type: 'POST',
        data: $('#addCategoryForm').serialize(),
        success: function (response) {
            if (response.isSuccess) {
                $('#categoryModal').modal('hide');
                GetAllCategories();
                toastr.success(response.message);
            } else {
                var errors = response.errors;
                for (var key in errors) {
                    $('#' + key + '-validation-error').text(errors[key]);
                }
            }
        },
        error: function (err) {
            console.log('Error:', err);
        }
    });
}
//DELETE
function DeleteCategory(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: "/admin/deletecategory?id=" + id,
                type: 'DELETE',
                success: function (data) {
                    if (data && data.isSuccess) {
                        GetAllCategories();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
//EDIT
function EditCategory(id) {
    $.ajax({
        url: "/admin/GetCategory?id=" + id,
        type: "GET",
        dataType: 'json',
        success: function (response) {
            var category = response.category;
            $("#editCategoryId").val(category.categoryId);
            $("#editCategoryName").val(category.categoryName);
            $("#editDescription").val(category.description);
            $("#editCreated_At").val(formatDate(new Date(category.created_At)));
            $('#categoryEditModal').modal('show');
        },
        error: function (err) {
            console.log('Unable to fetch the data.', err);
        }

    })
}
//Update 
function UpdateCategory(e) {
    e.preventDefault();

    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, update it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: "/admin/updateCategory",
                type: "POST",
                data: $('#updateCategoryForm').serialize(),
                dataType: 'json',
                success: function (response) {
                    console.log(response)
                    if (response && response.isSuccess) {
                        GetAllCategories();
                        toastr.success(response.message)
                    }
                    else {
                        toastr.error(response.message)
                    }

                    $('#categoryEditModal').modal('hide');
                },
                error: function (err) {
                    console.log('Unable to fetch the data.', err);
                }

            })
        }
    });
   
}