let datatable;

$(document).ready(function () {
    loadDatatable();
});

function loadDatatable() {
    datatable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll"
        },
        "columns": [
            {"data": "id", "width": "10%"},
            {"data": "name", "width": "15%"},
            {"data": "phoneNumber", "width": "15%"},
            {"data": "user.email", "width": "15%"},
            {"data": "orderStatus", "width": "15%"},
            {"data": "orderTotal", "width": "15%"},
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <div class="w-75 btn-group" role="group">
                    <a href="/Admin/Order/Details?orderId=${data}"
                     class="btn btn-primary mx-2 ">
                        <i class="bi bi-pen"></i></a>
                </div>
                    `
                },
                "width": "15%"
            },
        ]
    });
}
//
// function Delete(url) {
//     Swal.fire({
//         title: 'Are you sure?',
//         text: "You won't be able to revert this!",
//         icon: 'warning',
//         showCancelButton: true,
//         confirmButtonColor: '#3085d6',
//         cancelButtonColor: '#d33',
//         confirmButtonText: 'Yes, delete it!'
//     }).then((result) => {
//         if (result.isConfirmed) {
//             $.ajax({
//                     url: url,
//                     type: "DELETE",
//                     success: function (data) {
//                         if (data.success) {
//                             datatable.ajax.reload();
//                             toastr.success(data.message)
//                         } else {
//                             toastr.error(data.message)
//                         }
//                     }
//                 }
//             )
//         }
//     })
// }