$(function () {
    GetStarSubject("#personal_star_subject")
    GetActiveSubject("#personal_active_subject")
    GetFinishSubject("#personal_finish_subject")
    $("#personal_active_subject").on("click", ".card-item", function () {
        var linkid = $(this).attr("data-link")
        if (linkid) {
            location.href = "/TaskManagement/Subject_Detail?SubjectId=" + linkid
        }
    })
    $("#personal_active_subject").on("click", ".card-header-right", function () {
        alert(1);
        return false;
    })

});


//请求
function GetStarSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_StarSubjectListPartial",
        success: function (data) {
            $(container).html(data);
        }
    })
}
function GetActiveSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_ActiveSubjectListPartial",
        success: function (data) {
            $(container).html(data);
        }
    })
}
function GetFinishSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_FinishSubjectListPartial",
        success: function (data) {
            $(container).html(data);
        }
    })
}