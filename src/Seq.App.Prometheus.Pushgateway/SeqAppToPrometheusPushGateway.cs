using Prometheus.Client;
using Prometheus.Client.MetricPusher;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using Prometheus.Client.Collectors;
using Prometheus.Client.Collectors.Abstractions;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace Seq.App.Prometheus.Pushgateway
{
    [SeqApp("Seq.App.Prometheus.Pushgateway",
       Description = "\"GaugeLabelKey\" data from filtered events are sent through gauge to the Pushgateway.")]
    public class SeqAppToPrometheusPushGateway : SeqApp, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
           DisplayName = "Pushgateway URL",
           HelpText = "The URL of the Pushgateway where seq events will be forwarded.")]
        public string PushgatewayUrl { get; set; }

        [SeqAppSetting(
             DisplayName = "Pushgateway Counter Name",
             HelpText = "Name of the Gauge with which this will be identified in the Pushgateway Metrics.")]
        public string CounterName { get; set; }

        [SeqAppSetting(
            DisplayName = "Pushgateway Gauge Label Key",
            HelpText = "Pushgateway Gauge Label Key against which values from GaugeLabelValues will be set")]
        public string GaugeLabelKey { get; set; }

        [SeqAppSetting(
            DisplayName = "Pushgateway Gague Label Values",
            HelpText = "Gauge Label values to be tracked",
            InputType = SettingInputType.LongText)]
        public string GaugeLabelValues { get; set; }

        public IMetricPushServer server;
        public ICollectorRegistry registry;

        protected override void OnAttached()
        {
            base.OnAttached();
            registry = new CollectorRegistry();
            var customPusher = new MetricPusher(registry, PushgatewayUrl, CounterName, new Uri(PushgatewayUrl).Host, null, null);
            server = new MetricPushServer(customPusher);
            server.Start();
        }

        public void On(Event<LogEventData> evt)
        {
            var gaugeLabelValuesList = SplitOnNewLine(this.GaugeLabelValues).ToList();
            var pushgatewayCounterData = ApplicationNameKeyValueMapping(evt, gaugeLabelValuesList);

            var counter = Metrics.CreateCounter(CounterName, "To keep the count of no of times a particular error coming in a module.", new[] { GaugeLabelKey });
            counter.Labels(pushgatewayCounterData.ResourceName).Inc();

        }

        public static pushgatewayCounterData ApplicationNameKeyValueMapping(Event<LogEventData> evt, List<string> gaugeLabelValuesList)
        {
            var properties = (IDictionary<string, object>)ToDynamic(evt.Data.Properties ?? new Dictionary<string, object>());

            pushgatewayCounterData data = new pushgatewayCounterData();
            data.ResourceName = "ResourceNotFound";

            foreach (var propertyName in gaugeLabelValuesList)
            {
                var name = (propertyName).ToString().Trim();
                foreach (var property in properties)
                {
                    if (property.Key == name)
                    {
                        data.ResourceName = property.Value.ToString();
                        break;
                    }
                }
            }
            return data;
        }

        private static object ToDynamic(object o)
        {
            switch (o)
            {
                case IEnumerable<KeyValuePair<string, object>> dictionary:
                    var result = new ExpandoObject();
                    var asDict = (IDictionary<string, object>)result;
                    foreach (var kvp in dictionary)
                    {
                        asDict.Add(kvp.Key, ToDynamic(kvp.Value));
                    }
                    return result;

                case IEnumerable<object> enumerable:
                    return enumerable.Select(ToDynamic).ToArray();
            }

            return o;
        }

        private static IEnumerable<string> SplitOnNewLine(string addtionalProperties)
        {
            if (addtionalProperties == null)
            {
                yield break;
            }

            using (var reader = new StringReader(addtionalProperties))
            {
                yield return reader.ReadLine();
            }
        }
    }
}
