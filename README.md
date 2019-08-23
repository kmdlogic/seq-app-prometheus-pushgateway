# Seq.App.Prometheus.Pushgateway

A Seq app that pushes events to [Prometheus Pushgateway](https://github.com/prometheus/pushgateway). **Requires Seq 5.1+.**

### Getting started

The app is published to NuGet as [_Seq.App.Prometheus.Pushgateway_](https://nuget.org/packages/Seq.App.Prometheus.Pushgateway). Follow the instructions for [installing a Seq App](https://docs.getseq.net/docs/installing-seq-apps) and start an instance of the app, providing your details.
* Based on the signals we select for the events Seq will send those events to SeqApp.
* SeqApp will send Prometheus counters for those events to the PushgatewayUrl.
* Data from filtered events are sent through gauge to the Pushgateway

## Procedure :
* SeqApps are implemented in .Net class library
* Install the Seq.Apps Nuget package
* The input attributes for the SeqApp can be written as 
```query
[SeqApp("Seq.App.Prometheus",
        Description = "Filtered events are sent to the Prometheus Pushgateway.")]
    public class SeqToPushGateway : SeqApp, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
            DisplayName = "Pushgateway URL",
            HelpText = "The URL of the Pushgateway where seq events will be forwarded.")]
        public string PushgatewayUrl { get; set; }
```
* Logic is written in `on` method
```query
   public void On(Event<LogEventData> evt)
        {
            var gaugeLabelValuesList = SplitOnNewLine(this.GaugeLabelValues).ToList();
            var pushgatewayGaugeData = ApplicationNameKeyValueMapping(evt, gaugeLabelValuesList);

            var customGauge = Metrics.WithCustomRegistry(registry).CreateGauge(GaugeName, "To track the Seq events based on the applied signal", new[] { GaugeLabelKey, "EventTimestamp" });
            var gaugeValue = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
         
            customGauge.Labels(pushgatewayGaugeData.ResourceName, evt.TimestampUtc.ToString()).Set(gaugeValue);
        }
 ```
 ## How to configure SeqApp :
 * Go to Seq
 * click `settings` and then click on `Apps`
 * click on `INSTALL FROM NUGET` and fill the details `Package id`, `version` and click on `INSTALL`
 * click on `ADD INSTANCE`
 * Fill the details `Title` 
 * Check the box `Stream incoming events` to send events and select the signal
 * Fill in the details `Pushgateway URL`, `Pushgateway Gauge Name`, `Pushgateway Gauge Label Key`, `Pushgateway Gague Label Values`
 * click on `SAVE CHANGES`
 


