using Common;
using Common.Events;
using Dashboard.Models;
using LiveCharts;
using LiveCharts.Configurations;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Renci.SshNet;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Dashboard.ViewModels
{
    public class LatencyViewModel : INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;
        private double _trend;
        private int _cpuUsage;

        public LatencyViewModel()
        {
            var mapper = Mappers.Xy<LatencyModel>()
                .X(model => model.DateTime.Ticks)
                .Y(model => model.Value);

            Charting.For<LatencyModel>(mapper);
            ChartValues = new ChartValues<LatencyModel>();
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");
            CpuFormatter = value => String.Format("{0}%", value.ToString());

            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            IsReading = false;
            // 新建一个进程获取网络监控信息
            IsReading = !IsReading;
            // start a task with a means to do a hard abort (unsafe!)
            Cts = new CancellationTokenSource();
            Ct = Cts.Token;
            if (IsReading) Task.Factory.StartNew(Read, Ct);

            //获取事件聚合器, 连接成功时开始监控
            eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            ConnectedEvent e = eventAggregator.GetEvent<ConnectedEvent>();
            e.Subscribe(GetConfig, ThreadOption.UIThread);
        }

        public ChartValues<LatencyModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public Func<double, string> CpuFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        // 事件聚合
        private IEventAggregator eventAggregator;

        public CancellationToken Ct;
        public CancellationTokenSource Cts;

        public static Config _config;

        public int CpuUsage
        {
            get { return _cpuUsage; }
            set
            {
                _cpuUsage = value;
                OnPropertyChanged("CpuUsage");
            }
        }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        public bool IsReading { get; set; }

        private void Read()
        {
            PasswordAuthenticationMethod authenticationMethod = null;
            SshClient ssh = null;
            String oldKey = null;
            String key = null;

            while (IsReading)
            {
                // 如何取消掉该任务
                if (Ct.IsCancellationRequested)
                {
                    // another thread decided to cancel
                    Console.WriteLine("latency page monitor task canceled");

                    if (ssh != null && ssh.IsConnected)
                    {
                        ssh.Disconnect();
                    }
                    break;
                }

                Thread.Sleep(1000);
                var now = DateTime.Now;
               

                key = Config.Ip + Config.User + Config.Port.ToString() + Config.Password;
                // 如果config有变动，更新config配置 或者首次连接
                if (Config.Ip != null && Config.User != null && Config.Port != 0 && Config.Ip != null && oldKey != key)
                {
                    oldKey = Config.Ip + Config.User + Config.Port.ToString() + Config.Password;
                    authenticationMethod =
                        new PasswordAuthenticationMethod(Config.User, Config.Password);
                    ssh = new SshClient(new ConnectionInfo(
                        Config.Ip,
                        Config.Port,
                        Config.User,
                        authenticationMethod));
                    ssh.Connect();
                }

                // cpu 监控结果
                string result = "0";
                if (ssh != null)
                {
                    try
                    {
                        var shellCommand = "top -bn2 -d1 | grep \"Cpu(s)\" |  sed \"s/.*, *\\([0-9.]*\\)%* id.*/\\1/\" | awk '{print 100 - $1}' | tail -1";
                        var command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        _trend = double.Parse(result);
                        CpuUsage = (int)Math.Floor(_trend);

                        ChartValues.Add(new LatencyModel
                        {
                            DateTime = now,
                            Value = _trend
                        });

                        SetAxisLimits(now);

                        //lets only use the last 150 values
                        if (ChartValues.Count > 150) ChartValues.RemoveAt(0);
                    }
                    catch (Exception e)
                    {
                        // TODO 处理异常
                    }
                }
                // end 
            }

            // 断开ssh 连接
            if (ssh != null && ssh.IsConnected)
            {
                ssh.Disconnect();
            }
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; 
            AxisMin = now.Ticks - TimeSpan.FromSeconds(149).Ticks;
        }

        // 属性更改处理
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 事件聚合处理函数
        public void GetConfig(Config c)
        {
            _config = c;
        }
    }
}
