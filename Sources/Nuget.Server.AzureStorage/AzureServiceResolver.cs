using System;
using System.Web;
using Elmah;
using Nuget.Server.AzureStorage.Domain.Services;
using NuGet.Server;
using NuGet.Server.Infrastructure;
using NuGet.Server.Publishing;

namespace Nuget.Server.AzureStorage
{
    public class AzureServiceResolver : IServiceResolver
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IServerPackageRepository _serverPackageRepository;
        private readonly IPackageService _packageService;

        public AzureServiceResolver(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
            var azureServerPackageRepository = new AzureServerPackageRepository(new AzurePackageLocator(), new AzurePackageSerializer());
            _serverPackageRepository = azureServerPackageRepository;
            _packageService = new AzurePackageService(azureServerPackageRepository, new PackageAuthenticationService()); 
        }

        public object Resolve(Type type)
        {

            ErrorLog.GetDefault(HttpContext.Current).Log(new Error(new Exception($"Resolve {type}")));

            if (type == typeof(IServerPackageRepository))
                return _serverPackageRepository;

            if (type == typeof(IPackageService))
                return _packageService;

            return _serviceResolver.Resolve(type);
        }
    }
}