using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FieldEngineerLite.Files.Metadata;

namespace FieldEngineerLite.Files.Sync
{
    public interface IFileSyncHandler
    {
        /// <summary>
        /// Gets the data source that will be used to retrieve the file data.
        /// </summary>
        /// <param name="metadata">A <see cref="MobileServiceFileMetadata"/> instance describing the target file.</param>
        /// <returns>A <see cref="IMobileServiceFileDataSource"/> that will be used to retrieve the file data.</returns>
        Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata);

        /// <summary>
        /// Invoked when, as a result of a synchronization operation, a file is created, updtaded or deleted.
        /// </summary>
        /// <param name="file">The <see cref="MobileServiceFile"/>.</param>
        /// <returns>A <see cref="Task"/> that is completed when the new file is processed.</returns>
        Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action);

        /// <summary>
        /// Invoked when, as a result of a synchronization operation, a new file is created.
        /// </summary>
        /// <param name="file">The newly created <see cref="MobileServiceFile"/>.</param>
        /// <returns>A <see cref="Task"/> that is completed when the new file is processed.</returns>
        Task ProcessNewFileAsync(MobileServiceFile file);

        /// <summary>
        /// Invoked when, as a result of a synchronization operation, a file is deleted.
        /// </summary>
        /// <param name="file">The deleted <see cref="MobileServiceFile"/>.</param>
        /// <returns>A <see cref="Task"/> that is completed when the file operation is processed.</returns>
        Task ProcessDeletedFileAsync(MobileServiceFile file);
    }

    public enum FileSynchronizationAction
    {
        Create,
        Update,
        Delete
    }
}
