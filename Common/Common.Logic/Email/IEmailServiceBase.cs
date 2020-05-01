using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Email
{
    public interface IEmailServiceBase : IService
    {
        Task LogInfo(string subject, string content);
        Task LogError(Exception exception, string subject = null);
    }
}