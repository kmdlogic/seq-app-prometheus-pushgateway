using Prometheus.Client;
using Prometheus.Client.MetricPusher;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
       Description = "\"CounterLabelKey\" data from filtered events are sent through Prometheus counters to the Pushgateway.")]
    public class SeqAppToPrometheusPushGateway : SeqApp, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
           DisplayName = "Pushgateway URL",
           HelpText = "The URL of the Pushgateway where seq events will be forwarded.")]
        public string PushgatewayUrl { get; set; }

        [SeqAppSetting(
             DisplayName = "Pushgateway Counter Name",
             HelpText = "Name of the Counter with which this will be identified in the Pushgateway Metrics.")]
        public string CounterName { get; set; }

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
            var counter = Metrics.CreateCounter(CounterName, "To keep the count of no of times a particular error coming in a module.");



            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri();
                var method = new HttpMethod("DELETE");
                var requestUri = $"{PushgatewayUrl}metrics/job/{CounterName}/instance/{instanceName}";
                var request = new HttpRequestMessage(method, requestUri);



                request.Headers
                                .Accept
                                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var resultApi = client.SendAsync(request).GetAwaiter().GetResult();
            }



            counter.Reset();
            counter.Inc();
        }
    }
}
