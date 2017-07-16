using Common;
using Dashboard.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dashboard.ViewModels
{
    public class IOViewModel : BindableBase
	{
        private double _trend;
        private double _axisMax;
        private double _axisMin;

        public IOViewModel()
        {
            // 初始化变量
            MemChartValue = new ChartValues<LatencyModel>();
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");
            MBFormatter = value => String.Format("{0}MB", value.ToString());
            SetAxisLimits(DateTime.Now);

            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;

            IsReading = false;
            // 新建一个进程获取网络监控信息
            IsReading = !IsReading;
            // start a task with a means to do a hard abort (unsafe!)
            Cts = new CancellationTokenSource();
            Ct = Cts.Token;
            if (IsReading) Task.Factory.StartNew(Read, Ct);
            TotalMem = 1024;
        }

	    public string[] Labels { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public Func<double, string> MBFormatter { get; set; }

        public ChartValues<LatencyModel> MemChartValue { get; set; }

        public int MemUsage { get; set; }
        public int TotalMem { get; set; }

        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        // 任务控制
        public CancellationToken Ct;
        public CancellationTokenSource Cts;

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                RaisePropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                RaisePropertyChanged("AxisMin");
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
                    Console.WriteLine("mem page monitor task canceled");

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
                        var shellCommand = "osqueryi -json 'select * from memory_info'";
                        var command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        var data = JsonConvert.DeserializeObject<List<JObject>>(result);

                        _trend = double.Parse(data[0]["memory_total"].ToString()) - double.Parse(data[0]["memory_free"].ToString());
                        MemUsage = (int)Math.Floor(_trend / 1024 / 1024 );
                        RaisePropertyChanged("MemUsage");

                        TotalMem = (int)Math.Floor(double.Parse(data[0]["memory_total"].ToString()) / 1024 / 1024);
                        RaisePropertyChanged("TotalMem");

                        MemChartValue.Add(new LatencyModel
                        {
                            DateTime = now,
                            Value = MemUsage
                        });

                        SetAxisLimits(now);

                        //lets only use the last 150 values
                        if (MemChartValue.Count > 150) MemChartValue.RemoveAt(0);
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
    }
}
