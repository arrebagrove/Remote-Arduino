using Glovebox.IO.VirtualArduino.Actuators;
using Glovebox.IO.VirtualArduino.Sensors;
using Glovebox.IoT;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Arduino Uno - A0 on Pin 14
// http://www.microsofttranslator.com/bv.aspx?from=&to=en&a=http%3A%2F%2Fblogs.msdn.com%2Fb%2Fsos%2Farchive%2F2015%2F07%2F11%2Fwindows-remote-arduino-creating-lamp-controlled-by-universal-windows-app.aspx

namespace RemoteArduino {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        UsbSerial connection;
        RemoteDevice arduino;

        DispatcherTimer dt;

        VirtualRelay relay;
        VirtualLed led;
        VirtualLDR ldr;

        byte relay_pin = 7;
        byte led_pin = 3;

        bool auto_mode = false;

        public MainPage() {
            this.InitializeComponent();
            connect();

            Util.StartNetworkServices(true);
        }

        private void connect() {
            dt = new DispatcherTimer() { Interval = new TimeSpan(500) };
            dt.Tick += loop;

            //var dev = await UsbSerial.listAvailableDevicesAsync();
            //usbcomm = new UsbSerial(dev[0]);

            connection = new UsbSerial("VID_2341", "PID_0043");

            arduino = new RemoteDevice(connection);
            connection.ConnectionEstablished += Connection_ConnectionEstablished;
            connection.begin(250000, SerialConfig.SERIAL_8N1);
        }

        private void Connection_ConnectionEstablished() {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() => {

                relay = new VirtualRelay(arduino, relay_pin, "relay01");
                led = new VirtualLed(arduino, led_pin, "led01");
                ldr = new VirtualLDR(arduino, VirtualLDR.UnoAnalogPins.A0, Timeout.Infinite, "light01");

                dt.Start();

                on.IsEnabled = true;
                off.IsEnabled = true;
                auto.IsEnabled = true;
               // auto_mode = true;
            }));
        }

        private void loop(object sender, object e) {
            if (auto_mode) {
                if (ldr.Current < 512) {
                    relay.On();
                    led.On();
                }
                else {
                    relay.Off();
                    led.Off();
                }
            }
        }

        private void on_Click(object sender, RoutedEventArgs e) {
            auto_mode = false;
            relay.On();
            led.On();
        }

        private void off_Click(object sender, RoutedEventArgs e) {
            auto_mode = false;
            relay.Off();
            led.Off();
        }

        private void auto_Click(object sender, RoutedEventArgs e) {
            auto_mode = true;
        }
    }
}
