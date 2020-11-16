using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LAB_6_SOLAE_HTTPS_SERVER_CS.Dtos
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
