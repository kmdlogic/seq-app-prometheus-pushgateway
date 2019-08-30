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
       Description = "Filtered events are sent to the Prometheus Pushgateway.")]
    class SeqAppToPrometheusPushGateway : SeqApp, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
           DisplayName = "Pushgateway URL",
           HelpText = "The URL of the Pushgateway where seq events will be forwarded.")]
        public string PushgatewayUrl { get; set; }

        [SeqAppSetting(
            DisplayName = "Pushgateway Counter Name",
            HelpText = "Name of the counter with which this will be identified in the Pushgateway Metrics.")]
        public string CounterName { get; set; }

        [SeqAppSetting(
            DisplayName = "ApplicationNameKeyList",
            HelpText = "The names of additional event properties.",
            InputType = SettingInputType.LongText)]
        public string ApplicationNameKeySet { get; set; }

        public IMetricPushServer server;
        public readonly string instanceName = "default";

        protected override void OnAttached()
        {
            base.OnAttached();
            server = new MetricPushServer(new MetricPusher(PushgatewayUrl, CounterName, instanceName));
            server.Start();
        }

        public void On(Event<LogEventData> evt)
        {
            var additionalPropertiesList = SplitOnNewLine(this.ApplicationNameKeySet).ToList();
            var pushgatewayCounterData = FormatTemplate(evt, additionalPropertiesList);

            var counter = Metrics.CreateCounter(CounterName, "To keep the count of no of times a particular error coming in a module.", new[] { "ApplicationName", "Message" });
            counter.Labels(pushgatewayCounterData.ResourceName).Inc();
        }

        public static PushgatewayCounterData FormatTemplate(Event<LogEventData> evt, List<string> applicationNameKeyList)
        {
            var properties = (IDictionary<string, object>)ToDynamic(evt.Data.Properties ?? new Dictionary<string, object>());

            PushgatewayCounterData data = new PushgatewayCounterData();
            

            foreach (var propertyName in applicationNameKeyList)
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