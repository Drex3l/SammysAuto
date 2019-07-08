using System.Collections.Generic;
using SammysAuto.Models;

namespace SammysAuto.ViewModel
{
    public class CarAndCustomerViewModel
    {
        public ApplicationUser UserObj { get; set; }
        public IEnumerable<Car> Cars { get; set;}
    }
}