using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Common.Core.Web.Security;
using System.Collections.Generic;
using System.Threading;
using CheckListManager.Common.Logic.Models;

namespace CheckListManager.Prototypes.Logic.Services
{
	public interface IPrototypeService : IService
	{
		Task<PrototypeResponse> OpenAsync(DefaultIdentity identity, PrototypeRequest request);
        Task<PrototypeResponse> CloneAsync(DefaultIdentity identity, PrototypeRequest request);
        Task<PrototypeResponse> CreateAsync(DefaultIdentity identity, PrototypeRequest request);
        Task CheckNewVersionAsync(DefaultIdentity identity, PrototypeRequest request);
        Task<PrototypeResponse> VersionAsync(DefaultIdentity identity, PrototypeRequest request);
        VerifyExpressionsResponse VerifyExpressions(PrototypeRequest request);
        Task<PrototypeResponse> SaveAsync(DefaultIdentity identity, PrototypeRequest request);
        Task DeleteAsync(DefaultIdentity identity, PrototypeRequest request);
        Task<PrototypeResponse> SetStateAsync(DefaultIdentity identity, PrototypeRequest request);
        Task<List<HistoryRow>> History(DefaultIdentity identity, PrototypeRequest request);
        Task UpdatePrototypeVersionsState(CancellationToken CancellationToken);
        Task<UploadAttachmentResponse> UploadAttachmentAsync(DefaultIdentity identity, UploadAttachmentRequest request);
        Task<StorageFile> DownloadAttachmentAsync(DefaultIdentity identity, ProcessAttachmentRequest request);
	}
}
