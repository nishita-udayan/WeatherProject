$(document).ready(function () {
    var loggedInUser = sessionStorage.getItem('logid');

    viewdetails();
    function viewdetails() {
        $.ajax({
            url: '/Weather/GetSavedDetails',
            method: 'Get',
            data: {
                logid: loggedInUser
            },
            success: function (res) {
                if (res.success) {
                    $("#WeatherDetails_tbl tbody").empty()

                    $.each(res.data, function (index, item) {
                        var html = '<tr>' +
                            '<input type="hidden" id="weather_id" value=' + item.w_id +'>'+
                            '<td ><input type="number" id="min_temp" value='+ item.Minimum_temp.toFixed(2) + '></td>' +
                            '<td ><input type="number" id="max-temp" value=' + item.Maximum_temp.toFixed(2) + '></td>' +
                            '<td ><input type="number" id="whumidity" value=' + item.humidity + '></td>' +
                            '<td ><input type="date" id="wdate" value=' + item.date.split("T")[0] + '></td>' +
                            '</tr>';

                        $("#WeatherDetails_tbl tbody").append(html);
                    });

                }
            },
            error: function () {
                alert("Failed to load  data");
            }
        });
    }

    $("#weatherSearch").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#WeatherDetails_tbl tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    $("#btnupdate").click(function () {
        var weatherDataList = [];

        $("#WeatherDetails_tbl tbody tr").each(function () {
            var row = $(this);

            weatherDataList.push({
                w_id: row.find("#weather_id").val(),
                Minimum_temp: parseFloat(row.find("#min_temp").val()),
                Maximum_temp: parseFloat(row.find("#max-temp").val()),
                humidity: parseInt(row.find("#whumidity").val()),
                date: row.find("#wdate").val()
            });
        });

        $.ajax({
            url: '/Weather/UpdateWeatherDetails',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(weatherDataList),
            success: function (res) {
                if (res.success) {
                    swal("Updated successfully!");
                    viewdetails()
                }
            }
        });
    });

});