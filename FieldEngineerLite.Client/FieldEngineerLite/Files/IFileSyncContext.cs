using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files
{
    public interface IFileSyncContext
    {
        Task AddFileAsync(MobileServiceFile file);

        Task<bool> QueueOperationAsync(IMobileServiceFileOperation operation);

        Task PushChangesAsync(CancellationToken cancellationToken);

        Task DeleteFileAsync(MobileServiceFile file);
    }
}
