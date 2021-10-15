using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DDzia.Extensions.Configuration.AzureBlob.Core
{
    internal sealed class BlobAccessor
    {
        private readonly Func<CancellationToken, Task<BlobClient>> blobClientFactory;
        private readonly Func<CancellationToken> fetchCancellationTokenFactory;

        public BlobAccessor(
            Func<CancellationToken, Task<BlobClient>> blobClientFactory,
            Func<CancellationToken> fetchCancellationTokenFactory)
        {
            this.blobClientFactory = blobClientFactory ?? throw new ArgumentNullException(nameof(blobClientFactory));
            this.fetchCancellationTokenFactory = fetchCancellationTokenFactory ?? throw new ArgumentNullException(nameof(fetchCancellationTokenFactory));
        }

        public async Task<(ETag, bool updated, bool exists)> RetrieveIfUpdated(MemoryStream ms, ETag eTag)
        {
            if (ms == null)
            {
                throw new ArgumentNullException(nameof(ms));
            }

            var ct = this.fetchCancellationTokenFactory();

            var blobClient = await this.blobClientFactory(ct)
                .ConfigureAwait(false);

            Response<BlobProperties> properties;
            try
            {
                properties = await blobClient.GetPropertiesAsync(new BlobRequestConditions(), ct)
                    .ConfigureAwait(false);
            }
            catch (RequestFailedException e) when (e.Status == 404 || e.ErrorCode == "BlobNotFound")
            {
                return (default, eTag != default, false);
            }

            if (properties.Value.ETag == eTag)
            {
                return (properties.Value.ETag, false, true);
            }

            await blobClient.DownloadToAsync(ms, ct)
                .ConfigureAwait(false);
            return (properties.Value.ETag, true, true);
        }
    }
}
