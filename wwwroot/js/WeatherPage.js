$(document).ready(function () {

    $("#drpcity").change(function () {
        const city = $(this).val();
        if (!city) return;

        $.ajax({
            url: '/Weather/GetWeatherDetails',
            method: 'Get',
            data: {
               city:city
            },
            success: function (res) {
                $("#Weather_tbl tbody").empty()

                var html = '<tr>'+
                         '<td id="txtmintemp">'+res.temperatureMin+'</td>'+
                         '<td id="txtmaxtemp">'+res.temperatureMax+'</td>'+
                        '<td id="txthumidity">' + res.humidity + '</td>'+
                         '</tr>';

                $("#Weather_tbl tbody").append(html);
            },
            error: function () {
                alert("Failed to load  data");
            }
        });
    })
    var loggedInUser = sessionStorage.getItem('logid');

    $("#btnSaveWeather").click(function () {
        $.ajax({
            url: '/Weather/SaveWeather',
            method: 'Post',
            data: {
                temp_min: $("#txtmintemp").text(),
                temp_max: $("#txtmaxtemp").text(),
                humidity: $("#txthumidity").text(),
                logid: loggedInUser
            },
            success: function (res) {
                if (res.success) {
                    swal("Successfully Added!!!")
                    $("#Weather_tbl tbody").empty()
                }
            }
        })
    })

    $("#btnweatherDetails").click(function () {
        window.location.href = "/Weather/Weather_details";
    })
        
    
})