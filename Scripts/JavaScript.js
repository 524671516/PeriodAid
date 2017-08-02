$(function () {
    var device_width = $(document).width();
    if (device_width > 640) {
        $(".container-body").width(640);
    }
    $(".input-validation-error").closest(".form-group").addClass("has-error");
    var nav_name = $("#nav-name").text();
    
    if (nav_name == "User") {
        $("#nav-user").addClass("activ");
    }
    else if (nav_name == "Event") {
        $("#nav-event").addClass("activ");
    }
    if (nav_name == "Calendar") {
        $("#nav-calendar").addClass("activ");
    }
})