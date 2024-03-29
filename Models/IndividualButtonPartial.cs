using System;
using System.Text;

namespace SammysAuto.Models
{
    public class IndividualButtonPartial
    {
        public string ButtonType { get; set; }
        public string Action { get; set; }
        public string Glyph { get; set; }
        public string Text { get; set; }

        public int? ServiceId { get; set; }
        public string CustomerId { get; set; }
        public int? CarId { get; set; }

        public string ActionParameters
        {
            get
            {
                var param = new StringBuilder(@"/");
                if (ServiceId != 0 && ServiceId != null)
                {
                    param.Append(String.Format("{0}", ServiceId));
                }
                if (CustomerId != null && CustomerId.Length > 0)
                {
                    param.Append(String.Format("{0}", CustomerId));
                }
                if (CarId != 0 && CarId != null)
                {
                    param.Append(String.Format("{0}", CarId));
                }
                return param.ToString().Substring(0, param.Length);
            }
        }
    }
}