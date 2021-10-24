using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure.Storage.Blobs;

namespace DDzia.Extensions.Configuration.AzureBlob.ClientProviders
{
    public class SasUriClientProvider : BlobClientProvider
    {
        private readonly Uri uri;

        public SasUriClientProvider(Uri uri)
        {
            this.uri = uri;
        }

        public override Task<BlobClient> Get(
            Uri blobUri,
            CancellationToken cancellationToken)
        {
            if (blobUri == null)
            {
                throw new ArgumentNullException(nameof(blobUri));
            }
            return Task.FromResult(new BlobContainerClient(this.uri).GetBlobClient(string.Join(string.Empty, blobUri.Segments.Skip(2))));
        }
    }
}