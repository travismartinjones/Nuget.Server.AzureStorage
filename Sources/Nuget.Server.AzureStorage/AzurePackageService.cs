﻿using NuGet;
using NuGet.Server;
using NuGet.Server.DataServices;
using NuGet.Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Web;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nuget.Server.AzureStorage.Domain.Services;
using System.IO;

namespace Nuget.Server.AzureStorage
{
    public class AzurePackageService : PackageService, IPackageService
    {
        private readonly AzureServerPackageRepository _azureRepository;

        public AzurePackageService(AzureServerPackageRepository repository,
                              IPackageAuthenticationService authenticationService)
            : base(repository, authenticationService)
        {
            _azureRepository = repository;
        }

        public void DownloadPackage(HttpContextBase context)
        {
            RouteData routeData = GetRouteData(context);
            // Get the package file name from the route
            string packageId = routeData.GetRequiredString("packageId");
            var version = new SemanticVersion(routeData.GetRequiredString("version"));

            string filename = packageId + "." + version.ToString() + ".nupkg";
            IPackage requestedPackage = _azureRepository.FindPackage(packageId, version);
            if (requestedPackage != null)
            {
                try
                {
                    var blob = _azureRepository.GetBlob(requestedPackage);
                    
                    if (!blob.Exists())
                    {
                        WritePackageNotFound(context, packageId, version);
                        return;
                    }

                    var ms = new MemoryStream();
                    blob.DownloadToStream(ms);

                    context.Response.Clear();
                    context.Response.AddHeader("content-disposition", $"attachment; filename={filename}");
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.BinaryWrite(ms.ToArray());
                    context.Response.Flush();
                    context.Response.SuppressContent = true;
                    context.Response.End();
                }
                catch (Exception exception)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(exception);                    
                    WritePackageNotFound(context, packageId, version);
                }
            }
            else
            {
                // Package not found
                WritePackageNotFound(context, packageId, version);
            }
        }

        private static void WritePackageNotFound(HttpContextBase context, string packageId, SemanticVersion version)
        {
            WriteStatus(context, HttpStatusCode.NotFound, String.Format("'Package {0} {1}' Not found.", packageId, version));
        }

        private static void WriteStatus(HttpContextBase context, HttpStatusCode statusCode, string body = null)
        {
            context.Response.StatusCode = (int)statusCode;
            if (!String.IsNullOrEmpty(body))
            {
                context.Response.StatusDescription = body;
            }
        }

        private RouteData GetRouteData(HttpContextBase context)
        {
            return RouteTable.Routes.GetRouteData(context);
        }
    }
}
