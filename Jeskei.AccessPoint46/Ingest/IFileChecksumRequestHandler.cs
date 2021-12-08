namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IFileChecksumRequestHandler
    {
        List<IngestFileInfoDto> GetNextFilesToChecksum();
    }
}
