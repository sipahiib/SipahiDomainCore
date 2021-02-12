using Microsoft.Extensions.Configuration;
using SipahiDomainCore.Models;
using System.Collections.Generic;

namespace SipahiDomainCore.ViewModels
{
    public class ViewModels
    {
        public IConfigurationSection Followups { get; set; }
        public IConfigurationSection Educations { get; set; }
    }
}
