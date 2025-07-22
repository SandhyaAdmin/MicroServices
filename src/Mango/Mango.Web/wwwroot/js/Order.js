var dataTable;

/*invoke loadDataTable when the document is ready*/
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("approved")) {
        loadDataTable("approved")
    }
    else if (url.includes("readyforpickup")) {
        loadDataTable("readyforpickup")
    }
    else if (url.includes("cancelled")) {
        loadDataTable("cancelled")
    }
    else {
        loadDataTable("all")
    }
});
function loadDataTable(status) {
/*    using the query selector, we need to get the id, using order index, where I have added tblData, we access that here, on that we will call the .DataTable() that is avaible because we configured datatable.net 
JS file in Layou.cshtmlt for datatables dotnet*, there we have few basic configurations, when we make the ajax calls
 what url should it go to, add that end point in order controller to retrive all the records.
 
 Final Conclusuion, when this method gets called in cshtml, using ajax calls, it retrived the data from end point
 and assingns the values to the table with id tblData.

 Note : As we are using dot net core with MVC, we have built in api supprort, where we access the end point
 */

    dataTable = $('#tblData').DataTable({
        order:[[0,'desc']],
        "ajax": {
            url: "/order/getall?status=" + status,
            //type: "GET",
            //dataType: "json"
        },
        //    next we need to defined the columns in our datable which is an array
        "columns": [
            { data: 'orderHeaderId', 'width': "5%" },
            { data: 'email', 'width': "25%" },
            { data: 'name', 'width': "20%" },
            { data: 'phone', 'width': "10%" },
            { data: 'status', 'width': "10%" },
            { data: 'orderTotal', 'width': "10%" },
            {
                data: 'orderHeaderId',
                render: function (data) {
                    console.log("Rendering icon for order ID:", data); // Check console output

                    //here render custom html with some bootstap classes
                    return `
                    <div class="w-75 btn-group" role="group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2">
                    <i class="bi bi-pencil"></i>
                    </a>
                    </div>
                    `
                },
                width: "10%"

            }
        ]

    })
}