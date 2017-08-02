using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PeriodAid.Models;
using System.Threading.Tasks;

namespace PeriodAid.DAL
{
    public class TaskManagementLogsUtilities
    {
        private ProjectSchemeModels _db { get; set; }
        public TaskManagementLogsUtilities()
        {
            _db = new ProjectSchemeModels();
        }
        // 添加项目
        public async Task<bool> CreateSubjectLogAsync(Employee user,Subject subject)
        {
            try
            {
                OperationLogs item = new OperationLogs()
                {
                    SubjectId=subject.Id,
                    UserId=user.Id,
                    LogCode=LogCode.CREATESUBJECT,
                    LogContent="添加了项目:"+subject.SubjectTitle,
                    LogTime=DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 修改项目
        public async Task<bool> EditSubjectLogAsync(Employee user, Subject subject)
        {
            try
            {
                OperationLogs item = new OperationLogs()
                {

                    SubjectId = subject.Id,
                    UserId = user.Id,
                    LogCode = LogCode.EDITSUBJECT,
                    LogContent = "修改了项目:" + subject.SubjectTitle,
                    LogTime = DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 添加任务
        public async Task<bool> CreateTaskLogAsync(Employee user, Subject subject) {

            try
            {
                OperationLogs item = new OperationLogs()
                {
                    SubjectId = subject.Id,
                    UserId = user.Id,
                    LogCode = LogCode.EDITSUBJECT,
                    LogContent = "添加了任务:" + subject.SubjectTitle,
                    LogTime = DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch {
                return false;
            }
        }

        // 修改任务
        public async Task<bool> EditTaskLogAsync(Employee user, Subject subject)
        {
            try
            {
                OperationLogs item = new OperationLogs()
                {
                    SubjectId = subject.Id,
                    UserId = user.Id,
                    LogCode = LogCode.EDITTASK,
                    LogContent = "修改了任务:" + subject.SubjectTitle,
                    LogTime = DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //创建子任务
        public async Task<bool> CreateSubTaskLogAsync(Employee user, Subject subject) {
            try
            {
                OperationLogs item = new OperationLogs()
                {
                    SubjectId = subject.Id,
                    UserId = user.Id,
                    LogCode = LogCode.CREATESUBTASK,
                    LogContent = "创建了子任务:" + subject.SubjectTitle,
                    LogTime = DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch {
                return false;
            }
        }


        //修改子任务
        public async Task<bool> EditSubTaskLogAsync(Employee user, Subject subject)
        {
            try
            {
                OperationLogs item = new OperationLogs()
                {
                    SubjectId = subject.Id,
                    UserId = user.Id,
                    LogCode = LogCode.EDITSUBTASK,
                    LogContent = "修改了子任务:" + subject.SubjectTitle,
                    LogTime = DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //修改个人信息
        public async Task<bool> EditPersonalInfor(Employee user, Subject subject) {
            try
            {
                OperationLogs item = new OperationLogs()
                {
                    SubjectId = subject.Id,
                    UserId = user.Id,
                    LogCode = LogCode.EDITPERSONALINFO,
                    LogContent = "修改了个人信息。",
                    LogTime = DateTime.Now
                };
                _db.OperationLogs.Add(item);
                await _db.SaveChangesAsync();
                return true;
            }
            catch {
                return false;
            }
        }
    }
}