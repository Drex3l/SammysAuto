using System.Collections.Generic;
using SammysAuto.Models;

namespace SammysAuto.ViewModel
{
    public class CarAndServicesViewModel
    {
         public int carId { get; set;}
         public string VIN { get; set;}
         public string Make { get; set;}
         public string Model { get; set;}
         public string Style { get; set;}
         public int Year { get; set;}
         public string UserId { get; set; }

         public Service NewServiceObj { get; set; }
         public IEnumerable<Service> PastServicesObj { get; set;}
         public List<ServiceType> ServiceTypesObj { get; set; }
    }
}