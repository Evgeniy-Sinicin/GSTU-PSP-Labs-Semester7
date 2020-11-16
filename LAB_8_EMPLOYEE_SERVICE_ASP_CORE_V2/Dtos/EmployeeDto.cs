using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2.Dtos
{
    [DataContract]
    public class EmployeeDto
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string AgeStr { get; set; }
        [DataMember]
        public string ExperienceYearsStr { get; set; }
        [DataMember]
        public string CompanyAddress { get; set; }
    }
}
