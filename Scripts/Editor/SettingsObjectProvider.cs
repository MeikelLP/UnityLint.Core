using System;
using System.Linq;
using System.Linq.Expressions;
using Editor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine.Assertions;

namespace Editor
{
    public class SettingsObjectProvider : IServiceConfiguration
    {
        public void ConfigureServices(ServiceCollection services)
        {
            var method = typeof(ServiceCollectionServiceExtensions)
                .GetMethods()
                .Single(x => x.Name == nameof(ServiceCollectionServiceExtensions.AddScoped) &&
                             x.GetParameters().Length == 2 &&
                             x.GetGenericArguments().Length == 1 &&
                             x.IsGenericMethodDefinition);

            Assert.IsNotNull(method, nameof(method) + " != null");

            foreach (var (_, value) in LintingEngineSettingsProvider.Settings.AnalyzerSettings)
            {
                var genericMethod = method.MakeGenericMethod(value.GetType());

                // provider => setting
                var parameterExpression = Expression.Parameter(typeof(IServiceProvider), "provider");
                var constant = Expression.Constant(value, value.GetType());
                var lambda = Expression.Lambda(constant, parameterExpression);

                // services.AddScoped<IAnalyzerSettings>(provider => settings)
                var compile = lambda.Compile();
                genericMethod.Invoke(null, new object [] {services, compile});
            }

            services.AddScoped(x => x);
        }
    }
}
