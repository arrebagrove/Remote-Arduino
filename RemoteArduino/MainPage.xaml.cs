using Glovebox.IoT;
using Glovebox.IoT.Actuators;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Arduino Uno - A0 on Pin 14
// http://www.microsofttranslator.com/bv.aspx?from=&to=en&a=http%3A%2F%2Fblogs.msdn.com%2Fb%2Fsos%2Farchive%2F2015%2F07%2F11%2Fwindows-remote-arduino-creating-lamp-controlled-by-universal-windows-app.aspx

namespace RemoteArduino {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        UsbSerial usbcomm;
        RemoteDevice arduino;
        DispatcherTimer dt;
        VirtualRelay relay;

        byte relay_pin = 7;
        byte led_pin = 3;
        byte A0_pin = 14;

        bool auto_mode = false;

        public MainPage() {
            this.InitializeComponent();
            connect();
        }

        private async void connect() {
            dt = new DispatcherTimer() { Interval = new TimeSpan(500) };
            dt.Tick += loop;

            var dev = await UsbSerial.listAvailableDevicesAsync();
            usbcomm = new UsbSerial(dev[0]);
            arduino = new RemoteDevice(usbcomm);
            usbcomm.ConnectionEstablished += Comm_ConnectionEstablished;
            usbcomm.begin(57600, SerialConfig.SERIAL_8N1);
        }

        private void Comm_ConnectionEstablished() {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => {
                arduino.pinMode(relay_pin, PinMode.OUTPUT);
                arduino.pinMode(led_pin, PinMode.OUTPUT);
                dt.Start();
                on.IsEnabled = true;
                off.IsEnabled = true;
                auto.IsEnabled = true;
                // auto_mode = true;
            }));
        }

        private void loop(object sender, object e) {
            if (auto_mode) {
                arduino.pinMode(A0_pin, PinMode.ANALOG);
                var reading = arduino.analogRead(0);

                Debug.WriteLine(reading.ToString());

                var on = reading < 512;
                arduino.digitalWrite(relay_pin, on ? PinState.HIGH : PinState.LOW);
                arduino.digitalWrite(led_pin, on ? PinState.HIGH : PinState.LOW);
            }
        }

        private void on_Click(object sender, RoutedEventArgs e) {
            OnAction();
        }

        private void off_Click(object sender, RoutedEventArgs e) {
            OffAction();
        }

        private void OnAction() {
            auto_mode = false;
            arduino.digitalWrite(relay_pin, PinState.HIGH);
            arduino.digitalWrite(led_pin, PinState.HIGH);
        }

        private void OffAction() {
            auto_mode = false;
            arduino.digitalWrite(relay_pin, PinState.LOW);
            arduino.digitalWrite(led_pin, PinState.LOW);
        }

        private void auto_Click(object sender, RoutedEventArgs e) {
            auto_mode = true;
        }

        void ConnectNetworkng() {
            relay = new VirtualRelay("relay01");
            relay.OnEvent += Relay_OnEvent;
            Util.StartNetworkServices(true);
        }

        private void Relay_OnEvent(object sender, EventArgs e) {
            var a = ((VirtualRelay.RelayEventArg)e).action;
            switch (a) {
                case VirtualRelay.Actions.On:
                    OnAction();
                    break;
                case VirtualRelay.Actions.Off:
                    OffAction();
                    break;
            }
        }
    }
}
