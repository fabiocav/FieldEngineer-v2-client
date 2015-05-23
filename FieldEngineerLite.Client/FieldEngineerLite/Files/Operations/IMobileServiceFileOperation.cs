using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files
{
    public interface IMobileServiceFileOperation
    {
        string FileId { get; }

        FileOperationState State { get; }

        Task Execute(IFileSyncContext context);

        void OnQueueingNewOperation(IMobileServiceFileOperation operation);
    }


}
