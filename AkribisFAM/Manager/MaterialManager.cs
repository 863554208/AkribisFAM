using Newtonsoft.Json;
using System;
using AkribisFAM.Helper;

namespace AkribisFAM.Manager
{


    public class MaterialManager
    {
        private int currentFeederNum = (int)FeederNum.None;
        public Feeder Feeder1 { get; set; } = new Feeder(1);
        public Feeder Feeder2 { get; set; } = new Feeder(2);

        /// <summary>
        /// Flag that indicate the feeders is at ready state
        /// True : Feeder has enough supply for next cycle
        /// False : Feeder do not have material (stop the process when this flag is false)
        /// </summary>
        [JsonIgnore] public bool IsMaterialEnough => !Feeder1.IsOut && !Feeder2.IsOut;
        /// <summary>
        /// Flag to remind user to replenish material. 
        /// True : Either one of the feeder material is out and ready for replenishment
        /// False : Both feeder material supply is at runnable state
        /// </summary>
        [JsonIgnore] public bool IsMaterialRequiredReplenish => Feeder1.IsOut || Feeder2.IsOut;

        /// <summary>
        /// Return the Feeder instance of running feeder
        /// Return NULL if error or both feeder's material supply is out
        /// </summary>
        [JsonIgnore]
        public Feeder CurrentFeeder
        {
            get
            {
                switch (currentFeederNum)
                {
                    case 1:
                        return Feeder1;
                    case 2:
                        return Feeder2;
                    default:
                        return null;
                }
            }
        }
        /// <summary>
        /// Method to set threshold for material low
        /// Set Material low threshold after the initialization
        /// </summary>
        /// <param name="targetFeeder">Target feeder to be set</param>
        /// <param name="lowThreshold">Threshold for material low</param>
        public void SetLowThreshold(int targetFeeder, int lowThreshold)
        {
            if (targetFeeder < 1 && targetFeeder > 2) return;

            switch (targetFeeder)
            {
                case 1:
                    Feeder1.SetLowThreshold(lowThreshold);
                    break;
                case 2:
                    Feeder2.SetLowThreshold(lowThreshold);
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// Method to set threshold for material out
        /// Set Material out threshold after the initialization
        /// </summary>
        /// <param name="targetFeeder">Target feeder to be set</param>
        /// <param name="outThreshold">Threshold for material out</param>
        public void SetOutThreshold(int targetFeeder, int outThreshold)
        {
            if (targetFeeder < 1 && targetFeeder > 2) return;

            switch (targetFeeder)
            {
                case 1:
                    Feeder1.SetOutThreshold(outThreshold);
                    break;
                case 2:
                    Feeder2.SetOutThreshold(outThreshold);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Method to update the input material remaining balance count during the material replenishment 
        /// </summary>
        /// <param name="targetFeeder">Target feeder to be set</param>
        /// <param name="newMaterialCount">Amount of newly insert material</param>
        public void ReplenishMaterial(int targetFeeder, int newMaterialCount)
        {
            if (targetFeeder < 1 && targetFeeder > 2) return;

            InputMaterialDetails newMaterial = new InputMaterialDetails()
            {
                TotalAmount = newMaterialCount
            };
            switch (targetFeeder)
            {
                case 1:
                    Feeder1.Replenish(newMaterial);
                    break;
                case 2:
                    Feeder2.Replenish(newMaterial);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Method to consume and minus off the material supply
        /// </summary>
        /// <param name="count">Amount of part or material used</param>
        public void UseFoamCount(int count)
        {
            if (CurrentFeeder == null) return;
            //if (count <= 0 || count > 4) return;

            try
            {
                if (CurrentFeeder.MaterialBalance >= count)
                {
                    CurrentFeeder.MaterialUsedCount += count;
                }
                else
                {
                    CurrentFeeder.MaterialUsedCount = CurrentFeeder.CurrentMaterial.TotalAmount;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// This function has to be called during the system initialization step.
        /// It will retrieve the material information from the last save, if it failed to find or load the file it will save the default value into file
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            MaterialManager mat;
            FileHelper.Load(out mat);
            if (mat == null)
            {
                Default();
                FileHelper.Save(this);
            }
            else
            {
                FileHelper.CopyProperties(mat, this);
            }

            return true;
        }


        /// <summary>
        /// The number of feeder to be used for feeding process
        /// -99 : Default state
        /// -1 : Both feeder's material supply is out
        /// -2 : One of the threshold is not set
        /// 1 : Feeder 1 is used to feed material
        /// 2 : Feeder 2 is used to feed material
        /// </summary>
        public int CurrentFeederNum
        {
            get
            {
                if (Feeder1.MaterialOutThreshold == 0 || Feeder2.MaterialOutThreshold == 0)
                {
                    currentFeederNum = (int)FeederNum.ThresholdNotSet;
                }
                else
                {
                    switch (currentFeederNum)
                    {
                        case 1:
                            if (Feeder1.IsOut)
                            {
                                currentFeederNum = Feeder2.IsOut ? (int)FeederNum.MaterialOut : (int)FeederNum.Feeder2;
                            }
                            break;
                        case 2:
                            if (Feeder2.IsOut)
                            {
                                currentFeederNum = Feeder1.IsOut ? (int)FeederNum.MaterialOut : (int)(int)FeederNum.Feeder1;
                            }
                            break;
                        default:

                            if (!Feeder1.IsOut)
                            {
                                currentFeederNum = (int)FeederNum.Feeder1;
                            }
                            else if (!Feeder2.IsOut)
                            {
                                currentFeederNum = (int)FeederNum.Feeder2;
                            }

                            break;
                    }

                }
                return currentFeederNum;
            }
            set
            {
                currentFeederNum = value;
            }
        }


        /// <summary>
        /// Default property of MaterialManager
        /// </summary>
        private void Default()
        {
            Feeder1 = new Feeder(1);
            Feeder2 = new Feeder(2);
        }



        public class InputMaterialDetails
        {
            /// <summary>
            /// Total amount for input material, this number should be divisible
            /// </summary>
            public int TotalAmount { get; set; } = 0;
            /// <summary>
            /// Unique code that assigned to a specific type of component or product
            /// </summary>
            public string PartNumber { get; set; } = string.Empty;
            /// <summary>
            /// Unique identifier assigned to each individual unit of a product
            /// </summary>
            public string SerialNumber { get; set; } = string.Empty;
            /// <summary>
            /// Date time of the material is loaded into the system
            /// </summary>
            public DateTime DateTimeLoaded { get; set; }
        }
        public class Feeder
        {
            /// <summary>
            /// Feeder ID
            /// </summary>
            [JsonIgnore] public int Number { get; set; } = -1;
            /// <summary>
            /// The amount of remaining part since the last material change
            /// </summary>
            public int MaterialBalance => CurrentMaterial.TotalAmount - MaterialUsedCount;
            /// <summary>
            /// The amount of part that has been consumed since the last material change
            /// </summary>
            public int MaterialUsedCount { get; set; } = 0;
            public InputMaterialDetails CurrentMaterial { get; private set; } = new InputMaterialDetails();

            /// <summary>
            /// Threshold level to indicate supply is low and need attention, filling process will not stop
            /// </summary>
            [JsonIgnore] public int MaterialLowThreshold { get; private set; }
            /// <summary>
            /// Threshold level to indicate supply is empty and need to switch feeder
            /// </summary>
            [JsonIgnore] public int MaterialOutThreshold { get; private set; }
            /// <summary>
            /// Flag that indicate the remaining material or part is below the runnable amount
            /// Monitor this flag to decide this feeder could be used for feeding
            /// </summary>
            [JsonIgnore] public bool IsOut => MaterialBalance <= MaterialOutThreshold;

            /// <summary>
            /// Flag that indicate the remaining material or part is below warning level
            /// </summary>
            [JsonIgnore] public bool IsLow => MaterialBalance <= MaterialLowThreshold;
            /// <summary>
            /// Number of parts feed in per index
            /// </summary>
            const int perRowCount = 4;
            public Feeder(int number)
            {
                Number = number;
            }

            /// <summary>
            /// Method to update the input material remaining balance count during the material replenishment 
            /// </summary>
            /// <param name="input">Object that contains all the input material information</param>
            public void Replenish(InputMaterialDetails input)
            {
                if (input.TotalAmount < 0) return;
                if (input.TotalAmount % perRowCount != 0) return;


                MaterialUsedCount = 0;
                CurrentMaterial = input;
            }

            /// <summary>
            /// Method to set threshold for material low
            /// </summary>
            /// <param name="outVal">Threshold for material low</param>
            public void SetLowThreshold(int lowVal)
            {
                if (lowVal % perRowCount != 0) return;

                if (lowVal < 0) { MaterialLowThreshold = 0; }

                MaterialLowThreshold = (MaterialLowThreshold > MaterialOutThreshold) ? MaterialOutThreshold : lowVal;
            }

            /// <summary>
            /// Method to set threshold for material out
            /// </summary>
            /// <param name="outVal">Threshold for material out</param>
            public void SetOutThreshold(int outVal)
            {

                if (outVal % perRowCount != 0) return;

                if (outVal < 0) { MaterialLowThreshold = 0; }

                if (outVal < MaterialLowThreshold)
                {
                    MaterialLowThreshold = outVal;
                }

                MaterialOutThreshold = outVal;
            }
            /// <summary>
            /// Method to set threshold for material low, and set threshold when material is out
            /// </summary>
            /// <param name="lowVal">Threshold for material low</param>
            /// <param name="outVal">Threshold for material out</param>
            public void SetThresholds(int lowVal, int outVal)
            {
                SetOutThreshold(outVal);
                SetLowThreshold(lowVal);
            }
        }


        public enum FeederNum
        {
            None = -99,
            MaterialOut = -1,
            ThresholdNotSet = -2,
            Feeder1 = 1,
            Feeder2 = 2
        }
    }

}
