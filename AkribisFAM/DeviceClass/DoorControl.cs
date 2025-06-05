using AkribisFAM.CommunicationProtocol;

namespace AkribisFAM.DeviceClass
{
    public class DoorControl
    {

        public bool IsDoor1Locked => !IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_4Door_opened_lock1);
        public bool IsDoor2Locked => !IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_5Door_opened_lock2);
        public bool IsDoor3Locked => !IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_6Door_opened_lock3);
        public bool IsDoor4Locked => !IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_7Door_opened_lock4);
        public bool IsAllDoorClosed => !IOManager.Instance.ReadIO(IO_INFunction_Table.IN5_15SSR2_OK_LOCK);
        public bool IsAllLockTriggered => IOManager.Instance.GetOutputStatus(IO_OutFunction_Table.OUT6_10Feeder1_light) &&
                                        IOManager.Instance.GetOutputStatus(IO_OutFunction_Table.OUT2_9LOCK2) &&
                                        IOManager.Instance.GetOutputStatus(IO_OutFunction_Table.OUT2_10LOCK3) &&
                                        IOManager.Instance.GetOutputStatus(IO_OutFunction_Table.OUT2_11LOCK4);
        //public bool Is => IOManager.Instance.ReadIO(IO_OutFunction_Table.OUT2_8LOCK1);
        //public bool IsAllDoorLocked => IsDoor1Locked && IsDoor2Locked && IsDoor3Locked && IsDoor4Locked;

        public enum DoorNumber
        {
            Door1,
            Door2,
            Door3,
            Door4,
        }

        public bool Lock(DoorNumber num)
        {
            IO_OutFunction_Table output;
            switch (num)
            {
                case DoorNumber.Door1:
                    output = IO_OutFunction_Table.OUT2_8LOCK1;
                    break;
                case DoorNumber.Door2:
                    output = IO_OutFunction_Table.OUT2_9LOCK2;
                    break;
                case DoorNumber.Door3:
                    output = IO_OutFunction_Table.OUT2_10LOCK3;
                    break;
                case DoorNumber.Door4:
                    output = IO_OutFunction_Table.OUT2_11LOCK4;
                    break;
                default:
                    return false;
            }
            return IOManager.Instance.IO_ControlStatus(output, 1);
        }


        public bool Unlock(DoorNumber num)
        {
            IO_OutFunction_Table output;
            switch (num)
            {
                case DoorNumber.Door1:
                    output = IO_OutFunction_Table.OUT2_8LOCK1;
                    break;
                case DoorNumber.Door2:
                    output = IO_OutFunction_Table.OUT2_9LOCK2;
                    break;
                case DoorNumber.Door3:
                    output = IO_OutFunction_Table.OUT2_10LOCK3;
                    break;
                case DoorNumber.Door4:
                    output = IO_OutFunction_Table.OUT2_11LOCK4;
                    break;
                default:
                    return false;
            }
            return IOManager.Instance.IO_ControlStatus(output, 0);
        }
        public bool UnlockAll()
        {
            return Unlock(DoorNumber.Door1) & Unlock(DoorNumber.Door2) & Unlock(DoorNumber.Door3) & Unlock(DoorNumber.Door4);
        }
        public bool LockAll()
        {
            return Lock(DoorNumber.Door1) & Lock(DoorNumber.Door2) & Lock(DoorNumber.Door3) & Lock(DoorNumber.Door4);
        }
    }



}
