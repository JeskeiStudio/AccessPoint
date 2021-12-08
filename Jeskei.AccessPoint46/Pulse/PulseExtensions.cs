namespace Jeskei.AccessPoint.Ingest.Pulse
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    internal static class PulseExtensions
    {
        public static XElement ToDriveXml(this DriveInfo driveInfo)
        {
            return new XElement(
                "Drive",
                    new XAttribute("name", driveInfo.Name),
                    new XAttribute("available-space", driveInfo.AvailableFreeSpace),
                    new XAttribute("total-size", driveInfo.TotalSize),
                    new XAttribute("drive-format", driveInfo.DriveFormat),
                    new XAttribute("volume-label", driveInfo.VolumeLabel));
        }

        public static XElement ToFolderXml(this DirectoryInfo root)
        {
            XElement dirXml = new XElement("FolderXml", new XAttribute("root-path", root.FullName));
            RecurseDirectory(root, dirXml);

            return dirXml;
        }

        private static void RecurseDirectory(DirectoryInfo di, XElement element)
        {
            XElement dirlist = new XElement(
                "Folder",
                    new XAttribute("name", di.Name),
                    new XAttribute("last-write-time", di.LastWriteTimeUtc),
                    new XAttribute("creation-time", di.CreationTimeUtc),
                    new XAttribute("last-access-time", di.LastAccessTimeUtc));

            try
            {
                foreach (DirectoryInfo subDir in di.GetDirectories())
                {
                    RecurseDirectory(subDir, dirlist);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // TODO: don't have access to this folder, so ignore?
            }

            foreach (FileInfo fi in di.GetFiles())
            {
                fi.Refresh();
                dirlist.Add(
                    new XElement(
                        "File",
                            new XAttribute("name", fi.Name),
                            new XAttribute("full-name", fi.FullName),
                            new XAttribute("length", fi.Length),
                            new XAttribute("last-write-time", fi.LastWriteTimeUtc),
                            new XAttribute("creation-time", fi.CreationTimeUtc),
                            new XAttribute("last-access-time", fi.LastAccessTimeUtc)));
            }

            element.Add(dirlist);
        }
    }
}
