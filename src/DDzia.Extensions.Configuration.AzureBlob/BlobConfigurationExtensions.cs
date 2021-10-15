using System;
using DDzia.Extensions.Configuration.AzureBlob.Core;
using Microsoft.Extensions.Configuration;

namespace DDzia.Extensions.Configuration.AzureBlob
{
    public static class BlobConfigurationExtensions
    {
        public static IConfigurationBuilder AddBlobJson(this IConfigurationBuilder builder, BlobJsonConfigurationOption option)
        {
            return builder.Add(new BlobJsonConfigurationSource(option ?? throw new ArgumentNullException(nameof(builder))));
        }
    }
}
