using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using AkribisFAM.CommunicationProtocol;
using System.Timers;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for IOControlView.xaml
    /// </summary>
    public partial class IOControlView : UserControl
    {
        IOControlVM vm = new IOControlVM();
        Timer _timer;
        ObservableCollection<DigitalInputVM> _allinputs = new ObservableCollection<DigitalInputVM>();
        ObservableCollection<DigitalOutputVM> _alloutput = new ObservableCollection<DigitalOutputVM>();
        List<string> Tags = new List<string>();
        List<string> Groups = new List<string>();
        public IOControlView()
        {
            InitializeComponent();
            App.Current.Exit += Current_Exit;

            _timer = new Timer(200);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;

            var enumList = EnumMetadataHelper.ToEnumItemList<IO_INFunction_Table>();
            _allinputs = new ObservableCollection<DigitalInputVM>();
            foreach (var enumItem in enumList)
            {
                _allinputs.Add(new DigitalInputVM()
                {
                    Enum = enumItem,
                });
            }
            var enumList2 = EnumMetadataHelper.ToEnumItemList<IO_OutFunction_Table>();
            _alloutput = new ObservableCollection<DigitalOutputVM>();
            foreach (var enumItem2 in enumList2)
            {
                _alloutput.Add(new DigitalOutputVM()
                {
                    Enum = enumItem2,
                });
            }
            var allinputtags = _allinputs.Select(x => x.Enum.Tags).ToList();
            var alloutputtags = _alloutput.Select(x => x.Enum.Tags).ToList();
            allinputtags.AddRange(alloutputtags);
            Tags.AddRange(allinputtags.SelectMany(x => x).Distinct().ToList());
            cbxTag.ItemsSource = Tags;

            var allinputgroups = _allinputs.Select(x => x.Enum.EEGroup).ToList();
            var alloutputgroups = _alloutput.Select(x => x.Enum.EEGroup).ToList();
            allinputgroups.AddRange(alloutputgroups);
            Groups.AddRange(allinputgroups.Select(x => x).Distinct().ToList());
            cbxTag.ItemsSource = Tags;
            cbxModule.ItemsSource = Groups;


            vm.Inputs = new ObservableCollection<DigitalInputVM>(_allinputs);
            vm.Outputs = new ObservableCollection<DigitalOutputVM>(_alloutput);
            DataContext = vm;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsVisible)
            {
                if (IOManager.Instance.INIO_status.Length != vm.Inputs.Count())
                    return;


                for (int i = 0; i < IOManager.Instance.INIO_status.Length; i++)
                {
                    vm.Inputs[i].Status = IOManager.Instance.INIO_status[i] == 1;
                }


                if (IOManager.Instance.OutIO_status.Length != vm.Outputs.Count())
                    return;


                for (int i = 0; i < IOManager.Instance.OutIO_status.Length; i++)
                {
                    vm.Outputs[i].Status = IOManager.Instance.OutIO_status[i] == 1;
                }
            }
        }

        private void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            vm.PauseUpdateThread();
            vm.TerminateUpdateThread();
        }
        public class IOControlVM : ViewModelBase
        {
            private ObservableCollection<DigitalInputVM> _inputs = new ObservableCollection<DigitalInputVM>();

            public ObservableCollection<DigitalInputVM> Inputs
            {
                get { return _inputs; }
                set { _inputs = value; OnPropertyChanged(); }
            }


            private ObservableCollection<DigitalOutputVM> _outputs = new ObservableCollection<DigitalOutputVM>();

            public ObservableCollection<DigitalOutputVM> Outputs
            {
                get { return _outputs; }
                set { _outputs = value; OnPropertyChanged(); }
            }


        }
        public class DigitalInputVM : ViewModelBase
        {
            private EnumItem<IO_INFunction_Table> _input;

            public EnumItem<IO_INFunction_Table> Enum
            {
                get { return _input; }
                set { _input = value; OnPropertyChanged(); }
            }

            private bool _status;

            public bool Status
            {
                get { return _status; }
                set { _status = value; OnPropertyChanged(); }
            }
        }

        public class DigitalOutputVM : ViewModelBase
        {
            private EnumItem<IO_OutFunction_Table> _output;

            public EnumItem<IO_OutFunction_Table> Enum
            {
                get { return _output; }
                set { _output = value; OnPropertyChanged(); }
            }

            private bool _status;

            public bool Status
            {
                get { return _status; }
                set { _status = value; OnPropertyChanged(); }
            }
        }

        private void btnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != "")
            {
                btnClear.IsEnabled = true;
                cbxModule.Text = "";
                cbxTag.Text = "";
                var foundInputs = vm.Inputs.Where(x => x.Enum.Value.ToString().Contains(txtSearch.Text)).ToList();
                vm.Inputs = new ObservableCollection<DigitalInputVM>(foundInputs);

                var foundOutputs = vm.Outputs.Where(x => x.Enum.Value.ToString().Contains(txtSearch.Text)).ToList();
                vm.Outputs = new ObservableCollection<DigitalOutputVM>(foundOutputs);
            }
            else
            {
                btnClear.IsEnabled = false;
                vm.Inputs = new ObservableCollection<DigitalInputVM>(_allinputs);
                vm.Outputs = new ObservableCollection<DigitalOutputVM>(_alloutput);
            }
        }

        private void txtSearch_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void cbxModule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbxTag.Text = "";
            txtSearch.Text = "";
            cbxTag.Text = "";
            vm.Inputs = new ObservableCollection<DigitalInputVM>(_allinputs);
            vm.Outputs = new ObservableCollection<DigitalOutputVM>(_alloutput);
            if (cbxModule.Text != "")
            {
                var foundInputs = vm.Inputs.Where(x => x.Enum.EEGroup.Contains(cbxModule.Text)).ToList();
                vm.Inputs = new ObservableCollection<DigitalInputVM>(foundInputs);

                var foundOutputs = vm.Outputs.Where(x => x.Enum.EEGroup.Contains(cbxModule.Text)).ToList();
                vm.Outputs = new ObservableCollection<DigitalOutputVM>(foundOutputs);
            }
            else
            {
                vm.Inputs = new ObservableCollection<DigitalInputVM>(_allinputs);
                vm.Outputs = new ObservableCollection<DigitalOutputVM>(_alloutput);
            }
        }

        private void cbxTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbxModule.Text = "";
            txtSearch.Text = "";
            cbxModule.Text = "";
            vm.Inputs = new ObservableCollection<DigitalInputVM>(_allinputs);
            vm.Outputs = new ObservableCollection<DigitalOutputVM>(_alloutput);
            if (cbxTag.Text != "")
            {
                var foundInputs = vm.Inputs.Where(x => x.Enum.Tags.Contains(cbxTag.Text)).ToList();
                vm.Inputs = new ObservableCollection<DigitalInputVM>(foundInputs);

                var foundOutputs = vm.Outputs.Where(x => x.Enum.Tags.Contains(cbxTag.Text)).ToList();
                vm.Outputs = new ObservableCollection<DigitalOutputVM>(foundOutputs);
            }
            else
            {
                vm.Inputs = new ObservableCollection<DigitalInputVM>(_allinputs);
                vm.Outputs = new ObservableCollection<DigitalOutputVM>(_alloutput);
            }
        }
    }
}
