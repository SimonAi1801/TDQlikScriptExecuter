using System;
using System.Collections.Generic;
using System.Text;

namespace TDQlikScriptExecuter.Entities
{
    public class CustomInformation
    {
        public string AppId { get; set; }
        public string Url { get; set; }
        public string HeadName { get; set; }
        public string UserDirectory { get; set; }
        public string UserId { get; set; }
        public string ProxyPath { get; set; }

        public override string ToString()
        {
            return $"{AppId},{Url},{HeadName},{UserDirectory},{UserId},{ProxyPath}";
        }
    }
}
