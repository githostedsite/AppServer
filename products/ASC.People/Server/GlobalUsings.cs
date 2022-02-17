﻿global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Net.Mail;
global using System.Runtime.InteropServices;
global using System.Security;
global using System.ServiceModel.Security;
global using System.Threading;
global using System.Web;

global using ASC.Api.Core;
global using ASC.Api.Utils;
global using ASC.Common;
global using ASC.Common.Mapping;
global using ASC.Common.Logging;
global using ASC.Common.Utils;
global using ASC.Common.Web;
global using ASC.Core;
global using ASC.Core.Common.Settings;
global using ASC.Core.Tenants;
global using ASC.Core.Users;
global using ASC.Data.Reassigns;
global using ASC.Employee.Core.Controllers;
global using ASC.FederatedLogin;
global using ASC.FederatedLogin.LoginProviders;
global using ASC.FederatedLogin.Profile;
global using ASC.MessagingSystem;
global using ASC.People;
global using ASC.People.ApiModels;
global using ASC.People.ApiModels.RequestDto;
global using ASC.People.ApiModels.ResponseDto;
global using ASC.People.Mapping;
global using ASC.People.Resources;
global using ASC.Security.Cryptography;
global using ASC.Web.Api.Models;
global using ASC.Web.Api.Routing;
global using ASC.Web.Core;
global using ASC.Web.Core.Mobile;
global using ASC.Web.Core.PublicResources;
global using ASC.Web.Core.Users;
global using ASC.Web.Studio.Core;
global using ASC.Web.Studio.Core.Notify;
global using ASC.Web.Studio.UserControls.Statistics;
global using ASC.Web.Studio.Utility;

global using Autofac;
global using Autofac.Extensions.DependencyInjection;

global using AutoMapper;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.Extensions;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting.WindowsServices;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;

global using SixLabors.ImageSharp;
global using SixLabors.ImageSharp.Formats;

global using StackExchange.Redis.Extensions.Core.Configuration;
global using StackExchange.Redis.Extensions.Newtonsoft;
