$(function () {
    //控制我的任务
    $("#my-app").on("click", ".tm-control-view-Panel", function () {
        if ($("#tm-my-view").length > 0) {
            $("#tm-my-view").fadeToggle();
            $("#my-app").removeClass(".tm-open-view");
        } else {
            $.ajax({
                url: "/TaskManagement/PersonalActionPanel",
                success: function (data) {
                    if (data =="FAIL") {
                        ErrorAlert("获取我的任务失败。")
                    } else {
                        $("#my-app").append(data).addClass(".tm-open-view");
                        $("#tm-my-view").fadeToggle();
                    }
                },
                error: function () {
                    ErrorAlert("请求失败。");
                }
            });
        }       
        })
    //请求项目表单
    $("#my-app").on("click", ".create_subject_target", function () {
        $.ajax({
            url: "/TaskManagement/GetSubjectForm",
            success: function (data) {
                if (data == "FAIL") {
                    ErrorAlert("操作失败。")
                } else {
                    $("#Add-Subject #Create-Subject-Content").html(data);
                    $("#Add-Subject").modal('show');
                }
            },
            error: function () {
                ErrorAlert("请求失败。")
            }
        });
    })
});

//请求活动中的项目
function GetActiveSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_ActiveSubjectListPartial",
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("读取执行中的项目失败。")
            } else {
                $(container).html(data);
            }
        }
    })
}
//请求完成的项目
function GetFinishSubject(container) {
    $.ajax({
        url: "/TaskManagement/Personal_FinishSubjectListPartial",
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("读取归档的项目失败。")
            } else {
                $(container).html(data);
            }
        }
    })
}

//获取项目的过程
function GetProcedure(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/SubjectProcedure",
        data: {
            ProcedureId: ProcedureId
        },
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("获取项目阶段失败。")
            } else {
                container.html(data)
                GetAssignment(ProcedureId, SubjectId, container.find(".tm_Panel-body"));
            }  
        },
        error: function () {
            ErrorAlert("操作失败。")
        }
    });
}

//获取任务表单
function GetAssignmentForm(ProcedureId, SubjectId, container) {
    $.ajax({
        url: "/TaskManagement/GetAssignmentForm",
        data: {
            ProcedureId: ProcedureId,
            SubjectId: SubjectId
        },
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("创建任务失败。")
            } else {
                container.html(data)
            }
        },
        error: function () {
            ErrorAlert("请求失败。")
        }
    });
}

//获取任务详情
function GetAssignmentDetail(AssignmentId, container, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_Detail",
        data: {
            AssignmentId: AssignmentId,
        },
        success: function (data) {
            if (data == "FAIL") {
                ErrorAlert("获取任务详情失败。")
            } else {
                container.html(data)
                if (Callback && typeof Callback == "function") {
                    Callback(data);
                }
            }        
        },
        error: function () {
            ErrorAlert("请求失败。")
        }
    });
}

//获取任务列表
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
            [].forEach.call(byId('tm_Panel-container').getElementsByClassName('tm_list-show'), function (el) {
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
                                        AssignmentId: evt.item.dataset.aid,
                                        nowpid: evt.item.offsetParent.offsetParent.dataset.procedureid
                                    },
                                    success: function (data) {
                                        if (data.result != "SUCCESS") {
                                            ErrorAlert(data.errmsg);
                                        } else {
                                            $.ajax({                                               
                                                url: "/TaskManagement/GetProcedureJsonInfo",
                                                type:"post",
                                                data: {
                                                    SubjectId: $("#Panel-wrap").attr("data-sid")
                                                },
                                                success: function (data) {
                                                    var len = data.data.length;
                                                    $(".ajax-procedure").each(function () {
                                                        for (i = 0; i < len; i++) {
                                                            if (data.data[i].ProcedureId == $(this).attr("data-procedureid")) {
                                                                
                                                                $(this).find(".total-num").html(data.data[i].TotalNum)
                                                                $(this).find(".num").html(data.data[i].FinishNum)
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

//确定完成任务
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
            ErrorAlert("操作失败。")
        }
    });
}
function Delete_Procedure(ProcedureId,SubjectId, Callback) {
    $.ajax({
        url: "/TaskManagement/Delete_Procedure",
        type: "post",
        data: {
            ProcedureId: ProcedureId,
            SubjectId:SubjectId
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
            ErrorAlert("操作失败。")
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
            ErrorAlert("操作失败。")
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
            ErrorAlert("操作失败。")
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
            ErrorAlert("操作失败。")
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
        format: 'yyyy-mm-dd hh:00',
        minView:"day",
        autoclose: true,
        todayBtn: true,
        clearBtn: true,
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

//var FuzzySearch = function () {
//    var Defaults = {
//        InputId:"password",
//    }
//    function Init(Options) {
//        var Options = $.extend({}, Defaults, Options);
//        console.log(Options)

//    }
//    return { Init: Init };
//}();
//FuzzySearch.Init({
//    InputId: "Name",

//});