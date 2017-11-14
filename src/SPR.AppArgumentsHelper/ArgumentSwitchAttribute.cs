using System;

namespace SPR.AppArgumentsHelper
{
    public class ArgumentSwitchAttribute : Attribute
    {
        private string _switchName;
        private ArgumentMode _argumentMode;

        public string SwitchName
            => _switchName;
        public ArgumentMode ArgumentMode
            => _argumentMode;

        public ArgumentSwitchAttribute(string switchName,
                                       ArgumentMode optional = ArgumentMode.Optional)
        {
            this._switchName = switchName;
            this._argumentMode = optional;
        }
    }
}