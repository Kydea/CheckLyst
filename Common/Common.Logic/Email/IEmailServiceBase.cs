using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Email
{
    public interface IEmailServiceBase : IService
    {
        Task Send(EmailMessage message);
    }
}