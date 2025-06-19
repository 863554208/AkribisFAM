using AkribisFAM.CommunicationProtocol;
using AkribisFAM.Helper;
using AkribisFAM.Manager;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static AkribisFAM.CommunicationProtocol.Task_FeedupCameraFunction;
using static AkribisFAM.CommunicationProtocol.Task_PrecisionDownCamreaFunction;
using static AkribisFAM.CommunicationProtocol.Task_RecheckCamreaFunction;
using static AkribisFAM.GlobalManager;

namespace AkribisFAM.DeviceClass
{
    public class CognexVisionControl
    {
        List<VisionTravelPath> snapPalleteListv2 = new List<VisionTravelPath>();
        List<SinglePoint> RealPalletePointsList = new List<SinglePoint>();
        private List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> _feederVisionResult = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
        private List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> _bottomVisionResult = new List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition>();
        private List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition> _trayVisionResult = new List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition>();
        public enum FeederNum
        {
            Feeder1 = 1,
            Feeder2 = 2,
        }
        public enum VisionStation
        {
            TopVision,
            BottomVision,
            RecheckVision,
        }
        public enum OnTheFlyXDirection
        {
            Positive,
            Negative,
        }
        public enum OnTheFlyYDirection
        {
            Inward,
            Outward,
        }
        public enum TeachPointLocation
        {
            StartingPoint,
            EndingPoint,
        }
        public CognexVisionControl() { }
        private bool GetFoamXYPoints(FeederNum feeder, OnTheFlyXDirection direction, out List<SinglePoint> points)
        {
            points = new List<SinglePoint>();
            List<SinglePoint> teachpoints;
            switch (feeder)
            {
                case FeederNum.Feeder1:
                    teachpoints = GlobalManager.Current.feedar1Points;
                    break;
                case FeederNum.Feeder2:
                    teachpoints = GlobalManager.Current.feedar2Points;
                    break;
                default:
                    return false;
            }

            int numberOfFoam = 4;
            if (direction == OnTheFlyXDirection.Positive)
            {
                for (int i = 0; i < numberOfFoam; i++)
                {
                    points.Add(new SinglePoint()
                    {
                        X = teachpoints[(int)TeachPointLocation.StartingPoint].X + App.paramLocal.LiveParam.FoamXOffset * i,
                        Y = teachpoints[(int)TeachPointLocation.StartingPoint].Y,
                    });
                }
            }
            else
            {
                for (int i = numberOfFoam; i > 0; i--)
                {
                    points.Add(new SinglePoint()
                    {
                        X = teachpoints[(int)TeachPointLocation.StartingPoint].X + App.paramLocal.LiveParam.FoamXOffset * i,
                        Y = teachpoints[(int)TeachPointLocation.StartingPoint].Y,
                    });
                }
            }

            return true;
        }

        private bool GetBottomVisionXYPoints(OnTheFlyXDirection direction, out List<SinglePoint> points)
        {
            points = new List<SinglePoint>();
            List<SinglePoint> teachpoints = GlobalManager.Current.lowerCCDPoints;

            int numberOfFoam = 4;
            if (direction == OnTheFlyXDirection.Negative)
            {
                //1,2,3,4
                for (int i = 0; i < numberOfFoam; i++)
                {
                    points.Add(new SinglePoint()
                    {
                        X = teachpoints[(int)TeachPointLocation.StartingPoint].X - App.paramLocal.LiveParam.FoamXOffset * i,
                        Y = teachpoints[(int)TeachPointLocation.StartingPoint].Y,
                    });
                }
            }
            else
            {
                //
                for (int i = numberOfFoam; i > 0; i--)
                {
                    points.Add(new SinglePoint()
                    {
                        X = teachpoints[(int)TeachPointLocation.StartingPoint].X + App.paramLocal.LiveParam.FoamXOffset * i,
                        Y = teachpoints[(int)TeachPointLocation.StartingPoint].Y,
                    });
                }
            }

            return true;
        }
        public bool GetPalletStandbyPosition(Recipe recipe, out SinglePoint point)
        {
            point = new SinglePoint();
            if (!GetPalletXYPoints(recipe, out var discard, out var paths) && paths.Count() > 0)
            {
                return false;
            }
            point = paths[0].StartingPoint;
            return true;
        }


        public List<SinglePoint> GenerateLeftToRightGrid(
            double startX,
            double startY,
            double gapX,
            double gapY,
            int totalRows,
            int totalColumns)
        {
            var points = new List<SinglePoint>();

            for (int row = 0; row < totalRows; row++)
            {
                for (int col = 0; col < totalColumns; col++)
                {
                    points.Add(new SinglePoint
                    {
                        X = startX + col * gapX,
                        Y = startY + row * gapY
                    });
                }
            }

            return points;
        }
        public List<SinglePoint> RemapLeftToRightToSnake(List<SinglePoint> originalList, int totalRows, int totalColumns,
            int xDir, // +1 = L→R for first row, -1 = R→L for first row
            int yDir  // +1 = top→bottom, -1 = bottom→top
            )
        {
            var remappedList = new List<SinglePoint>();

            for (int logicalRow = 0; logicalRow < totalRows; logicalRow++)
            {
                int actualRow = (yDir == 1) ? logicalRow : (totalRows - 1 - logicalRow);

                bool isEvenRow = logicalRow % 2 == 0;
                bool leftToRight = (xDir == 1 && isEvenRow) || (xDir == -1 && !isEvenRow);

                for (int logicalCol = 0; logicalCol < totalColumns; logicalCol++)
                {
                    int actualCol = leftToRight ? logicalCol : (totalColumns - 1 - logicalCol);

                    int index = actualRow * totalColumns + actualCol;
                    remappedList.Add(originalList[index]);
                }
            }

            return remappedList;
        }


        public List<VisionTravelPath> GenerateSnakeTravelPathsWithDirection(
       List<SinglePoint> originalFlatList,
       int totalRows,
       int totalColumns,
       int xDir,     // +1: first row is L→R, -1: first row is R→L
       int yDir,     // +1: top→bottom, -1: bottom→top
       double xOffset)
        {
            var travelPaths = new List<VisionTravelPath>();

            for (int logicalRow = 0; logicalRow < totalRows; logicalRow++)
            {
                int actualRow = (yDir == 1) ? logicalRow : (totalRows - 1 - logicalRow);
                int rowStartIndex = actualRow * totalColumns;

                bool isEvenRow = logicalRow % 2 == 0;
                bool leftToRight = (xDir == 1 && isEvenRow) || (xDir == -1 && !isEvenRow);

                int firstCol = leftToRight ? 0 : totalColumns - 1;
                int lastCol = leftToRight ? totalColumns - 1 : 0;

                var firstPoint = originalFlatList[rowStartIndex + firstCol];
                var lastPoint = originalFlatList[rowStartIndex + lastCol];

                if (logicalRow == 0)
                {
                    // First row: true external start to end
                    travelPaths.Add(new VisionTravelPath
                    {
                        StartingPoint = new SinglePoint
                        {
                            X = firstPoint.X - xOffset * xDir,
                            Y = firstPoint.Y,
                            Z = firstPoint.Z,
                            R = firstPoint.R
                        },
                        EndingPoint = new SinglePoint
                        {
                            X = lastPoint.X + xOffset * xDir,
                            Y = lastPoint.Y,
                            Z = lastPoint.Z,
                            R = lastPoint.R
                        }
                    });
                }
                else
                {
                    var previousEnd = travelPaths.Last().EndingPoint;

                    travelPaths.Add(new VisionTravelPath
                    {
                        StartingPoint = new SinglePoint
                        {
                            X = previousEnd.X,
                            Y = firstPoint.Y,
                            Z = firstPoint.Z,
                            R = firstPoint.R
                        },
                        EndingPoint = new SinglePoint
                        {
                            X = lastPoint.X + xOffset * (leftToRight ? 1 : -1),
                            Y = lastPoint.Y,
                            Z = lastPoint.Z,
                            R = lastPoint.R
                        }
                    });
                }
            }

            return travelPaths;
        }



        public bool GetPalletXYPoints(Recipe recipe, out List<SinglePoint> partPoints, out List<VisionTravelPath> visionStartEndPaths)
        {
            partPoints = new List<SinglePoint>();
            visionStartEndPaths = new List<VisionTravelPath>();
            if (recipe == null)
            {
                return false;
            }

            var teachpoints = GlobalManager.Current.snapPalletePoints;
            double start_pos_X = teachpoints[(int)TeachPointLocation.StartingPoint].X;
            double start_pos_Y = teachpoints[(int)TeachPointLocation.StartingPoint].Y;
            int totalRow = recipe.PartRow;
            int totalColumn = recipe.PartColumn;
            double gap_X = recipe.XPitch;
            double gap_Y = recipe.YPitch;
            int direction_X = -1;
            int direction_Y = -1;
            double x_offset = App.paramLocal.LiveParam.FoamXOffset;

            var list = App.visionControl.GenerateLeftToRightGrid(start_pos_X, start_pos_Y, gap_X, gap_Y, totalRow, totalColumn);
            partPoints = App.visionControl.RemapLeftToRightToSnake(list, totalRow, totalColumn, direction_X, direction_Y); // -x , -y direction
            visionStartEndPaths = App.visionControl.GenerateSnakeTravelPathsWithDirection(list, totalRow, totalColumn, direction_X, direction_Y, x_offset);

            return true;
        }

        //public bool GetPalletXYPoints(Recipe recipe, out List<SinglePoint> partPoints, out List<VisionTravelPath> visionStartEndPaths)
        //{
        //    partPoints = new List<SinglePoint>();
        //    visionStartEndPaths = new List<VisionTravelPath>();

        //    if (recipe == null)
        //        return false;

        //    var teachpoints = GlobalManager.Current.snapPalletePoints;
        //    double start_pos_X = teachpoints[(int)TeachPointLocation.StartingPoint].X;
        //    double start_pos_Y = teachpoints[(int)TeachPointLocation.StartingPoint].Y;
        //    int totalRow = recipe.PartRow;
        //    int totalColumn = recipe.PartColumn;
        //    double gap_X = recipe.XPitch;
        //    double gap_Y = recipe.YPitch;

        //    double left_end_X = start_pos_X - (totalColumn - 1) * gap_X;

        //    bool reverse2 = true;
        //    RealPalletePointsList.Clear();
        //    for (int row = 0; row < totalRow; row++)
        //    {
        //        if (reverse2)
        //        {
        //            for (int column = 0; column < totalColumn; column++)
        //            {

        //                RealPalletePointsList.Add(new SinglePoint()
        //                {
        //                    X = start_pos_X - column * gap_X,
        //                    Y = start_pos_Y - row * gap_Y,
        //                });

        //            }
        //            reverse2 = false;
        //        }
        //        else
        //        {
        //            for (int column = 0; column < totalColumn; column++)
        //            {
        //                RealPalletePointsList.Add(new SinglePoint()
        //                {
        //                    X = left_end_X + column * gap_X,
        //                    Y = start_pos_Y - row * gap_Y,
        //                });

        //            }
        //            reverse2 = true;
        //        }


        //        double current_start_pos_X = start_pos_X;
        //        double current_start_pos_Y = start_pos_Y - row * gap_Y; // 当前行的起点Y坐标
        //        double current_end_pos_X = start_pos_X - (totalColumn - 1) * gap_X;
        //        double current_end_pos_Y = current_start_pos_Y; // 当前行的终点Y坐标（在同一行）]
        //        snapPalleteListv2.Add(new VisionTravelPath()
        //        {
        //            StartingPoint = new SinglePoint()
        //            {
        //                X = (row % 2 == 0 ? current_start_pos_X : current_end_pos_X) - gap_X,
        //                Y = row % 2 == 0 ? current_start_pos_Y : current_end_pos_Y,
        //            },
        //            EndingPoint = new SinglePoint()
        //            {
        //                X = (row % 2 == 0 ? current_end_pos_X : current_start_pos_X) + gap_X,
        //                Y = row % 2 == 0 ? current_end_pos_X : current_start_pos_Y,
        //            },

        //        });
        //    }
        //    return true;
        //}


        public bool SendTLMCommands(List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> TLMCommands)
        {
            //给Cognex发拍照信息
            if (!Task_FeedupCameraFunction.TriggFeedUpCamreaTLMSendData(FeedupCameraProcessCommand.TLM, TLMCommands))
            {
                Logger.WriteLog("Failed to send TLM");
                return false;
            }

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds <= App.paramLocal.LiveParam.ProcessTimeout)
            {
                if (Task_FeedupCameraFunction.TriggFeedUpCamreaready() == "OK")
                {
                    return true;
                }
            }
            Logger.WriteLog("TLM return is not 'OK'");
            return false;
        }

        public bool SendTLNCommands(List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> TLNCommands)
        {
            //给Cognex发拍照信息
            if (!Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNSendData(PrecisionDownCamreaProcessCommand.TLN, TLNCommands))
            {
                Logger.WriteLog("Failed to send TLN");
                return false;
            }

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds <= App.paramLocal.LiveParam.ProcessTimeout)
            {
                if (Task_PrecisionDownCamreaFunction.TriggDownCamreaready() == "OK")
                {
                    return true;
                }
            }
            Logger.WriteLog("TLN return is not 'OK'");
            return false;
        }

        public bool SendTLTCommands(List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> TLTCommands)
        {
            //给Cognex发拍照信息
            if (!Task_AssUpCameraFunction.TriggAssUpCamreaTLTSendData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT, TLTCommands))
            {
                Logger.WriteLog("Failed to send TLT");
                return false;
            }

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds <= App.paramLocal.LiveParam.ProcessTimeout)
            {
                if (Task_AssUpCameraFunction.TriggAssUpCamreaready() == "OK")
                {
                    return true;
                }
            }
            Logger.WriteLog("TLN return is not 'OK'");
            return false;
        }




        public bool XYPointToTLMCommand(List<SinglePoint> points, out List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> TLMCommands)
        {
            TLMCommands = new List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition>();

            if (points.Count < 1)
            {
                return false;
            }

            for (int i = 0; i < points.Count(); i++)
            {
                FeedUpCamrea.Pushcommand.SendTLMCamreaposition TLMCommand = new FeedUpCamrea.Pushcommand.SendTLMCamreaposition()
                {
                    SN1 = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    RawMaterialName1 = "Foam",
                    FOV = (i + 1).ToString(),
                    Photo_X1 = points[i].X.ToString(),
                    Photo_Y1 = points[i].Y.ToString(),
                    Photo_R1 = "0"
                };
                TLMCommands.Add(TLMCommand);
            }
            return true;

        }
        public bool XYPointToTLNCommand(List<SinglePoint> points, out List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> TLNCommands)
        {
            TLNCommands = new List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition>();

            if (points.Count < 1)
            {
                return false;
            }

            for (int i = 0; i < points.Count(); i++)
            {
                PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition TLNCommand = new PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition()
                {
                    SN = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    NozzleID = (i + 1).ToString(),
                    RawMaterialName = "Foam",
                    CaveID = "0",
                    TargetMaterialName1 = "Foam->Module",
                    Photo_X1 = points[i].X.ToString(),
                    Photo_Y1 = points[i].Y.ToString(),
                    Photo_R1 = "90.0",
                };
                TLNCommands.Add(TLNCommand);
            }
            return true;

        }
        public bool XYPointToTLTCommand(List<SinglePoint> points, out List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> TLTCommands)
        {
            TLTCommands = new List<AssUpCamrea.Pushcommand.SendTLTCamreaposition>();

            if (points.Count < 1)
            {
                return false;
            }

            for (int i = 0; i < points.Count(); i++)
            {
                AssUpCamrea.Pushcommand.SendTLTCamreaposition sendTLTCameraposition = new AssUpCamrea.Pushcommand.SendTLTCamreaposition()
                {
                    SN = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    NozzleID = "0",
                    MaterialTypeN1 = "Foam",
                    AcupointNumber = $"{i + 1}",
                    TargetMaterialName1 = "Foam->Module",
                    Photo_X1 = points[i].X.ToString(),
                    Photo_Y1 = points[i].Y.ToString(),
                    Photo_R1 = "0"
                };
                TLTCommands.Add(sendTLTCameraposition);
            }
            return true;

        }


        public bool GetTLMResult(out List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> TLM_returns)
        {
            ////接受Cognex的信息
            TLM_returns = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();
            TLM_returns = Task_FeedupCameraFunction.TriggFeedUpCamreaTLMAcceptData(FeedupCameraProcessCommand.TLM);

            if (TLM_returns == null || TLM_returns.Count == 0)
            {
                return false;
            }

            _feederVisionResult = TLM_returns;
            return true;
        }

        public bool GetTLNResult(out List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> TLN_returns)
        {
            ////接受Cognex的信息
            TLN_returns = Task_PrecisionDownCamreaFunction.TriggDownCamreaTLNAcceptData(PrecisionDownCamreaProcessCommand.TLN);

            if (TLN_returns == null || TLN_returns.Count == 0)
            {
                return false;
            }

            return true;
        }

        public bool GetTLTResult(out List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition> TLT_returns)
        {
            ////接受Cognex的信息
            List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition> msg_received = new List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition>();

            TLT_returns = Task_AssUpCameraFunction.TriggAssUpCamreaTLTAcceptData(Task_AssUpCameraFunction.AssUpCameraProcessCommand.TLT);
            if (TLT_returns == null || TLT_returns.Count == 0)
            {
                return false;
            }

            return true;
        }


        public bool SetAgitoXOnTheFlyModeOn(double startingPoint, double pitch, int eventSelect)
        {
            var numOfPitch = 3;
            AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, startingPoint, pitch, startingPoint + pitch * numOfPitch, eventSelect);

            AkrAction.Current.EventEnable(AxisName.FSX);

            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);

            Thread.Sleep(100);
            return true;
        }
        public bool SetAgitoXOnTheFlyModeOn(double startingPoint, double endingPoint, double pitch, int eventSelect)
        {
            AkrAction.Current.SetEventFixedGapPEG(AxisName.FSX, startingPoint, pitch, endingPoint, eventSelect);

            AkrAction.Current.EventEnable(AxisName.FSX);

            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response);

            Thread.Sleep(100);
            return true;
        }
        public bool SetAgitoXOnTheFlyModeOff()
        {
            AkrAction.Current.EventDisable(AxisName.FSX);
            AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response2);

            return true;
        }



        public bool VisionOnTheFlyFoam(FeederNum feeder, out List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition> messages, bool waitResult = true)
        {
            OnTheFlyXDirection direction = OnTheFlyXDirection.Positive;
            messages = new List<FeedUpCamrea.Acceptcommand.AcceptTLMFeedPosition>();

            _feederVisionResult.Clear();

            for (int i = 0; i < ZuZhuang.Current.PickPositions.Count(); i++)
            {
                ZuZhuang.Current.PickPositions[i].Reset();
            }

            if (!MoveToFoamVisionStandbyPos(feeder, direction))
            {
                return false;
            }

            //Config setting
            if (!GetFoamXYPoints(feeder, direction, out List<SinglePoint> points))
            {
                return false;
            }
            if (!XYPointToTLMCommand(points, out List<FeedUpCamrea.Pushcommand.SendTLMCamreaposition> commands))
            {
                return false;
            }
            if (!SendTLMCommands(commands))
            {
                return false;
            }
            SetAgitoXOnTheFlyModeOn(points[(int)TeachPointLocation.StartingPoint].X, App.paramLocal.LiveParam.FoamXOffset, 1);

            //移动到拍照结束点
            if (!MoveToFoamVisionEndingPos(feeder, direction))
            {
                return false;
            }

            SetAgitoXOnTheFlyModeOff();
            if (waitResult)
            {
                if (!GetTLMResult(out messages))
                {
                    return false;
                }
            }

            Logger.WriteLog("feedar飞拍接收到的消息为:" + _feederVisionResult[0].Errcode1);
            return true;
        }

        public bool Vision1OnTheFlyPalletTrigger(Recipe recipe, bool waitResult = true)
        {
            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }

            if (!GetPalletXYPoints(recipe, out List<SinglePoint> points, out List<VisionTravelPath> travelPaths))
            {
                return false;
            }

            if (!XYPointToTLTCommand(points, out List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> commands))
            {
                return false;
            }
            if (!VisionTriggerMove(points, travelPaths, commands))
            {
                return false;
            }
            if (waitResult)
            {
                if (!GetTLTResult(out List<AssUpCamrea.Acceptcommand.AcceptTLTRunnerPosition> messages))
                {
                    return false;
                }
                _trayVisionResult = messages;
            }

            return true;
        }

        public bool Vision2OnTheFlyTrigger(out List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition> messages, bool waitResult = true)
        {
            messages = new List<PrecisionDownCamrea.Acceptcommand.AcceptTLNDownPosition>();
            OnTheFlyXDirection direction = OnTheFlyXDirection.Negative;
            Logger.WriteLog("CCD2准备移动到拍照位置");
            if (!MoveToBottomVisionStandbyPos(direction))
            {
                return false;
            }

            if (!GetBottomVisionXYPoints(direction, out List<SinglePoint> points))
            {
                return false;
            }
            if (!XYPointToTLNCommand(points, out List<PrecisionDownCamrea.Pushcommand.SendTLNCamreaposition> commands))
            {
                return false;
            }
            if (!App.assemblyGantryControl.ZCamPosAll())
            {
                App.assemblyGantryControl.ZUpAll();
                return false;
            }
            if (!SendTLNCommands(commands))
            {
                App.assemblyGantryControl.ZUpAll();
                return false;
            }

            SetAgitoXOnTheFlyModeOn(points[(int)TeachPointLocation.StartingPoint].X, -App.paramLocal.LiveParam.FoamXOffset, 2);

            if (!MoveToBottomVisionEndingPos(direction))
            {
                App.assemblyGantryControl.ZUpAll();
                return false;
            }

            Logger.WriteLog("结束CCD2运动1");
            SetAgitoXOnTheFlyModeOff();


            //接受Cognex信息
            if (waitResult)
            {
                if (!GetTLNResult(out messages))
                {
                    App.assemblyGantryControl.ZUpAll();
                    return false;
                }
                _bottomVisionResult = messages;
            }

            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }
            return true;
        }


        public bool MoveToBottomVisionStandbyPos(OnTheFlyXDirection direction, bool waitMotionDone = true)
        {
            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }

            if (!GetBottomVisionStandbyPos(direction, out SinglePoint point))
            {
                return false;
            }

            //移动到拍照起始点
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        public bool MoveToBottomVisionEndingPos(OnTheFlyXDirection direction, bool bypassCheckZ = false, bool waitMotionDone = true)
        {
            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }

            if (!GetBottomVisionEndingPos(direction, out SinglePoint point))
            {
                return false;
            }

            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, bypassCheckZ, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }

        public bool MoveToPalletVisionStartingPos(Recipe recipe, bool waitMotionDone = true)
        {
            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }

            if (!GetPalletStandbyPosition(recipe, out SinglePoint point))
            {
                return false;
            }

            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }

        public bool IsAllFeederVisionOK()
        {
            var isAllFeederVisionOK = _feederVisionResult.Count == 4 && _feederVisionResult.All(x => x.Errcode1 == "1");
            return isAllFeederVisionOK;
        }
        public string GetFailedFoamResult()
        {
            string failedMessage = "";
            var failedResults = _feederVisionResult.Where(x => x.Errcode1 != "1");
            if (failedResults.Count() < 1)
            {
                return "";
            }
            foreach (var failedResult in failedResults)
            {
                failedMessage += $"Foam {failedResult.FOV1} failed";
            }
            return failedMessage;
        }
        public bool IsAllTrayVisionOK()
        {
            var isAllTrayVisionOK = _trayVisionResult.All(x => x.Errcode == "1");
            return isAllTrayVisionOK;
        }

        public bool TurnOffTheFlyPEG()
        {

            return AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=0", out string response3);
        }
        public bool TurnOnTheFlyPEG()
        {

            return AAmotionFAM.AGM800.Current.controller[0].SendCommandString("CeventOn=1", out string response3);
        }
        private bool VisionTriggerMove(List<SinglePoint> points, List<VisionTravelPath> paths, List<AssUpCamrea.Pushcommand.SendTLTCamreaposition> commands)
        {
            if (!App.assemblyGantryControl.ZUpAll())
                return false;

            //移动到拍照起始点

            for (int i = 0; i < paths.Count; i++)
            {
                var startingPoint = paths[i].StartingPoint;
                var endingPoint = paths[i].EndingPoint;
                SetAgitoXOnTheFlyModeOff();

                if (AkrAction.Current.MoveFoamXY(startingPoint.X, startingPoint.Y) != (int)AkrAction.ACTTION_ERR.NONE)
                {
                    return false;
                }

                if (i == 0 && !SendTLTCommands(commands))
                {
                    return false;
                }

                SetAgitoXOnTheFlyModeOn(points[1 + i * 4].X, points[1 + i * 4].X, App.paramLocal.LiveParam.FoamXOffset, 1);

                if (AkrAction.Current.MoveFoamXY(endingPoint.X, endingPoint.Y) != (int)AkrAction.ACTTION_ERR.NONE)
                {
                    return false;
                }

                SetAgitoXOnTheFlyModeOff();
            }
            return true;
        }

        private bool GetFoamTravelPath(FeederNum feeder, out VisionTravelPath path)
        {
            List<SinglePoint> points = new List<SinglePoint>();
            path = new VisionTravelPath();
            switch (feeder)
            {
                case FeederNum.Feeder1:
                    points = GlobalManager.Current.feedar1Points;
                    break;
                case FeederNum.Feeder2:
                    points = GlobalManager.Current.feedar2Points;
                    break;
                default:
                    return false;
            }

            path = new VisionTravelPath()
            {
                StartingPoint = points[0],
                EndingPoint = points[1],
            };

            return true;
        }
        private bool GetBottomVisionTravelPath(out VisionTravelPath path)
        {
            List<SinglePoint> points = GlobalManager.Current.lowerCCDPoints;
            path = new VisionTravelPath()
            {
                StartingPoint = points[0],
                EndingPoint = points[1],
            };

            return true;
        }
        private bool GetBottomVisionStandbyPos(OnTheFlyXDirection direction, out SinglePoint point)
        {
            point = new SinglePoint();
            var gap_X = App.paramLocal.LiveParam.FoamXOffset;
            var sign = (direction == OnTheFlyXDirection.Positive) ? -1 : 1;

            if (!GetBottomVisionTravelPath(out VisionTravelPath path))
            {
                return false;
            }
            if (direction == OnTheFlyXDirection.Positive)
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X - gap_X * 4,
                    Y = path.StartingPoint.Y,
                };
            }
            else
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X + gap_X,
                    Y = path.StartingPoint.Y,
                };
            }
            return true;
        }
        private bool GetBottomVisionEndingPos(OnTheFlyXDirection direction, out SinglePoint point)
        {
            point = new SinglePoint();
            var gap_X = App.paramLocal.LiveParam.FoamXOffset;

            if (!GetBottomVisionTravelPath(out VisionTravelPath path))
            {
                return false;
            }

            if (direction == OnTheFlyXDirection.Positive)
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X + gap_X,
                    Y = path.StartingPoint.Y,
                };
            }
            else
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X - gap_X * 4,
                    Y = path.StartingPoint.Y,
                };
            }

            return true;
        }
        public bool GetFoamStandbyPos(FeederNum feeder, OnTheFlyXDirection direction, out SinglePoint point)
        {
            point = new SinglePoint();
            var gap_X = App.paramLocal.LiveParam.FoamXOffset;

            if (!GetFoamTravelPath(feeder, out VisionTravelPath path))
            {
                return false;
            }
            if (direction == OnTheFlyXDirection.Positive)
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X - gap_X,
                    Y = path.StartingPoint.Y,
                };
            }
            else
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X + gap_X * 4,
                    Y = path.StartingPoint.Y,
                };
            }

            return true;
        }
        private bool GetFoamEndingPos(FeederNum feeder, OnTheFlyXDirection direction, out SinglePoint point)
        {
            point = new SinglePoint();
            var gap_X = App.paramLocal.LiveParam.FoamXOffset;

            if (!GetFoamTravelPath(feeder, out VisionTravelPath path))
            {
                return false;
            }
            if (direction == OnTheFlyXDirection.Positive)
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X + gap_X * 4,
                    Y = path.StartingPoint.Y,
                };
            }
            else
            {
                point = new SinglePoint()
                {
                    X = path.StartingPoint.X - gap_X,
                    Y = path.StartingPoint.Y,
                };
            }
            return true;
        }


        public bool MoveToFoamVisionStandbyPos(FeederNum feeder, OnTheFlyXDirection direction, bool waitMotionDone = true)
        {
            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }

            if (!GetFoamStandbyPos(feeder, direction, out SinglePoint point))
            {
                return false;
            }

            //移动到拍照起始点
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone: waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }

        public bool MoveToFoamVisionEndingPos(FeederNum feeder, OnTheFlyXDirection direction, bool waitMotionDone = true)
        {
            if (!App.assemblyGantryControl.ZUpAll())
            {
                return false;
            }

            if (!GetFoamEndingPos(feeder, direction, out SinglePoint point))
            {
                return false;
            }

            //移动到拍照起始点
            if (AkrAction.Current.MoveFoamXY(point.X, point.Y, waitMotionDone: waitMotionDone) != (int)AkrAction.ACTTION_ERR.NONE)
            {
                return false;
            }
            return true;
        }
        public bool Trigger(VisionStation station)
        {
            IO_OutFunction_Table output;
            switch (station)
            {
                case VisionStation.TopVision:
                    output = IO_OutFunction_Table.OUT5_5PnP_Gantry_Camera_Trig;
                    break;
                case VisionStation.BottomVision:
                    output = IO_OutFunction_Table.OUT5_6Feeder_Camera_Trig;
                    break;
                case VisionStation.RecheckVision:
                    output = IO_OutFunction_Table.OUT5_7Recheck_Camera_Trig;
                    break;
                default:
                    return false;
            }
            if (IOManager.Instance.IO_ControlStatus(output, 0) && IOManager.Instance.IO_ControlStatus(output, 1))
            {
                return false;
            }
            return true;
        }

        public bool CheckFilm(int index, int totalRow, int totalColumn)
        {
            //return true;
            var trayIndex = AlgorithmHelper.GetZigzagIndexFromFlat(index, totalRow, totalColumn);
            string command = "SN" + "sqcode" + $"+{trayIndex}," + $"{trayIndex}," + "Foam+Moudel," + "0.000,0.000,0.000";
            TriggRecheckCamreaTFCSendData(RecheckCamreaProcessCommand.TFC, command);

            Logger.WriteLog("CCD3 开始接受COGNEX的OK信息");
            DateTime start = DateTime.Now;
            while ((start - DateTime.Now).TotalMilliseconds < 3000)
            {
                if (Task_RecheckCamreaFunction.TriggRecheckCamreaready() != "OK")
                {
                    string res = "接收到的信息是:" + Task_RecheckCamreaFunction.TriggRecheckCamreaready();
                    return true;
                }
            }
            Logger.WriteLog("CCD3 接受到COGNEX的OK信息");

            return true;

        }

        public class VisionTravelPath
        {
            public SinglePoint StartingPoint { get; set; } = new SinglePoint();
            public SinglePoint EndingPoint { get; set; } = new SinglePoint();
        }

    }
}
