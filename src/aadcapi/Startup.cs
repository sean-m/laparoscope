﻿/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using Microsoft.Owin;
using Microsoft.Configuration.ConfigurationBuilders;
using Owin;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using App.Metrics;
using App.Metrics.Extensions.Owin.WebApi;
using App.Metrics.Reporting.Abstractions;
using System.Diagnostics;

[assembly: OwinStartup(typeof(aadcapi.Startup))]

namespace aadcapi
{
    public partial class Startup
	{
		public void ConfigureServices(IServiceCollection services)
        {
			Trace.WriteLine(">> ConfigureServices() called!!!");
			services.AddLogging();
        }

		public void Configuration(IAppBuilder app)
		{
			Trace.WriteLine(">> Configuration() called!!!");
			var services = new ServiceCollection();
			ConfigureServices(services);



            ConfigureAuth(app);
			ConfigureAuthorizationFilters(app);
		}
	}
}