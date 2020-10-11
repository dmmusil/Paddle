using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DomainTactics.Persistence;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Paddle.API
{
    public class BlobDocumentStorage : IDocumentStorage
    {
        private readonly CloudBlobClient _client;

        private readonly Dictionary<string, ICloudBlob> _blobLookup =
            new Dictionary<string, ICloudBlob>();

        public BlobDocumentStorage(CloudBlobClient client)
        {
            _client = client;
        }

        public async Task<T> Load<T>(string identifier)
        {
            var container = await EnsureContainer(identifier);
            var blob = container.GetBlockBlobReference(identifier.Split('/')[1]);

            if (await blob.ExistsAsync())
            {
                await using var stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                _blobLookup[identifier] = blob;
                return JsonConvert.DeserializeObject<T>(json);
            }

            throw new ArgumentOutOfRangeException(nameof(identifier));
        }


        public async Task Save(IHaveIdentifier document)
        {
            var json = JsonConvert.SerializeObject(document);
            var container = await EnsureContainer(document.Identifier);

            var blob =
                container.GetBlockBlobReference(document.Identifier.Split('/')[1]);
            await using var stream = new MemoryStream();
            await using var streamWriter = new StreamWriter(stream);
            streamWriter.Write(json);
            await streamWriter.FlushAsync();
            stream.Position = 0;
            if (await blob.ExistsAsync())
            {
                var etag = _blobLookup[document.Identifier].Properties.ETag;
                await blob.UploadFromStreamAsync(stream,
                    AccessCondition.GenerateIfMatchCondition(etag), null,
                    null);
            }
            else
            {
                await blob.UploadFromStreamAsync(stream);
            }
        }

        private async Task<CloudBlobContainer> EnsureContainer(string identifier)
        {
            var container = _client.GetContainerReference(identifier.Split('/')[0]);
            await container.CreateIfNotExistsAsync(
                BlobContainerPublicAccessType.Off, new BlobRequestOptions(),
                new OperationContext());
            return container;
        }
    }
}