using System.Runtime.Serialization;

namespace LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2.Entities
{
    [DataContract]
    public class Employee
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Age { get; set; }
        [DataMember]
        public int ExperienceYears { get; set; }
        [DataMember]
        public string CompanyAddress { get; set; }
    }
}
