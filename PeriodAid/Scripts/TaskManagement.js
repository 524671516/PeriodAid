$(function () {
    GetStarTask("#personal_star_task")
    GetActiveTask("#personal_active_task")
    GetFinishTask("#personal_finish_task")
    console.log(1);
    $("#personal_active_task").on("click", ".card-item", function () {
        var linkid = $(this).attr("data-link")
        if (linkid) {
            location.href = "/TaskManagement/Task_Detail?TaskId=" + linkid
        }
    })
    $("#personal_active_task").on("click", ".card-header-right", function () {
        alert(1);
        return false;
    })

});
function GetStarTask(container) {
    $.ajax({
        url: "/TaskManagement/Personal_StarTaskListPartial",
        success: function (data) {
            $(container).html(data);
        }
    })
}
function GetActiveTask(container) {
    $.ajax({
        url: "/TaskManagement/Personal_ActiveTaskListPartial",
        success: function (data) {
            $(container).html(data);
        }
    })
}
function GetFinishTask(container) {
    $.ajax({
        url: "/TaskManagement/Personal_FinishTaskListPartial",
        success: function (data) {
            $(container).html(data);
        }
    })
}