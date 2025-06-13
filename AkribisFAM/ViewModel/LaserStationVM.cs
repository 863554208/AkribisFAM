using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using AkribisFAM.Manager;
using AkribisFAM.Util;
using AkribisFAM.WorkStation;
using LiveCharts.Wpf;
using static AkribisFAM.WorkStation.Conveyor;

namespace AkribisFAM.ViewModel
{
    public class LaserStationVM : ViewModelBase
    {
        private AllProductTracker _productTracker;

        public AllProductTracker ProductTracker
        {
            get { return _productTracker; }
            set { _productTracker = value; OnPropertyChanged(); }
        }

        public LaserStationVM()
        {
            ProductTracker = App.productTracker;
            IntializeUpdateThread("LaserStationVM", 200);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LoadGoToView();
            });
        }

        public override void ThreadBody()
        {
            Station = Conveyor.Current.station;
            Status = Conveyor.Current.status;

            Steps = Conveyor.Current.steps;
            //Counters = Conveyor.Current.counters;
            StartTime = Conveyor.Current.startTime;

            stationReadyStatus = Conveyor.Current.StationReadyStatus;
            stationTrayStatus = Conveyor.Current.StationTrayStatus;
            traySendingNextStation = Conveyor.Current.TraySendingNextStation;
            ConveyorTrays = Conveyor.Current.ConveyorTrays;
            ConveyorTraysSending = Conveyor.Current.ConveyorTraysSending;

            DisplayItem[] temp = new DisplayItem[4];
            for (int i = 0; i < 4; i++)
            {
                temp[i] = new DisplayItem();
                temp[i].Station = Station[i].ToString();
                temp[i].Status = Status[i];
                temp[i].Steps = Steps[i];
                temp[i].StartTime = StartTime[i].ToString();
                temp[i].StationReadyStatus = StationReadyStatus[i];
                temp[i].StationTrayStatus = StationTrayStatus[i];
                temp[i].TraySendingNextStation = TraySendingNextStation[i];
                temp[i].ConveyorTrays = ConveyorTrays[i];
            }
            Items = temp;


            //bool[,] tempgo = new bool[4,9];
            //for (int i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 9; j++)
            //    {
            //        tempgo[i,j]
            //    }

            //}

            //SaveViewToGo();
            Conveyor.Current.Go = Go;

            //GoView = GoView;
        }
        public void SaveViewToGo()
        {
            //for (int i = 0; i < GoView.Count; i++)
            //{
            //    for (int j = 0; j < GoView[i].Count; j++)
            //    {
            //        go[i, j] = GoView[i][j].Value;
            //    }
            //}
        }

        public StationState[] Station
        {
            get { return station; }
            set { station = value; OnPropertyChanged(); }
        }

        public bool[] Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged(); }
        }
        private DisplayItem[] items = new DisplayItem[4];

        public DisplayItem[] Items
        {
            get { return items; }
            set { items = value; OnPropertyChanged(); }
        }

        public class DisplayItem
        {
            public string Station { get; set; }
            public bool Status { get; set; }
            public int Steps { get; set; }
            public string StartTime { get; set; }
            public bool StationReadyStatus { get; set; }
            public bool StationTrayStatus { get; set; }
            public bool TraySendingNextStation { get; set; }
            public TrayData ConveyorTrays { get; set; }
            public TrayData ConveyorTraysSending { get; set; }


        }

        //// Backing fields
        //private bool[,] go = new bool[4,9];
        private StationState[] station = new StationState[4];
        private bool[] status = new bool[4];
        private int[] steps = new int[4];
        //private int[] counters = new int[4];
        private DateTime[] starttime = new DateTime[4];

        private bool[] stationReadyStatus = new bool[4];
        private bool[] stationTrayStatus = new bool[4];
        private bool[] traySendingNextStation = new bool[4];
        private TrayData[] conveyorTrays = new TrayData[4];
        private TrayData[] conveyorTraysSending = new TrayData[4];

        // Properties
        public int[] Steps
        {
            get { return steps; }
            set { steps = value; OnPropertyChanged(); }
        }

        private bool[,,] go = new bool[4, 4, 20];
        public bool[,,] Go
        {
            get { return go; }
            set { go = value; OnPropertyChanged(); }
        }
        //public int[] Counters
        //{
        //    get { return counters; }
        //    set { counters = value; OnPropertyChanged(); }
        //}
        public ObservableCollection<ObservableCollection<BoolCell>> GoView { get; set; } =
    new ObservableCollection<ObservableCollection<BoolCell>>();

        private void LoadGoToView()
        {
            //GoView.Clear();
            //for (int i = 0; i < go.GetLength(0); i++)
            //{
            //    var row = new ObservableCollection<BoolCell>();
            //    for (int j = 0; j < go.GetLength(1); j++)
            //    {
            //        row.Add(new BoolCell { Row = i, Column = j, Value = go[i, j] });
            //    }
            //    GoView.Add(row);
            //}
        }
        public class BoolCell : INotifyPropertyChanged
        {
            private bool _value;
            public bool Value
            {
                get => _value;
                set { _value = value; OnPropertyChanged(); }
            }

            public int Row { get; set; }
            public int Column { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = "") =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public DateTime[] StartTime
        {
            get { return starttime; }
            set { starttime = value; OnPropertyChanged(); }
        }

        public bool[] StationReadyStatus
        {
            get { return stationReadyStatus; }
            set { stationReadyStatus = value; OnPropertyChanged(); }
        }

        public bool[] StationTrayStatus
        {
            get { return stationTrayStatus; }
            set { stationTrayStatus = value; OnPropertyChanged(); }
        }

        public bool[] TraySendingNextStation
        {
            get { return traySendingNextStation; }
            set { traySendingNextStation = value; OnPropertyChanged(); }
        }

        public TrayData[] ConveyorTrays
        {
            get { return conveyorTrays; }
            set { conveyorTrays = value; OnPropertyChanged(); }
        }

        public TrayData[] ConveyorTraysSending
        {
            get { return conveyorTraysSending; }
            set { conveyorTraysSending = value; OnPropertyChanged(); }
        }
    }
}
