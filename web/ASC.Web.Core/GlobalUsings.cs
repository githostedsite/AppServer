﻿global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Data;
global using System.Diagnostics;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Net.Mail;
global using System.Net.Mime;
global using System.Reflection;
global using System.Runtime.Serialization;
global using System.Security;
global using System.Security.Authentication;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Web;
global using System.Xml;

global using ASC.Common;
global using ASC.Common.Caching;
global using ASC.Common.Logging;
global using ASC.Common.Notify.Engine;
global using ASC.Common.Security;
global using ASC.Common.Security.Authorizing;
global using ASC.Common.Threading;
global using ASC.Common.Utils;
global using ASC.Common.Web;
global using ASC.Core;
global using ASC.Core.Billing;
global using ASC.Core.Common;
global using ASC.Core.Common.Billing;
global using ASC.Core.Common.Configuration;
global using ASC.Core.Common.EF;
global using ASC.Core.Common.EF.Context;
global using ASC.Core.Common.EF.Model;
global using ASC.Core.Common.EF.Model.Mail;
global using ASC.Core.Common.Notify.Push;
global using ASC.Core.Common.Security;
global using ASC.Core.Common.Settings;
global using ASC.Core.Common.WhiteLabel;
global using ASC.Core.Notify;
global using ASC.Core.Tenants;
global using ASC.Core.Users;
global using ASC.Data.Storage;
global using ASC.FederatedLogin.LoginProviders;
global using ASC.FederatedLogin.Profile;
global using ASC.Feed;
global using ASC.Feed.Data;
global using ASC.MessagingSystem;
global using ASC.Notify;
global using ASC.Notify.Engine;
global using ASC.Notify.Messages;
global using ASC.Notify.Model;
global using ASC.Notify.Patterns;
global using ASC.Notify.Recipients;
global using ASC.Notify.Textile;
global using ASC.Security.Cryptography;
global using ASC.VoipService.Dao;
global using ASC.Web.Core;
global using ASC.Web.Core.Helpers;
global using ASC.Web.Core.ModuleManagement.Common;
global using ASC.Web.Core.Notify;
global using ASC.Web.Core.PublicResources;
global using ASC.Web.Core.Sms;
global using ASC.Web.Core.Subscriptions;
global using ASC.Web.Core.Users;
global using ASC.Web.Core.Utility;
global using ASC.Web.Core.Utility.Settings;
global using ASC.Web.Core.Utility.Skins;
global using ASC.Web.Core.WebZones;
global using ASC.Web.Core.WhiteLabel;
global using ASC.Web.Studio.Core;
global using ASC.Web.Studio.Core.Notify;
global using ASC.Web.Studio.UserControls.Management;
global using ASC.Web.Studio.UserControls.Statistics;
global using ASC.Web.Studio.Utility;

global using Google.Authenticator;
global using Google.Protobuf;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;

global using MimeKit.Utils;

global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;

global using SixLabors.ImageSharp;
global using SixLabors.ImageSharp.Drawing.Processing;
global using SixLabors.ImageSharp.Formats;
global using SixLabors.ImageSharp.Formats.Png;
global using SixLabors.ImageSharp.PixelFormats;
global using SixLabors.ImageSharp.Processing;

global using TMResourceData;

global using Twilio.Clients;
global using Twilio.Rest.Api.V2010.Account;
global using Twilio.Types;

global using static ASC.Web.Core.Files.DocumentService;

global using License = ASC.Core.Billing.License;
global using SecurityContext = ASC.Core.SecurityContext;
