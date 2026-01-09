$(document).ready(function () {

    $("#loginform").submit(function (e) {
        e.preventDefault();
        var user_name = $("#txtusername").val();
        var pass_word = $("#txtpassword").val();

        $.ajax({
            url: '/Login/LoginUser',
            method: 'Post',
            data: {
                username: user_name,
                password: pass_word
            },
            success: function (res) {
                if (res.success) {
                    var logid = res.data;
                    sessionStorage.setItem("logid", logid);
                    window.location.href = "/Weather/Weather_Page";
                }

            }
        });

    })

    $("#btnweatherDetails").click(function () {
        window.location.href = "/Weather/Weather_Page";
    })
})