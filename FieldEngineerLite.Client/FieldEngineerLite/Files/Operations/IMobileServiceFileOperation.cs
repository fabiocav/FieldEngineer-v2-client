using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FieldEngineerLite.Files
{
    public interface IMobileServiceFileOperation
    {
        string FileId { get; }

        FileOperationKind Kind { get; }

        FileOperationState State { get; }

        Task Execute();

        void OnQueueingNewOperation(IMobileServiceFileOperation operation);
    }


}
