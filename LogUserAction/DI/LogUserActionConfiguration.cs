using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace LogUserAction
{
    public static class LogUserActionConfiguration
    {
        /// <summary>
        /// Add LogUserAction infrastructure to DI-services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLogUserAction(this IServiceCollection services)
        {
            // LogUserActionModel new instance on request liftime
            services.AddScoped<LogUserActionModel>();
            // ILogUserActionService new instance on request liftime
            services.AddScoped<ILogUserActionService, LogUserActionService>();

            // for find any LogContext service
            services.AddSingleton(services);

            return services;
        }
    }
}
