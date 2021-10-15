using System;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Configuration.AzureBlob.Core
{
    public sealed class RemoteFileProvider : IFileProvider
    {
        internal RemoteFileDelegatedChangeToken ChangeToken { get; private set; }

        public IFileInfo GetFileInfo(string subpath)
        {
            return null;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            return this.ChangeToken ??= new RemoteFileDelegatedChangeToken();
        }

        internal class RemoteFileDelegatedChangeToken : IChangeToken
        {
            private Action cbRoot = () => { };

            public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            {
                Action cb = () => callback(state);
                this.cbRoot = (Action) Delegate.Combine(this.cbRoot, cb);

                return new DelegatedDispose(() => this.cbRoot = (Action) Delegate.Remove(this.cbRoot, cb));
            }

            public bool HasChanged { get; private set; }
            public bool ActiveChangeCallbacks { get; private set; }

            internal void Changed()
            {
                this.HasChanged = true;
                ActiveChangeCallbacks = true;

                this.cbRoot();

                ActiveChangeCallbacks = false;
                this.HasChanged = false;
            }

            private class DelegatedDispose : IDisposable
            {
                private Action action;

                public DelegatedDispose(Action action)
                {
                    this.action = action;
                }

                public void Dispose()
                {
                    this.action();
                }
            }
        }
    }
}