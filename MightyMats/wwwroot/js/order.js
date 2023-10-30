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