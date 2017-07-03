$(function () {

    //请求项目表单
    $("#my-app").on("click", ".create_subject_target", function () {
        $.ajax({
            url: "/TaskManagement/GetSubjectForm",
            success: function (data) {
                $("#Add-Subject #Create-Subject-Content").html(data);
                $("#Add-Subject").modal('show');
            },
            error: function () {
                ErrorAlert("操作失败。")
            }
        });
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
            var byId = function (id) { return document.getElementById(id); };
            [].forEach.call(byId('tm_pannel-container').getElementsByClassName('tm_list-show'), function (el) {
                Sortable.create(el, {
                    filter: ".tm_list-add",
                    group: 'item',
                    dragClass: "sortable-drag",
                    ghostClass: "sortable-ghost",
                    animation: 150,
                    onEnd: function (evt) {
                        if (!evt.item.dataset.aid) {
                            ErrorAlert("操作失败")
                        } else {
                            if (!evt.item.offsetParent.offsetParent.dataset.procedureid) {
                                ErrorAlert("操作失败")
                            } else {
                                $.ajax({
                                    url:"/TaskManagement/DragAssignment",
                                    type:"post",
                                    data: {
                                        aid: evt.item.dataset.aid,
                                        nowpid: evt.item.offsetParent.offsetParent.dataset.procedureid
                                    },
                                    success: function (data) {
                                        if (data != "SUCCESS") {
                                            ErrorAlert("操作失败");
                                        } else {
                                            $.ajax({                                               
                                                url: "/TaskManagement/GetProcedureJsonInfo",
                                                type:"post",
                                                data: {
                                                    SubjectId: $("#pannel-wrap").attr("data-sid")
                                                },
                                                success: function (data) {
                                                    var len = data.result.length;
                                                    $(".ajax-procedure").each(function () {
                                                        for (i = 0; i < len; i++) {
                                                            if (data.result[i].ProcedureId == $(this).attr("data-procedureid")) {
                                                                
                                                                $(this).find(".total-num").html(data.result[i].TotalNum)
                                                                $(this).find(".num").html(data.result[i].FinishNum)
                                                            }
                                                        }
                                                    })
                                                }
                                            })
                                        }
                                    },
                                    error: function (data) {
                                        ErrorAlert("操作失败");
                                    }
                                })
                            }
                        }
                    },
                });
            });
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
function GetElementsByClass(className) {
            var elements;

            if (document.getElementsByClassName) {
                elements = document.getElementsByClassName(className);
            }
            else {
                var elArray = [];
                var tmp = document.getElementsByTagName(elements);
                var regex = new RegExp("(^|\\s)" + className + "(\\s|$)");
                for (var i = 0; i < tmp.length; i++) {

                    if (regex.test(tmp[i].className)) {
                        elArray.push(tmp[i]);
                    }
                }

                elements = elArray;
            }
            return elements
        }