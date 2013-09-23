using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterExplorer.Networks
{
    class SkyDriveAccount : NetworkItem
    {
        public SkyDriveAccount()
        {
            _type = AccountType.OnlineStorage;
            _service = Networks.AccountService.SkyDrive;
            
        }
 //       private async void GetSkyDriveFiles()
 //       {
 //         var auth = new LiveAuthClient("");
 //
 //         LiveConnectClient client = new LiveConnectClient(auth.Session);
 //         try
 //         {
 //           LiveOperationResult skydriveResult = await client.GetAsync("me/skydrive/files");
 //           dynamic data = skydriveResult.Result["data"];

 //           foreach (dynamic file in data)
 //           {
 //             //process the result 
 //           }
 //           //skydriveResult.Result
 //           //SkyDriveDataModel skydriveFilesModel = GetSkyDriveDataModel(skydriveResult);
 //           //return View("MySkyDriveView", skydriveFilesModel);
 //         }
 //         catch (LiveConnectException ex)
 //         {
 //           //return HanldeUserDataError(ex);
 //         }
 //       }

    }
}
