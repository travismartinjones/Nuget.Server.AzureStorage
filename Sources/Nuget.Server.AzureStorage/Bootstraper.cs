using System;
using System.Web;
using NuGet.Server.Publishing;
using AutoMapper;
using Elmah;
using Nuget.Server.AzureStorage.Domain.Services;
using Nuget.Server.AzureStorage.Doman.Entities;
using NuGet;
using NuGet.Server;
using NuGet.Server.Infrastructure;

namespace Nuget.Server.AzureStorage
{
    public static class AzureStorageStartup
    {
        public static void Startup()
        {
            ErrorLog.GetDefault(HttpContext.Current).Log(new Error(new Exception("Starting up")));
            SetUpMapper();
            ServiceResolver.SetServiceResolver(new AzureServiceResolver(ServiceResolver.Current));
        }

        public static void SetUpMapper()
        {
            Mapper.CreateMap<IPackage, AzurePackage>();
            Mapper.CreateMap<PackageDependencySet, AzurePackageDependencySet>()
                .ForMember(x => x.SeriazlizableDependencies, opt => opt.Ignore())
                .ForMember(x => x.SeriazlizableSupportedFrameworks, opt => opt.Ignore())
                .ForMember(x => x.SeriazlizableTargetFramework, opt => opt.Ignore());
        }
    }
}
