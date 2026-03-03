using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfpr3.Service
{
    //переключатель двухфакторной аутентификации
    public static class AppSecurityOptions
    {
        public static bool TwoFactorEnabledGlobally { get; set; } = true;
    }
}
