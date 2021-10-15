using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Microsoft.Extensions.Configuration.Json;

namespace DDzia.Extensions.Configuration.AzureBlob.Core
{
    public class BlobJsonConfigurationProvider : JsonConfigurationProvider
    {
        private BlobJsonConfigurationSource source;
        private Timer timer;
        private ETag etag;
        private bool? exists;
        private int initialLoad;
        private int _reloadInProgress;

        public BlobJsonConfigurationProvider(BlobJsonConfigurationSource source) : base(source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));

            this.Load();

            this.ReloadOnChange();
        }

        public override void Load()
        {
            LoadAsync().Wait();
        }

        private void ReloadOnChange()
        {
            if (this.source.ReloadOnChange)
            {
                this.timer = new Timer(this.ReloadOnChange, null, this.source.Option.PollingInterval.Value, this.source.Option.PollingInterval.Value);
            }
        }

        private void ReloadOnChange(object _)
        {
            try
            {
                if (Interlocked.CompareExchange(ref _reloadInProgress, 1, 0) == 0)
                {
                    Load();
                }
            }
            catch (Exception ex)
            {
                source.Option.LogReloadException?.Invoke(ex);
            }
            finally
            {
                Interlocked.CompareExchange(ref _reloadInProgress, 0, 1);
            }
        }

        private async Task LoadAsync()
        {
            using (var ms = new MemoryStream())
            {
                Action notifyChanged = () =>
                {
                    if (Interlocked.CompareExchange(ref this.initialLoad, 1, 0) != 0)
                    {
                        source.Option.ActionOnReload?.Invoke();
                        this.source.RemoteFileProvider.ChangeToken.Changed();
                    }
                };

                Action loadStream = () =>
                {
                    ms.Position = 0;
                    base.Load(ms);
                };

                var (etagNew, updated, blobExists) = await source.BlobAccessor.RetrieveIfUpdated(ms, this.etag);

                if (Interlocked.CompareExchange(ref this.initialLoad, 1, 0) == 0)
                {
                    this.exists = blobExists;
                    if (blobExists)
                    {
                        this.etag = etagNew;
                        loadStream();
                    }
                    else
                    {
                        this.etag = default;
                    }
                }
                else if (this.exists != blobExists)
                {
                    this.exists = blobExists;
                    if (blobExists)
                    {
                        this.etag = etagNew;
                        loadStream();
                    }
                    else
                    {
                        this.etag = default;
                    }

                    notifyChanged();
                }
                else if (updated)
                {
                    this.etag = etagNew;
                    loadStream();
                    notifyChanged();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.timer?.Dispose();
                this.timer = null;
            }

            base.Dispose(disposing);
        }
    }
}
