var dataTable;
function loadDataTable() {
/*    using the query selector, we need to get the id, using order index, where I have added tblData, we access that here, on that we will call the .DataTable() that is avaible because we configured datatable.net 
JS file in Layou.cshtmlt for datatables dotnet*, there we have few basic configurations, when we make the ajax calls
 what url should it go to, add that end point in order controller to retrive all the records.
 
 Final Conclusuion, when this method gets called in cshtml, using ajax calls, it retrived the data from end point
 and assingns the values to the table with id tblData.

 Note : As we are using dot net core with MVC, we have built in api supprort, where we access the end point
 */

    dataTable = $('#tblData').dataTable({
        "ajax": {
            url: "/order/getall"
        },
        //    next we need to defined the columns in our datable which is an array
        "columns": [
            { data: 'orderheaderid', 'width': "5%"},
            { data: 'email', 'width': "25%"},
        ]

    })
}