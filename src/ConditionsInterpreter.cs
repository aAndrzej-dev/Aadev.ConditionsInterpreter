using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aadev.ConditionsInterpreter
{
    public class ConditionsInterpreter
    {
        public ConditionsInterpreter(Func<string, object> getVariable, string conditionString)
        {
            GetVariable = getVariable;
            ConditionString = conditionString;
        }

        internal Func<string, object> GetVariable { get; }
        private string ConditionString { get; }

        public bool ResolveCondition() => (bool)new Parser(new Lexer(ConditionString).GetTokens()).Parse().GetValue(this);


    }

}
