using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using Metrics;
using RealtimeMeasurement.Infrastructure.Reporter;
using RealtimeMeasurement.Web.App_Start;
using Microsoft.Practices.Unity.Mvc;

namespace RealtimeMeasurement.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Регистрация стандартных маршрутов и Web API
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Чтение параметров подключения к InfluxDB из web.config
            string influxUrl = ConfigurationManager.AppSettings["InfluxUrl"];
            string influxToken = ConfigurationManager.AppSettings["InfluxToken"];
            string influxOrg = ConfigurationManager.AppSettings["InfluxOrg"];
            string influxBucket = ConfigurationManager.AppSettings["InfluxBucket"];

            // Конфигурация Metrics.NET и репортера
            Metric.Config
                .WithHttpEndpoint("http://localhost:1234/") // Метрика доступна по этому адресу
                .WithReporting(reports =>
                {
                    reports.WithReport(
                        new InfluxDbReporter(influxUrl, influxToken, influxOrg, influxBucket),
                        TimeSpan.FromSeconds(5)
                    );
                })
                .WithAllCounters();
        }
    }
}
