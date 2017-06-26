$(function () {
    //新增项目验证
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
                    UnimportantAlert("添加项目成功");
                    $("#btn_add_subject").removeClass("disabled");
                    $('#Add-Subject').modal('hide');
                    GetActiveSubject("#personal_active_subject")
                    $('#Add-Subject').find(".form-group input,.form-group textarea").val("");
                } else {
                    ErrorAlert(data);
                    $('#Add-Subject').find(".form-group input,.form-group textarea").val("");
                    $("#btn_add_subject").removeClass("disabled");
                }
            }, function (data) {
                ErrorAlert("操作失败");
                $("#btn_add_subject").removeClass("disabled");
            })
        },
        errorClass: "has-error",
        //验证失败
        errorPlacement: function (error, element) {
            $("#btn_add_subject").removeClass("disabled");
        }
    });
    //新增项目的提交
    $("#btn_add_subject").on("click", function () {
        if (!$(this).hasClass("disabled")) {
            $(this).addClass("disabled")
            $("#add_subject_form").submit();
        }
    });
    

    //新增任务
    $("#add_assignment_form").validate({
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
                    UnimportantAlert("添加任务成功");
                    $("#btn_add_assignment").removeClass("disabled");
                    $('#Add-Assignment').modal('hide');
                    $('#Add-Assignment').find(".form-group input,.form-group textarea").val("");
                } else {
                    ErrorAlert(data);
                    $('#Add-Assignment').find(".form-group input,.form-group textarea").val("");
                    $("#btn_add_assignment").removeClass("disabled");
                }
            })
        },
        errorClass: "has-error",
        //验证失败
        errorPlacement: function (error, element) {
            $("#btn_add_assignment").removeClass("disabled");
        }
    });
    //新增任务的提交
    $("#btn_add_assignment").on("click", function () {
        if (!$(this).hasClass("disabled")) {
            $(this).addClass("disabled")
            $("#add_assignment_form").submit();
        }
    });

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
function GetProcedure(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/SubjectProcedure",
        data: {
            ProcedureId: ProcedureId
        },
        success: function (data) {
            container.html(data)
            GetAssignment(ProcedureId, SubjectId, container.find(".tm_pannel-body"));
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}
function GetAssignmentForm(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/GetAssignmentForm",
        data: {
            ProcedureId: ProcedureId,
            SubjectId: SubjectId
        },
        success: function (data) {
            container.html(data)
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}
function GetAssignmentDetail(AssignmentId, container, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_Detail",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            container.html(data)
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}
function GetAssignment(ProcedureId, SubJectId, container) {
    $.ajax({
        url: "/TaskManagement/SubjectAssignment",
        data: {
            ProcedureId: ProcedureId,
            SubJectId: SubJectId
        },
        success: function (data) {
            container.html(data)
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}
function ComfirmFinishAssignment(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/ComfirmFinishAssignment",
        type: "post",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}
function Delete_Procedure(ProcedureId, Callback) {
    $.ajax({
        url: "/TaskManagement/Delete_Procedure",
        type: "post",
        data: {
            ProcedureId: ProcedureId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function (data) {
            ErrorAlert("操作失败")
        }
    });
}
function Delete_Assignment(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Delete_Assignment",
        type: "post",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function (data) {
            ErrorAlert("操作失败")
        }
    });
}
function GetAssignmnet_CollaboratorAddPartial(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignmnet_CollaboratorAddPartial",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}

function GetAssignment_CollaboratorPartial(AssignmentId,Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_CollaboratorPartial",
        data: {
            AssignmentId: AssignmentId
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}
function GetSubTaskListPartial(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_SubtaskPartial",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (Callback && typeof Callback == "function") {
                Callback(data);
            }
        },
        error: function () {
            ErrorAlert("操作失败")
        }
    });
}



//插件调用
/*dialog调用*/
function UnimportantAlert(text) {
    $.alert({
        autoClose: 'cancelAction|2000',
        backgroundDismiss: true,
        escapeKey: 'cancelAction',
        typeAnimated: true,
        content: text,
        buttons: {
            cancelAction: {
                text: '关闭',
            }
        }
    });
}
function ErrorAlert(text) {
    $.alert({
        type: "red",
        autoClose: 'cancelAction|10000',
        escapeKey: 'cancelAction',
        typeAnimated: true,
        content: text,
        buttons: {
            cancelAction: {
                text: '关闭',
            }
        }
    });
}
function CustomConfirm(text,callback) {
    $.confirm({
        type: "red",
        alignMiddle: true,
        typeAnimated: true,
        content: text,
        buttons: {
            cancel: {
                text: "取消"
            },
            confirm: {
                text:"确定",
                btnClass: 'btn-red',
                action: function () {
                    if (callback && typeof callback == "function") {
                        callback();
                    }
                }
            },
        }
    });
}

/*时间控件调用*/
function CompleteTimeWidget(cotainer) {
    $(cotainer).datetimepicker({
        format: 'yyyy-mm-dd',
        autoclose: true,
        todayBtn: true,
        clearBtn: true,
        minView: "2",
        maxView:"2",
        pickerPosition: "bottom-right",
        todayHighlighttodayHighlight: true,
    });
}