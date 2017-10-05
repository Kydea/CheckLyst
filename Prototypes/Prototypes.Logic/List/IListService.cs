using CheckListManager.Common.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> GetResponse(long Company, ListRequest form);
    }
}
