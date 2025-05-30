using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using RealtimeMeasurement.Infrastructure.Metrics; // ⬅ добавь это
using RealtimeMeasurement.Infrastructure.Metrics; // если реализация там

namespace RealtimeMeasurement.Web.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        public static void RegisterTypes(IUnityContainer container)
        {
            // 👇 Добавь реализацию интерфейса
            container.RegisterType<IWebsiteMetric, WebsiteMetric>();
        }
    }
}
