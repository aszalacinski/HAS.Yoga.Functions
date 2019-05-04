using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Yoga.Functions
{
    public static class Library
    {
        [FunctionName("LibraryAudio")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user-audio")] HttpRequest req,
            ILogger log)
        {
            List<string> fileNames = new List<string>();
            AudioDetails audioDetails = new AudioDetails();
            List<Audio> audios = new List<Audio>();

            var container = Utils.GetBlobContainer("audio");

            BlobContinuationToken continueToken = null;
            do
            {
                var results = await container.ListBlobsSegmentedAsync(null, continueToken);
                continueToken = results.ContinuationToken;

                var blobs = results.Results.Select(i => i.Uri.Segments.Last()).ToList();

                fileNames = blobs;

                foreach (var name in fileNames)
                {
                    Audio file = new Audio();

                    var blob = container.GetBlockBlobReference(name);
                    await blob.FetchAttributesAsync();

                    file.FileName = name;
                    file.Uri = blob.Uri;
                    file.Author = blob.Metadata["author"];
                    file.Description = blob.Metadata["description"];
                    file.Duration = blob.Metadata["duration"];
                    file.Title = blob.Metadata["title"];
                    file.Tags = blob.Metadata["tags"].Split(" ").ToList();

                    audios.Add(file);

                }

            } while (continueToken != null);

            audioDetails.Token = Utils.GetContainerSASToken(container);
            audioDetails.Audio = audios;

            return new JsonResult(audioDetails);
        }
    }
}
