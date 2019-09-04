# Seq.App.Prometheus.Pushgateway

A Seq app that pushes events to [Prometheus Pushgateway](https://github.com/prometheus/pushgateway). **Requires Seq 5.1+.**

### Getting started

The app is published to NuGet as [_Seq.App.Prometheus.Pushgateway_](https://nuget.org/packages/Seq.App.Prometheus.Pushgateway). Follow the instructions for [installing a Seq App](https://docs.getseq.net/docs/installing-seq-apps) and start an instance of the app, providing your details.
* Based on the signals we select for the events Seq will send those events to SeqApp.
* SeqApp will send Prometheus counters for those events to the PushgatewayUrl.

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
            var counterLabelValuesList = SplitOnNewLine(this.CounterLabelValues).ToList();
            var pushgatewayCounterData = ApplicationNameKeyValueMapping(evt, counterLabelValuesList);

            var counter = Metrics.CreateCounter(CounterName, "To keep the count of no of times a particular error coming in a module.");
            counter.Inc();
        }
 ```
 ## How to configure SeqApp :
 * To configure SeqApp go through this [Link]( https://docs.getseq.net/docs/installing-seq-apps)
 * Fill in the details `Pushgateway URL`, `Pushgateway Counter Name`, `Pushgateway Counter Label Key`, `Pushgateway Counter Label Values`


