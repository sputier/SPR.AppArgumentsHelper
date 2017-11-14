using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SPR.AppArgumentsHelper
{
    public class AppArgumentsManager<T> where T : new()
    {
        private readonly string _switchIdentifier = "--";
        private Dictionary<string, (PropertyInfo Property, ArgumentSwitchAttribute Attribute)> _switchesInfosMap;

        private void LoadAvailableSwitches()
        {
            if (_switchesInfosMap != null)
                return;

            _switchesInfosMap = typeof(T).GetTypeInfo().DeclaredProperties
                                         .Where(prop => !prop.GetMethod.IsStatic && prop.GetMethod.IsPublic)
                                         .Select(prop => (Property: prop, Attribute: prop.GetCustomAttribute<ArgumentSwitchAttribute>()))
                                         .Where(tuple => tuple.Attribute != null)
                                         .ToDictionary(tuple => tuple.Attribute.SwitchName, StringComparer.CurrentCultureIgnoreCase);
        }

        public T LoadArgs(string[] args)
        {
            LoadAvailableSwitches();

            var switchValuesDictionary = LoadArgsInternal(args);
            CheckSwitchesConstraints(switchValuesDictionary);

            return MapSwitchValuesToArgs(switchValuesDictionary);
        }

        private T MapSwitchValuesToArgs(Dictionary<string, string[]> switchValuesDictionary)
        {
            var result = new T();
            foreach (var switchValues in switchValuesDictionary)
            {
                var switchTuple = _switchesInfosMap[switchValues.Key];
                if (_switchesInfosMap[switchValues.Key].Property.PropertyType.IsArray)
                {
                    var array = Activator.CreateInstance(switchTuple.Property.PropertyType, switchValues.Value.Length);

                    for (int i = 0; i < switchValues.Value.Length; i++)
                        ((object[])array)[i] = switchValues.Value[i];

                    switchTuple.Property.SetValue(result, array);
                }
                else
                {
                    // We set the property with the next arg value with a simple cast
                    switchTuple.Property.SetValue(result, Convert.ChangeType(switchValues.Value[0], switchTuple.Property.PropertyType));
                }
            }

            return result;
        }

        private void CheckSwitchesConstraints(Dictionary<string, string[]> switchValuesDictionary)
        {
            foreach (var switchInfo in _switchesInfosMap)
            {
                var multiplicity = switchInfo.Value.Property.PropertyType.IsArray ? ArgumentMultiplicity.Multiple : ArgumentMultiplicity.Single;
                var mode = switchInfo.Value.Attribute.ArgumentMode;

                if (mode == ArgumentMode.Required && !switchValuesDictionary.ContainsKey(switchInfo.Key))
                    throw new ArgumentException($"The --{switchInfo.Key} switch is required.");

                if (multiplicity == ArgumentMultiplicity.Single
                        && switchValuesDictionary.ContainsKey(switchInfo.Key)
                        && switchValuesDictionary[switchInfo.Key].Length > 1)
                    throw new ArgumentException($"The --{switchInfo.Key} switch can be specified only once.");
            }
        }

        private Dictionary<string, string[]> LoadArgsInternal(string[] args)
        {
            var result = new Dictionary<string, string[]>();

            if (args.Length == 0)
                return result;

            string currentArg = null;

            for (int i = 0; i < args.Length; i++)
            {
                currentArg = args[i];

                // If the arg doesn't start with <_switchIdentifier> (default: --), it is not a switch
                // Also, it its length equals _switchIdentifier.Length, it has no name, so we ignore it
                if (!currentArg.StartsWith(_switchIdentifier) || currentArg.Length == _switchIdentifier.Length)
                    continue;

                // We strip the arg name, as the next processes don't need the switch identifier
                currentArg = currentArg.Substring(_switchIdentifier.Length, currentArg.Length - _switchIdentifier.Length);

                // If the _switchesInfos map doesn't contain the arg, it is not a switch. We ignore it
                if (!_switchesInfosMap.Keys.Contains(currentArg))
                    continue;

                var switchTuple = _switchesInfosMap[currentArg];

                // If this is a boolean switch (ie true if specified), 
                // we set the property and continue to the next arg
                if (switchTuple.Property.PropertyType.Equals(typeof(bool)))
                {
                    // If a boolean switch appears one, its value is set to true.
                    // If it appears more than once, its value is also set to true (true + true + true... )
                    // so we ignore it
                    if (!result.ContainsKey(switchTuple.Attribute.SwitchName))
                        result.Add(switchTuple.Attribute.SwitchName, new[] { "true" });

                    continue;
                }

                // The switch needs a value provided. If this switch is the last arg, we ignore it and return.
                if (i == args.Length - 1)
                    return result;

                if (!result.ContainsKey(switchTuple.Attribute.SwitchName))
                    result.Add(switchTuple.Attribute.SwitchName, new[] { args[i + 1] });
                else
                {
                    var tmpArray = new string[result[switchTuple.Attribute.SwitchName].Length + 1];
                    result[switchTuple.Attribute.SwitchName].CopyTo(tmpArray, 0);
                    tmpArray[result[switchTuple.Attribute.SwitchName].Length] = args[i + 1];

                    result[switchTuple.Attribute.SwitchName] = tmpArray;
                }

                // We increment i because have already read the next arg
                i++;
            }

            return result;
        }
    }
}
