$(function () {
    $("#start-date").datepicker();
    $("#end-date").datepicker();
    /*start downloadsalary*/
    $("#download-salary").click(function () {
        $("#downloadModal").modal();
    });
    $("#download-submit").click(function () {
        var start = $("#start-date").val();
        var end = $("#end-date").val();
        window.location.href = "/OfflineSales/Ajax_DownloadSalary?start=" + start + "&end=" + end;
        return false;
    });
    /*-------end--------*/
    /*start left navbar*/
    var left_nav = $("#current_page").val();
    $(".panel-collapse").removeClass("in");
    $(".leftnav-arrow").removeClass("panel-animate")
    $("div[aria-labelledby='" + left_nav + "']").addClass("in");
    $("i[aria-labelledby='" + left_nav + "']").addClass("panel-animate")
    /*------end------*/
    /*start scroll fixed*/
    $(document).scroll(function () {
        if (window.pageYOffset > 0) {
            $("#to-top").removeClass("hidden");
        }
        else
            $("#to-top").addClass("hidden");
    })
    $("#to-top").click(function () {
        scoll_ani();
    });
    function scoll_ani() {
        if (window.pageYOffset > 0) {
            $(document).scrollTop(window.pageYOffset - 50);
            setTimeout(scoll_ani, 50);
        }
    }
    /*------end------*/
});