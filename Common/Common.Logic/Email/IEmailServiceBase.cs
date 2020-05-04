using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Email
{
    public interface IEmailServiceBase : IService
    {
        Task LogInfoAsync(string subject, string content);
        Task LogErrorAsync(Exception exception, string subject = null);
    }
}