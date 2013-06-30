using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks
{
    class SkyDriveAccount : NetworkItem
    {
        public SkyDriveAccount()
        {
            _type = AccountType.OnlineStorage;
            _service = Networks.AccountService.SkyDrive;
        }

    }
}
