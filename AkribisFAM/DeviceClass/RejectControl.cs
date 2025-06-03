using AkribisFAM.CommunicationProtocol;

namespace AkribisFAM.DeviceClass
{
    public class RejectControl
    {

        public bool IsCover1Closed()
        {
            return IOManager.Instance.ReadIO(IO_INFunction_Table.IN1_8NG_cover_plate1);

        }
        public bool IsCover2Closed()
        {
            return IOManager.Instance.ReadIO(IO_INFunction_Table.IN1_9NG_cover_plate2);

        }

        public bool IsAllCoversClosed()
        {
            return IsCover1Closed() && IsCover2Closed();
        }

    }
}
