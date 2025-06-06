using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Manager;
using AkribisFAM.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.DeviceClass
{
    public class CognexBarcodeScanner
    {

        public int ScanBarcode(out string readout)
        {
            readout = "";
            if (!Task_Scanner.TriggScannerSendData())
            {

                return (int)ErrorCode.BarocdeScan_Failed;
            }
            var (barcode, error) = Task_Scanner.TriggScannerAcceptData();

            if (error == ErrorCode.BarocdeScan_Failed)
            {
                return (int)ErrorCode.BarocdeScan_Failed;
            }
            readout = barcode;
            Logger.WriteLog($"Readout scanner : {barcode} ");

            //GlobalManager.Current.BarcodeQueue.Enqueue(barcode ?? "NULL");

            ////global switch for using mes system
            //if (GlobalManager.Current.IsUseMES)
            //{
            //    // TODO:Upload barcode to Bali MES Sytem , then judge bypass 
            //}
            //else
            //{
            //    GlobalManager.Current.IsByPass = false;
            //}

            return (int)ErrorCode.NoError;
        }
    }
}
