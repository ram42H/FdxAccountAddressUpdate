using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FdxAccountAddressUpdate
{
    [DataContractAttribute]    
    public class APIResponse
    {
        [DataMember]
        public string goldMineId { get; set; }

        [DataMember]
        public bool goNoGo { get; set; }
    }
}
