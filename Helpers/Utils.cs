using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace HAS.Yoga.Functions
{
    public static class Utils
    {
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static CloudBlobContainer GetBlobContainer(string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(GetEnvironmentVariable("AzureWebJobsStorage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            return container;
        }

        public static string GetContainerSASToken(CloudBlobContainer container, string policyName = null)
        {
            string token;

            if (policyName == null)
            {
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(60),
                    Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read
                };

                token = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                token = container.GetSharedAccessSignature(null, policyName);
            }

            return token;
        }
    }
}
