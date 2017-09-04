$(function () {


    //控制子任务展开(小图标)
    $("#my-app").on("click", ".tm-show-subtask-panel", function () {
        $(this).parents("li").find(".tm-subtask-ul").slideToggle(300);
        return false;
    })

    //控制view展示按钮     现在只有我的和日历    按钮必须存在.tm-show-view 以及 data-href自定义属性
    $("#my-app").on("click", ".tm-show-view", function () {
        if ($(this).hasClass("active")) {
            HiddenTmView();
            $(this).removeClass("active");
        } else {
            $(".tm-show-view").removeClass("active");
            $(this).addClass("active");
            //根据data-href地址获取相应模板
            ShowTmView($(this).attr("data-href"));
        }
    });

    //控制view隐藏
    $("#my-app").on("click", ".tm-hidden-view", function () {
        HiddenTmView();             //主要功能函数
        $(".tm-show-view").removeClass("active");   //消除按钮样式
    });

    //查看任务详情(包含子任务)
    $("#my-app").on("click", ".tm-show-task-modal", function (e) {
        var data = {};
        //判读任务是子任务还是任务
        if ($(this).attr("data-aid")) {
            data.AssignmentId = $(this).attr("data-aid");
            //请求任务详情
            GetTemplate("/TaskManagement/Assignment_Detail", data, function (data) {
                $("#Edit-Assignment .modal-content").html(data);
                $('#Edit-Assignment').modal('show');
            });
        } else if ($(this).attr("data-atid")) {
            data.SubTaskId = $(this).attr("data-atid");
            //请求子任务详情
            GetTemplate("/TaskManagement/Subtask_Detail", data, function (data) {
                $("#Edit-Assignment .modal-content").html(data);
                $('#Edit-Assignment').modal('show');
            });
        } else {
            ErrorAlert("获取原始数据失败。");
        }
        return false;

    });

    //任务完成状态回传(包含子任务)
    $("#my-app").on("click", ".tm-change-task-status", function () {
        var _self = $(this);
        var _data = {};
        var subjectdata={
            SubjectId:$("#panel-wrap").attr("data-sid")
        };
        if ($("#my-app").hasClass("tm-open-view")) {
            if ($(this).attr("data-aid")) {
                _data.AssignmentId = _self.attr("data-aid");
                EditForAjax("/TaskManagement/ComfirmFinishAssignment", _data, function (data) {
                    if ($(".tm-select-range.selected").attr("data-type") == "finish" || $(".tm-select-range.selected").attr("data-type") == "unfinish") {
                            _self.parents("li").remove();
                        }
                }, function () {
                    if (_self.hasClass("tm-checkbox-input")) {
                        _self.prop("checked", !_self.is(":checked"));
                    }
                });
            } else if (($(this).attr("data-atid"))) {
                _data.SubTaskId = $(this).attr("data-atid");
                EditForAjax("/TaskManagement/ComfirmFinishSubtask", _data, function (data) {
                        if ($(".tm-select-range.selected").attr("data-type") == "finish" || $(".tm-select-range.selected").attr("data-type") == "unfinish") {
                            _self.parents("li").remove();
                        }
                }, function () {
                    if (_self.hasClass("tm-checkbox-input")) {
                        _self.prop("checked", !_self.is(":checked"));
                    }
                });
            } else {
                ErrorAlert("获取原始数据失败。");
            }
            if (!_self.hasClass("tm-checkbox-input")) {
                return false;
            }
        } else {
            if ($(this).attr("data-aid")) {
                _data.AssignmentId = _self.attr("data-aid");
                EditForAjax("/TaskManagement/ComfirmFinishAssignment", _data, function (data) {
                    if ($("#panel-wrap").attr("data-sid")) {                      
                        updateTaskNum($("#panel-wrap").attr("data-sid"));
                        //更新任务对应的列表
                        var __data = {
                            ProcedureId: data.Id,
                            SubjectId: $("#panel-wrap").attr("data-sid")
                        }
                        getAssignment("/TaskManagement/SubjectAssignment", __data, $(".ajax-procedure[data-procedureid=" + __data.ProcedureId + "]").find(".tm_panel-body"), function (data) {
                            /*成功回调*/
                            dragtask("tm-list-show-" + __data.ProcedureId)
                        }, function (data) {
                            /*失败回调*/
                        });
                        //获取完成的任务
                        getAssignment("/TaskManagement/SubjectAssignment", { SubjectId: $("#panel-wrap").attr("data-sid") }, $("#tm-finsih-task"), function (data) {
                            //成功
                        }, function (data) {
                            //失败
                        })
                    }
                }, function () {
                   //失败

                });
            } else if (($(this).attr("data-atid"))) {
                _data.SubTaskId = $(this).attr("data-atid");
                EditForAjax("/TaskManagement/ComfirmFinishSubtask", _data, function (data) {
                    //修改子任务后改变任务状态
                    $("#Status").val(data.ParentStatus);
                    updateTaskNum($("#panel-wrap").attr("data-sid"));
                    //更新任务对应的列表
                    var __data = {
                        ProcedureId: data.Id,
                        SubjectId: $("#panel-wrap").attr("data-sid")
                    }
                    getAssignment("/TaskManagement/SubjectAssignment", __data, $(".ajax-procedure[data-procedureid=" + __data.ProcedureId + "]").find(".tm_panel-body"), function (data) {
                        /*成功回调*/
                        dragtask("tm-list-show-" + __data.ProcedureId)
                    }, function (data) {
                        /*失败回调*/
                    });
                    //获取完成的任务
                    getAssignment("/TaskManagement/SubjectAssignment", { SubjectId: $("#panel-wrap").attr("data-sid") }, $("#tm-finsih-task"), function (data) {
                        //成功
                    }, function (data) {
                        //失败
                    })
                }, function () {
                  //失败
                });
            } else {
                ErrorAlert("获取原始数据失败。");
            }
            if (!_self.hasClass("tm-checkbox-input")) {
                return false;
            }
        }
    });

    //请求项目表单事件
    $("#my-app").on("click", ".create_subject_target", function () {
        GetTemplate("/TaskManagement/GetSubjectForm", {}, function (data) {
            $("#Add-Subject #Create-Subject-Content").html(data);
            $("#Add-Subject").modal('show');
        });
    });

    //帮助卡片
    $("#bn_help").on("click", function () {
        $.ajax({
            url: "/TaskManagement/PersonalHelp",
            data: {
                SubjectId: $(this).attr("data-link")
            },
            success: function (data) {
                if (data != "FAIL") {
                    $("#Create-Subject-Content").html(data);
                    $("#Add-Subject").modal();
                }
                else {
                    ErrorAlert("获取项目基本信息失败。");
                }
            }
        })
    });
});

//get请求模板  url=请求地址  data=传入参数请直接给对象  callback成功回调函数 errback失败回调函数
//统一失败反馈,只要专注写回调函数即可
//后端返回条件 data失败必须传FAIL
function GetTemplate(url, data, callback, errback) {
    if (url == null || url == "") {
        ErrorAlert("请求地址不合法。");
    } else {
        $.ajax({
            url: url,
            data: data,
            success: function (data) {
                if (data == "FAIL") {
                    ErrorAlert("数据获取失败。");
                    if (errback && typeof (errback) == "function") {
                        errback(data)
                    }
                } else {
                    if (callback && typeof (callback) == "function") {
                        callback(data);
                    }
                }
            },
            error: function (data) {
                ErrorAlert("请求失败。");
                if (errback && typeof (errback) == "function") {
                    errback(data)
                }
            }
        });
    }
}

//post请求json  url=请求地址  data=传入参数请直接给对象  callback成功回调函数 errback失败回调函数
//统一失败反馈,只要专注写回调函数即可
//后端返回条件 data失败必须传result=="FAIL"
function EditForAjax(url, data, callback, errback) {
    if (url == null || url == "") {
        ErrorAlert("请求地址不合法。");
    } else {
        $.ajax({
            url: url,
            type: "post",
            data: data,
            success: function (data) {
                if (data.result == "FAIL") {
                    if (data.errmsg) {
                        ErrorAlert(data.errmsg);
                    } else {
                        ErrorAlert("获取数据失败。");
                    }
                    if (errback && typeof (errback) == "function") {
                        errback(data)
                    }
                } else {
                    if (data.msg) {
                        UnimportantAlert(data.msg);
                    }
                    if (callback && typeof (callback) == "function") {
                        callback(data);
                    }
                }
            },
            error: function () {
                ErrorAlert("请求失败。");
                if (errback && typeof (errback) == "function") {
                    errback(data)
                }
            }
        });
    }
}

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
}    //成功
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
}          //失败
function CustomConfirm2(text, callback, cancelcallback) {
    $.confirm({
        type: "red",
        alignMiddle: true,
        typeAnimated: true,
        content: text,
        buttons: {
            cancel: {
                text: "否",
                action: function () {
                    if (cancelcallback && typeof cancelcallback == "function") {
                        cancelcallback();
                    }
                }
            },
            confirm: {
                text: "是",
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
} //确认框

/*时间控件调用*/
function CompleteTimeWidget(inputName, container) {
    if (container) {
        $(inputName).datetimepicker({
            format: 'yyyy-mm-dd hh:00',
            container: container,
            minView: "day",
            autoclose: true,
            todayBtn: true,
            clearBtn: true,
            pickerPosition: "bottom-right",
            todayHighlighttodayHighlight: true,
        });
    } else {
 $(inputName).datetimepicker({
        format: 'yyyy-mm-dd hh:00',
        minView:"day",
        autoclose: true,
        todayBtn: true,
        clearBtn: true,
        pickerPosition: "bottom-right",
        todayHighlighttodayHighlight: true,
    });
    }
  
}

//tm-view控制  用于日历和我的的控制
function HiddenTmView(callback) {
    //对整体数据进行一次刷新
    //加载任务列表
    $(".ajax-procedure").each(function () {
        var that = $(this);
        var _data = {
            ProcedureId: that.attr("data-procedureid")
        }                                                //请求参数
        GetTemplate("/TaskManagement/SubjectProcedure", _data, function (data) {
            //成功
            that.html(data);                            //填入模板
            var __data = {
                ProcedureId: that.attr("data-procedureid"),
                SubjectId: $("#panel-wrap").attr("data-sid")
            }                                           //请求参数
            //获取任务
            getAssignment("/TaskManagement/SubjectAssignment", __data, that.find(".tm_panel-body"), function (data) {
                dragtask("tm-list-show-" + that.attr("data-procedureid")) //为任务注册拖动事件
            }, function (data) {
                //失败
            })
        }, function (data) {
            //失败
        })
    });
    $(".tm-view").slideUp(500, function () {
        $(".tm-view").remove();
        $("body").removeClass("tm-open-view");
        if (callback && typeof callback == "function") {
            callback();
        }
    });
}

function ShowTmView(url) {
        if ($(".tm-view").length > 0) {
            HiddenTmView(function () {
                //获取对应模板
                GetTemplate(url, {}, function (data) {
                    $("body>div:last").after(data);
                    $("body").addClass("tm-open-view");
                    $(".tm-view").slideDown(500);
                });
            });
        } else {
            //获取对应模板
            GetTemplate(url, {}, function (data) {
                $("body>div:last").after(data);
                $("body").addClass("tm-open-view");
                $(".tm-view").slideDown(500);
            });
        }
}

//任务拖动事件注册函数 id(ul的id)
function dragtask(id) {
    var el = document.getElementById(id)
    Sortable.create(el, {
        filter: ".tm_list-add",
        group: 'item-ul',      
        dragClass: "sortable-drag",
        ghostClass: "sortable-ghost",
        animation: 150,
        onEnd: function (evt) {
            //判读是否携带任务id
            if (!evt.item.dataset.aid) {
                ErrorAlert("操作失败")
            } else {
                if (!evt.item.offsetParent.offsetParent.dataset.procedureid) {
                    ErrorAlert("操作失败")
                } else {
                    EditForAjax("/TaskManagement/DragAssignment", {
                        AssignmentId: evt.item.dataset.aid,
                        nowpid: evt.item.offsetParent.offsetParent.dataset.procedureid
                    }, function (data) {
                        EditForAjax("/TaskManagement/GetProcedureJsonInfo", {
                            SubjectId: $("#panel-wrap").attr("data-sid")
                        }, function (data) {
                            var len = data.data.length;
                            $(".ajax-procedure").each(function () {
                                for (i = 0; i < len; i++) {
                                    if (data.data[i].ProcedureId == $(this).attr("data-procedureid")) {
                                        $(this).find(".total-num").html(data.data[i].TotalNum)
                                        $(this).find(".num").html(data.data[i].FinishNum)
                                    }
                                }
                            })
                        }, function (data) {
                            /*失败回调*/
                        })
                    }, function (data) {
                        /*失败回调*/
                    })
                }
            }
        },
    });
}

//获取任务函数
function getAssignment(url, data, container, success, error) {
    if (data.SubjectId) {
        /*获取任务*/
        GetTemplate(url, data, function (data) {
            container.html(data);
            if (success && typeof (success) == "function") {
                success(data);
            }
        }, function (data) {
            if (error && typeof (error) == "function") {
                error(data);
            }
        });
    }   
}

//获取任务参与人模板
function GetAssignment_CollaboratorPartial(AssignmentId, Callback) {
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

//获取任务评论模板
function GetComment(AssignmentId, Callback) {
    $.ajax({
        url: "/TaskManagement/Assignment_CommentPartial",
        data: {
            AssignmentId: AssignmentId
        },
        success: function (data) {
            typeof Callback && Callback(data);
        },
        error: function () {
            ErrorAlert("请求失败。");
        }
    })
}

//获取子任务模板
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

//任务面板任务数量更新
function updateTaskNum(SubejctId) {
    var subjectdata={
        SubjectId: SubejctId
    }
    EditForAjax("/TaskManagement/GetProcedureJsonInfo", subjectdata, function (data) {
        var len = data.data.length;
        var totalnum = 0;
        $(".ajax-procedure").each(function () {
            for (i = 0; i < len; i++) {
                if (data.data[i].ProcedureId == $(this).attr("data-procedureid")) {
                    $(this).find(".total-num").html(data.data[i].TotalNum)
                    $(this).find(".num").html(data.data[i].FinishNum)
                    totalnum += data.data[i].FinishNum;
                }
            }
        });
        $("#tm-finsh-total").html(totalnum)
    })
}

//请求活动中的项目  container=页面元素
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
//请求完成的项目 container=页面元素
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










