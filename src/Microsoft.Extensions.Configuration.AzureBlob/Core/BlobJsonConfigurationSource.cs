using System;
using System.Threading;

using Microsoft.Extensions.Configuration.Json;

namespace Microsoft.Extensions.Configuration.AzureBlob.Core
{
    public class BlobJsonConfigurationSource : JsonConfigurationSource
    {
        internal BlobAccessor BlobAccessor { get; set; }
        internal BlobJsonConfigurationOption Option { get; set; }
        internal RemoteFileProvider RemoteFileProvider { get; private set; }

        public BlobJsonConfigurationSource(BlobJsonConfigurationOption option)
        {
            this.Option = option ?? throw new ArgumentNullException(nameof(option));

            BlobAccessor = new BlobAccessor(
                ct => option.ClientProvider.Get(option.BlobUri, ct),
                () => (option.FetchCancellationTokenFactory ?? (() => CancellationToken.None))());

            if (this.ReloadOnChange = option.PollingInterval.HasValue)
            {
                this.FileProvider = RemoteFileProvider = new RemoteFileProvider();
            }
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder) => new BlobJsonConfigurationProvider(this);
    }
}
