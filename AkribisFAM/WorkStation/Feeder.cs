using AkribisFAM.CommunicationProtocol;
using AkribisFAM.DeviceClass;
using AkribisFAM.Manager;
using AkribisFAM.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AkribisFAM.WorkStation
{
    internal class Feeder : WorkStationBase
    {
        private static Feeder _instance;
        public static Feeder Current
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new Feeder();
                    }
                }
                return _instance;
            }
        }
        public enum FeederSequenceStep
        {
            Idle,                   // Default state
            Initialize,             // Initialize feeder hardware and sensors
            StartFeeding,           // Command to activate indexing actuator
            VerifyPartInPosition,   // Check if part arrived properly in place
            WaitTillAllPartsPicked, // Wait for all parts to be picked from feeder
            SwitchFeeder,           // Switch between feeder 1 and feeder 2
            VerifySwitchSuccessful, // Verify if the switch between feeders is successful

            ErrorDetected,          // Error flag triggered (e.g., timeout, sensor mismatch)
            ErrorHandling,          // Run error recovery routine
            ReturnToPreviousStep    // For recovering to the last known good step
        }

        FeederSequenceStep currentStep = FeederSequenceStep.Initialize;
        FeederSequenceStep previousStep = FeederSequenceStep.Idle;

        DateTime SeqStartTime = DateTime.Now;

        private bool _canPick = false;

        public override string Name => throw new NotImplementedException();

        public bool ReadIO(IO_INFunction_Table index)
        {
            //return AutorunManager.Current.ReadIO(index);
            if (IOManager.Instance.INIO_status[(int)index] == 0)
            {
                return true;
            }
            else if (IOManager.Instance.INIO_status[(int)index] == 1)
            {
                return false;
            }
            else
            {
                ErrorManager.Current.Insert(ErrorCode.IOErr);
                return false;
            }
        }

        FeederControl _feeder = new FeederControl(1);
        private void LoadFeederControl(FeederControl feeder)
        {
            _feeder = feeder;
        }
        private void SwitchFeeder()
        {
            _feeder = _feeder.FeederNumber == 1 ? App.feeder2 : App.feeder1;
        }
        public int GetActiveFeederNumber()
        {
            return _feeder.FeederNumber;
        }
        /// <summary>
        /// Returns true if active feeder have parts available to pick
        /// </summary>
        /// <returns></returns>
        public bool CanPick()
        {
            return _canPick;
        }

        public bool IsFeederReady()
        {
            return IsFeederReady(_feeder, out _);
        }

        public override void AutoRun(CancellationToken token)
        {
        
            switch (currentStep)
            {
                case FeederSequenceStep.Initialize:
                    
                    // CHECK STATUS
                    if (IsFeederReady(_feeder, out string ErrMsg))
                    {
                        currentStep = FeederSequenceStep.StartFeeding;
                    } else
                    {
                        Logger.WriteLog($"Feeder {_feeder.FeederNumber} initialization failed: {ErrMsg}. Switching feeder now.");
                        currentStep = FeederSequenceStep.SwitchFeeder;
                    }
                    break;

                case FeederSequenceStep.StartFeeding:
                    if (IsInitialized(_feeder))
                    {
                        if (_feeder.hasPartsIn)
                        {
                            currentStep = FeederSequenceStep.WaitTillAllPartsPicked;
                        }
                        else
                        {
                            _feeder.Index(); // Start the indexing process
                            SeqStartTime = DateTime.Now;
                            currentStep = FeederSequenceStep.VerifyPartInPosition;
                        }
                            
                    }
                    else
                    {
                        Logger.WriteLog($"Feeder {_feeder.FeederNumber} is not initialized. Switching feeder now.");
                        currentStep = FeederSequenceStep.SwitchFeeder;
                    }
                    break;

                case FeederSequenceStep.VerifyPartInPosition:
                    if (_feeder.hasPartsIn) // Check if part is in position
                    {
                        _canPick = true; // Set the flag to allow picking
                        currentStep = FeederSequenceStep.WaitTillAllPartsPicked;
                        break;
                    }
                    else if ((DateTime.Now - SeqStartTime).TotalMilliseconds > 3000) // Timeout after 3 seconds
                    {
                        if (_feeder.hasAlarm) // Feeder empty or has alarm
                        {
                            Logger.WriteLog($"Feeder {_feeder.FeederNumber} is empty or has an alarm condition. Switching feeder now.");
                            currentStep = FeederSequenceStep.SwitchFeeder;
                        } else if (!_feeder.IsInitialized) // Feeder not initialized
                        {
                            Logger.WriteLog($"Feeder {_feeder.FeederNumber} is not initialized. Switching feeder now.");
                            currentStep = FeederSequenceStep.SwitchFeeder;
                        } else
                        {
                            //Logger.WriteLog("Part not detected in feeder within timeout period.");
                            //currentStep = FeederSequenceStep.ErrorDetected;
                        }
                        break;
                    }
                    else
                    {
                        // Continue waiting for part to be in position
                    }
                    break;

                case FeederSequenceStep.WaitTillAllPartsPicked:
                    if (!_feeder.hasPartsIn) // Check if all parts have been picked
                    {
                        // All parts have been picked, reset the flag
                        _canPick = false;
                        currentStep = FeederSequenceStep.StartFeeding;
                        break;
                    }
                    else
                    {
                        // Continue waiting for all parts to be picked while checking for errors
                        if (_feeder.hasAlarm) // Feeder has alarm
                        {
                            Logger.WriteLog($"Feeder {_feeder.FeederNumber} has alarm condition.");
                            currentStep = FeederSequenceStep.SwitchFeeder;
                        }
                        else if (!_feeder.IsInitialized) // Feeder not initialized
                        {
                            Logger.WriteLog($"Feeder {_feeder.FeederNumber} is not initialized. Switching feeder now.");
                            currentStep = FeederSequenceStep.SwitchFeeder;
                        }
                    }
                    break;

                case FeederSequenceStep.SwitchFeeder:
                    SwitchFeeder();
                    SeqStartTime = DateTime.Now; // Reset the sequence start time
                    currentStep = FeederSequenceStep.VerifySwitchSuccessful;
                    break;

                case FeederSequenceStep.VerifySwitchSuccessful:
                    if (IsFeederReady(_feeder, out string switchErrMsg))
                    {
                        Logger.WriteLog("Feeder switch successful.");
                        currentStep = FeederSequenceStep.StartFeeding;
                    }
                    else
                    {
                        // Both feeders are not ready
                        Logger.WriteLog($"Feeder {_feeder.FeederNumber} switch failed: {switchErrMsg}");
                        currentStep = FeederSequenceStep.ErrorDetected;
                    }
                    break;

                case FeederSequenceStep.ErrorDetected:
                    // Log error, alert operator, etc.
                    currentStep = FeederSequenceStep.ErrorHandling;
                    break;

                case FeederSequenceStep.ErrorHandling:
                    // Perform recovery or wait for manual reset
                    if (IsInitialized(_feeder))
                    {
                        currentStep = FeederSequenceStep.Initialize;
                    }
                    break;

                case FeederSequenceStep.ReturnToPreviousStep:
                    currentStep = previousStep;
                    break;

                case FeederSequenceStep.Idle:
                default:
                    break;
            }
        }
        private bool IsInitialized(FeederControl feeder)
        {
            var IO = feeder.FeederNumber == 1 ? IO_INFunction_Table.IN4_0Initialized_feeder1 : IO_INFunction_Table.IN4_4BInitialized_feeder2;
            return ReadIO(IO);
        }
        private bool HasPartIn(FeederControl feeder)
        {
            var IO = feeder.FeederNumber == 1 ? IO_INFunction_Table.IN4_2Platform_has_label_feeder1 : IO_INFunction_Table.IN4_7Backup_Platform_2_has_label_feeder2;
            return ReadIO(IO);
        }
        public void SetIO(IO_OutFunction_Table index, int value)
        {
            IOManager.Instance.IO_ControlStatus(index, value);
        }
        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool Ready()
        {
            throw new NotImplementedException();
        }

        public override void ReturnZero()
        {
            throw new NotImplementedException();
        }

        private bool IsFeederReady(FeederControl feeder, out string ErrMsg)
        {
            ErrMsg = string.Empty;
            // Check if feeder is ready based on its internal state

            // CHECK DRAWER STATUS
            if (!feeder.IsDrawerInPos)
            {
                ErrMsg = "Feeder drawer is not in position.";
                Logger.WriteLog(ErrMsg);
                return false;
            }

            // CHECK LOCK STATUS
            if (!feeder.IsLock) // If feeder is not locked
            {
                feeder.Lock(); // Attempt to auto lock it
                System.Threading.Thread.Sleep(1000);
                if (!feeder.IsLock)
                {
                    ErrMsg = "Feeder position is not locked";
                    Logger.WriteLog(ErrMsg);
                    return false;
                }
            }

            // CHECK VACUUM STATUS
            if (!feeder.IsAllVacPressureOk) // If vacuum pressure is not OK
            {
                feeder.VacOn(); // Turn on vacuum
                System.Threading.Thread.Sleep(1000);
                if (!feeder.IsAllVacPressureOk) // If vacuum pressure is still not OK
                {
                    ErrMsg = "Feeder vacuum pressure is not OK.";
                    Logger.WriteLog(ErrMsg);
                    return false;
                }
            }

            return true; // Feeder is ready
        }
    }
}
