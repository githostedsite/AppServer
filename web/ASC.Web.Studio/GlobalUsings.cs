﻿global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Runtime.InteropServices;

global using ASC.Api.Core;
global using ASC.Common.Utils;
global using ASC.Data.Storage;
global using ASC.Data.Storage.DiscStorage;
global using ASC.FederatedLogin;
global using ASC.FederatedLogin.LoginProviders;
global using ASC.Web.Studio;

global using Autofac;
global using Autofac.Extensions.DependencyInjection;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Hosting.WindowsServices;