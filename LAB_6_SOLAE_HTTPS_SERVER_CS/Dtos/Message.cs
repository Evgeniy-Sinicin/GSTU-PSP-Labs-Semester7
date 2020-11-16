using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LAB_5_SOLAE_HTTP_SERVER_CS.Dtos
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public List<double> Matrix { get; set; }
        [DataMember]
        public List<double> Coeffs { get; set; }
        [DataMember]
        public List<double> Decisions { get; set; }
    }
}
