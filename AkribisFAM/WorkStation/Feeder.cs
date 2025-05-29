using AkribisFAM.CommunicationProtocol;
using AkribisFAM.DeviceClass;
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
        public enum FeederSequenceStep
        {
            Idle,                   // Default state
            Initialize,             // Initialize feeder hardware and sensors
            CheckSensorStatus,      // Check all feeder sensors (e.g., empty, part present, etc.)
            CheckFeederReady,       // Check actuator readiness, safety, or interlocks
            WaitForStartCommand,    // Wait for command from upstream or control system
            StartFeeding,           // Command to activate indexing actuator
            IndexingInProgress,     // Indexing in motion
            IndexingComplete,       // Confirm indexing is complete (e.g., sensor confirmation)
            VerifyPartInPosition,   // Check if part arrived properly in place
            SequenceComplete,       // Normal end of sequence
            ErrorDetected,          // Error flag triggered (e.g., timeout, sensor mismatch)
            ErrorHandling,          // Run error recovery routine
            ReturnToPreviousStep    // For recovering to the last known good step
        }

        FeederSequenceStep currentStep = FeederSequenceStep.CheckSensorStatus;
        FeederSequenceStep previousStep = FeederSequenceStep.Idle;

        public override string Name => throw new NotImplementedException();

        FeederControl _feeder = new FeederControl(1);
        private void LoadFeederControl(FeederControl feeder)
        {
            _feeder = feeder;
        }
        private void SwitchFeeder()
        {
            _feeder = _feeder.FeederNumber == 1 ? App.feeder2 : App.feeder1;
        }   
        public override void AutoRun(CancellationToken token)
        {
            switch (currentStep)
            {
                case FeederSequenceStep.Initialize:
                    // CHECK STATUS
                    if (!_feeder.IsDrawerInPos)
                    {
                        // Switch feeder
                        SwitchFeeder();
                        if (!_feeder.IsDrawerInPos)
                        {
                            Logger.WriteLog("Both feeders are not in position.");
                            currentStep = FeederSequenceStep.ErrorDetected;
                            break;
                        }
                        else
                        {   
                            currentStep = FeederSequenceStep.Initialize; // Other feeder is in position, re-initialize with this new feeder
                            break;
                        }
                    }
                    // CHECK LOCK STATUS
                    if (!_feeder.IsLock) // If feeder is not locked
                    {
                        _feeder.Lock(); // Attempt to auto lock it
                        System.Threading.Thread.Sleep(1000);
                        if (!_feeder.IsLock)
                        {
                            Logger.WriteLog("Feeder lock failed.");
                            currentStep = FeederSequenceStep.ErrorDetected;
                            break;
                        }
                    }

                    // CHECK VACUUM STATUS
                    if (!_feeder.IsAllVacPressureOk) // If vacuum pressure is not OK
                    {
                        _feeder.VacOn(); // Turn on vacuum
                        System.Threading.Thread.Sleep(1000);
                        if (!_feeder.IsAllVacPressureOk) // If vacuum pressure is still not OK
                        {
                            Logger.WriteLog("Feeder vacuum pressure is not OK.");
                            currentStep = FeederSequenceStep.ErrorDetected;
                            break;
                        }
                    }

                    break;

                case FeederSequenceStep.CheckFeederReady:
                    if (_feeder.IsInitialized)
                    {
                        currentStep = FeederSequenceStep.WaitForStartCommand;
                    }
                    else
                    {
                        //TriggerError();
                    }
                    break;

                case FeederSequenceStep.WaitForStartCommand:
                  
                    break;

                case FeederSequenceStep.StartFeeding:
                    if (_feeder.IsInitialized)
                    {

                    }

                    currentStep = FeederSequenceStep.IndexingInProgress;
                    break;

                case FeederSequenceStep.IndexingInProgress:
                    //if (IsIndexingComplete())
                    //{
                    //    currentStep = FeederSequenceStep.IndexingComplete;
                    //}
                    //else if (HasIndexingError())
                    //{
                    //    TriggerError();
                    //}
                    break;

                case FeederSequenceStep.IndexingComplete:
                    currentStep = FeederSequenceStep.VerifyPartInPosition;
                    break;

                case FeederSequenceStep.VerifyPartInPosition:
                    //if (IsPartInPosition())
                    //{
                    //    currentStep = FeederSequenceStep.SequenceComplete;
                    //}
                    //else
                    //{
                    //    TriggerError();
                    //}
                    break;

                case FeederSequenceStep.SequenceComplete:
                    // Reset or wait for next cycle
                    break;

                case FeederSequenceStep.ErrorDetected:
                    // Log error, alert operator, etc.
                    currentStep = FeederSequenceStep.ErrorHandling;
                    break;

                case FeederSequenceStep.ErrorHandling:
                    // Perform recovery or wait for manual reset
                    currentStep = FeederSequenceStep.ReturnToPreviousStep;
                    break;

                case FeederSequenceStep.ReturnToPreviousStep:
                    currentStep = previousStep;
                    break;

                case FeederSequenceStep.Idle:
                default:
                    break;
            }
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
    }
}
