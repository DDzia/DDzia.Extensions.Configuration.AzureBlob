using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace DDzia.Extensions.Configuration.AzureBlob.ClientProviders
{
    public abstract class BlobClientProvider
    {
        public abstract Task<BlobClient> Get(Uri blobUri, CancellationToken cancellationToken);
    }
}