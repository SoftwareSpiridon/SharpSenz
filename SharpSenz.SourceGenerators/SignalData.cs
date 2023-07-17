using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSenz.SourceGenerators
{
    internal struct SignalData
    {
        public SignalData(string callerFilePath,
                          int callerFileLine,
                          string callerClassName,
                          string callerMethodName,
                          string signalMessage,
                          string signalMethodName,
                          string argumentsDefinitions,
                          string argumentsToCall)
        {
            CallerFilePath = callerFilePath;
            CallerFileLine = callerFileLine;
            CallerClassName = callerClassName;
            CallerMethodName = callerMethodName;
            SignalMessage = signalMessage;
            SignalMethodName = signalMethodName;
            ArgumentsDefinitions = argumentsDefinitions;
            ArgumentsToCall = argumentsToCall;
        }

        public string CallerFilePath { get; private set; }
        public int CallerFileLine { get; private set; }
        public string CallerClassName { get; private set; }
        public string CallerMethodName { get; private set; }
        public string SignalMessage { get; private set; }
        public string SignalMethodName { get; private set; }
        public string ArgumentsDefinitions { get; private set; }
        public string ArgumentsToCall { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is SignalData data &&
                   CallerFilePath == data.CallerFilePath &&
                   CallerFileLine == data.CallerFileLine &&
                   CallerClassName == data.CallerClassName &&
                   CallerMethodName == data.CallerMethodName &&
                   SignalMessage == data.SignalMessage &&
                   SignalMethodName == data.SignalMethodName &&
                   ArgumentsDefinitions == data.ArgumentsDefinitions &&
                   ArgumentsToCall == data.ArgumentsToCall;
        }

        public override int GetHashCode()
        {
            int hashCode = -2110509238;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CallerFilePath);
            hashCode = hashCode * -1521134295 + CallerFileLine.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CallerClassName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CallerMethodName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SignalMessage);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SignalMethodName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ArgumentsDefinitions);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ArgumentsToCall);
            return hashCode;
        }
    }
}
