using System;
using System.Collections.Generic;

namespace Aadev.ConditionsInterpreter
{

    public class ConditionInterpreter : IVariableProvider
    {

        public ConditionInterpreter(Func<string, object> getVariable, string conditionString, VariableBuffering variableBuffering = VariableBuffering.Request)
        {
            this.getVariable = getVariable;
            this.variableBuffering = variableBuffering;

            if (variableBuffering is VariableBuffering.Request || variableBuffering is VariableBuffering.Instance)
                varsValue = new Dictionary<string, object>();

            Lexer lexer = new Lexer(conditionString);
            rootNode = new Parser(lexer).Parse();
        }
        private readonly Func<string, object> getVariable;
        private readonly VariableBuffering variableBuffering;
        private readonly Dictionary<string, object>? varsValue;

        private readonly Node rootNode;



        object IVariableProvider.GetVariableValue(string name)
        {
            if (variableBuffering is VariableBuffering.None)
                return getVariable(name);

            if (varsValue!.ContainsKey(name))
                return varsValue[name];

            varsValue.Add(name, getVariable(name));

            return varsValue[name];

        }

        public enum VariableBuffering
        {
            None = 0,
            Request = 1,
            Instance = 2
        }

        public bool ResolveCondition()
        {
            if (variableBuffering is VariableBuffering.Request)
                varsValue!.Clear();

            bool value = (bool)rootNode.GetValue(this);

            return value;

        }

    }
    internal interface IVariableProvider
    {
        object GetVariableValue(string name);
    }

}
