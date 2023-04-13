using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaLocalization.Services
{
    public static class LocalizationProvider
    {
        public static ILocalizeService Service { get; private set; }

        static LocalizationProvider()
        {
            Service = new LocalizeService();
        }
    }
}
