using System;
using System.Collections.Generic;
using System.Text;

namespace TDQlikScriptExecuter.Entities
{
    public class Connector
    {
        public string DBConnection { get; set; }

        public string QlikLibrary { get; set; }

        public override string ToString()
        {
            return $"vDataBase={DBConnection}, QlikConnector={QlikLibrary}";
        }
    }
}
