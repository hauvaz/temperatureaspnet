using Metrics.Reporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Metrics.MetricData;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace RealtimeMeasurement.Infrastructure.Reporter
{
    public class InfluxDbReporter : MetricsReport
    {
        private readonly InfluxDBClient influxDbClient;
        private readonly string bucket;
        private readonly string org;

        public InfluxDbReporter(string url, string token, string org, string bucket)
        {
            this.bucket = bucket;
            this.org = org;
            this.influxDbClient = new InfluxDBClient(url, token); // ← string, без ToCharArray()
        }

        public async void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token)
        {
            var dataPoints = new List<PointData>();

            metricsData.Counters.ToList().ForEach(counter =>
                ProcessCounter(counter).ForEach(p => dataPoints.Add(p))
            );

            var writeApi = influxDbClient.GetWriteApiAsync(); // ← Без using
            await writeApi.WritePointsAsync(dataPoints, bucket, org);
        }

        private List<PointData> ProcessCounter(CounterValueSource counter)
        {
            var counterValue = counter.ValueProvider.GetValue(resetMetric: true);
            var points = new List<PointData>();

            if (counterValue.Items.Length > 0)
            {
                foreach (var item in counterValue.Items)
                {
                    points.Add(CreateDataPoint(counter.Name, (long)item.Count, CreateTags(item.Item)));
                }
            }
            else
            {
                points.Add(CreateDataPoint(counter.Name, (long)counterValue.Count, CreateTags()));
            }

            return points;
        }

        private Dictionary<string, string> CreateTags(string item = "")
        {
            var tags = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(item))
            {
                var parts = item.Split(':');
                var key = parts.Length > 1 ? parts[0] : "item";
                var value = parts.Length > 1 ? parts[1] : item;
                tags.Add(key, value);
            }

            return tags;
        }

        private PointData CreateDataPoint(string metricName, long metricValue, Dictionary<string, string> tags)
        {
            var point = PointData
                .Measurement(metricName)
                .Field("value", metricValue)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            foreach (var tag in tags)
            {
                point = point.Tag(tag.Key, tag.Value);
            }

            return point;
        }
    }
}
