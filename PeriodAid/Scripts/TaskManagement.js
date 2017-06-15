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

    $("#add_subject_form").validate({
        debug: false,
        rules: {
            SubjectTitle: {
                required: true,
                maxlength: 128
            }
        },
        //验证通过的正式提交函数
        submitHandler: function (form) {
            $(form).ajaxSubmit(function (data) {
                if (data == "SUCCESS") {
                    alert("新建任务成功");
                    $("#btn_add_subject").removeClass("disabled");
                    $('#Add-Subject').modal('hide');
                    $('#Add-Subject').find(".form-group input,.form-group textarea").val("");
                } else {
                    $('#Add-Subject').find(".form-group input,.form-group textarea").val("");
                    $("#btn_add_subject").removeClass("disabled");
                    alert(data);
                }
            })
        },
        errorClass: "has-error",
        //验证失败
        errorPlacement: function (error, element) {
            $("#btn_add_subject").removeClass("disabled");
        }
    });
    $("#btn_add_subject").on("click", function () {
        if (!$(this).hasClass("disabled")) {
            $(this).addClass("disabled")
            $("#add_subject_form").submit();
        }
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