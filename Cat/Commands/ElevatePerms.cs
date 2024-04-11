using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Command that checks if the app is running as admin, and if not, restarts asking for it
        /// </summary>
        /// <returns>true if already has admin, otherwise nothing as it kills the application.</returns>
        [LoggingAspects.UpsetStomach]
        [LoggingAspects.ConsumeException]
        internal static bool KillMyselfAndGetGodPowers()
        {
            bool? rah = Helpers.BackendHelping.RestartWithAdminRightsIfNeeded();
            if (rah == null)
            {
                Interface.AddLog("Already has admin perms");
                return true;
            }
            return rah ?? true;
        }
    }
}
