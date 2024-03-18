var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    var status = getStatusFromUrl(url);

    loadDataTable(status);
});

function getStatusFromUrl(url) {
    if (url.includes("InProcess")) {
        return "InProcess";
    } else if (url.includes("Pending")) {
        return "Pending";
    } else if (url.includes("completed")) {
        return "completed";
    } else if (url.includes("Approved") || url.includes("approved")) {
        return "Approved";
    } else {
        return "all";
    }
}

function loadDataTable(status) {
    if ($.fn.DataTable.isDataTable('#tblTable')) {
        dataTable.ajax.url('/admin/order/getall?status=' + status).load();
    } else {
        dataTable = $('#tblTable').DataTable({
            "ajax": { url: '/admin/order/getall?status=' + status },
            "columns": [
                { data: 'id', "width": "10%" },
                { data: 'name', "width": "30%" },
                { data: 'phoneNumber', "width": "20%" },
                { data: 'applicationUser.email', "width": "10%" },
                { data: 'orderStatus', "width": "30%" },
                { data: 'orderTotal', "width": "30%" },
                {
                    data: 'id',
                    "render": function (data) {
                        return `<div class="w-75 btn-group" role="group">
                                    <a href="/admin/order/Details?orderId=${data}" class="btn btn-primary mx-2">
                                        <i class="bi bi-pencil-square"></i>Edit
                                    </a>
                                </div>`;
                    }, "width": "25"
                }
            ]
        });
    }
}





function Delete(url) {
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
            $.ajax(
                {
                    url: url,
                    type: 'DELETE',
                    success: function (data) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                }
            )
        }
    });
}



