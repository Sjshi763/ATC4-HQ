using System;
using System.IO;
using System.Management;
using System.Runtime.Versioning;

namespace ATC4_HQ.ViewModels
{
    [SupportedOSPlatform("Windows")] 
    public static class DriveTypeService
    {
        public static bool IsDriveSSD(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(path));
                if (!drive.IsReady) return false;

                using (ManagementObject disk = new ManagementObject(
                    $"Win32_LogicalDisk.DeviceID=\"{drive.Name.TrimEnd('\\')}\""))
                {
                    disk.Get();
                    string diskDriveId = disk["DeviceID"].ToString();

                    using (ManagementObject partition = new ManagementObject(
                        $"Win32_LogicalDiskToPartition.Antecedent=\"{diskDriveId}\""))
                    {
                        partition.Get();
                        string partitionDeviceId = partition["Dependent"].ToString();

                        using (ManagementObject physicalDisk = new ManagementObject(
                            $"Win32_DiskDrive.DeviceID=\"{partitionDeviceId}\""))
                        {
                            physicalDisk.Get();
                            string mediaType = physicalDisk["MediaType"]?.ToString() ?? "";
                            string model = physicalDisk["Model"]?.ToString() ?? "";

                            return mediaType.Contains("SSD") ||
                                   mediaType.Contains("Solid State") ||
                                   model.Contains("SSD");
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool HasHDD()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        string mediaType = disk["MediaType"]?.ToString() ?? "";
                        string model = disk["Model"]?.ToString() ?? "";

                        if (!mediaType.Contains("SSD") &&
                            !mediaType.Contains("Solid State") &&
                            !model.Contains("SSD") &&
                            mediaType != "Removable Media")
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
