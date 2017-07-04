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
    }
}