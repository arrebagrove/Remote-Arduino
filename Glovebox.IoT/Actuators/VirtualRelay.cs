using Glovebox.IoT.Base;
using Glovebox.IoT.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glovebox.IoT.Actuators {
    public class VirtualRelay : IotBase {

        public delegate void ActuatorEventHandler(object sender, EventArgs e);
        public event ActuatorEventHandler OnEvent;

        public class RelayEventArg : EventArgs {
            public readonly Actions action;
            public RelayEventArg(Actions action) {
                this.action = action;
            }
        }

        private void RelayEvent(EventArgs e) {
            if (OnEvent != null) {
                OnEvent(this, e);
            }
        }

        public enum Actions {
            On,
            Off
        }

        public VirtualRelay(string name) : base(name, "relay", IotType.Actuator) {

        }
        protected override void CleanUp() {

        }

        public void Action(Actions action) {
            switch (action) {
                case Actions.On:
                    On();
                    break;
                case Actions.Off:
                    Off();
                    break;
                default:
                    break;
            }
        }

        public override void Action(IotAction action) {
            switch (action.cmd) {
                case "on":
                    On();
                    break;
                case "off":
                    Off();
                    break;
            }
        }

        public void On() {
            RelayEvent(new RelayEventArg(Actions.On));
        }

        public void Off() {
            RelayEvent(new RelayEventArg(Actions.Off));
        }
    }
}
