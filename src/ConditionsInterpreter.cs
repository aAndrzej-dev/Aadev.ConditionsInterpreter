using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aadev.ConditionsInterpreter
{
    public class ConditionsInterpreter
    {

        public ConditionsInterpreter(Func<string, object> getVariable, string conditionString, VariableBuffering variableBuffering = VariableBuffering.Request)
        {
            this.getVariable = getVariable;
            this.conditionString = conditionString;
            this.variableBuffering = variableBuffering;

            if (variableBuffering is VariableBuffering.Request || variableBuffering is VariableBuffering.Instance)
                varsValue = new Dictionary<string, object>();
        }

        private readonly Func<string, object> getVariable;
        private readonly string conditionString;
        private readonly VariableBuffering variableBuffering;
        private readonly Dictionary<string, object> varsValue;




        internal object GetVariableValue(string name)
        {
            if (variableBuffering is VariableBuffering.None)
                return getVariable(name);

            if (varsValue.ContainsKey(name))
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
                varsValue.Clear();
            Token[] tokens = new Lexer(conditionString).GetTokens();
            Node node = new Parser(tokens).Parse();
            bool value = (bool)node.GetValue(this);

            return value;

        }


    }

}
