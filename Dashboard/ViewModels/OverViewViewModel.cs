using Common;
using LiveCharts;
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
    public class OverViewViewModel : BindableBase
	{
        public OverViewViewModel()
        {
            LatencyViewModel = new LatencyViewModel();
            IOViewModel = new IOViewModel();
            IsReading = false;

            // 新建一个进程获取网络监控信息
            IsReading = !IsReading;
            Cts = new CancellationTokenSource();
            Ct = Cts.Token;
            if (IsReading) Task.Factory.StartNew(Read, Ct);
        }

	
        public LatencyViewModel LatencyViewModel { get; set; }
        public IOViewModel IOViewModel { get; set; }
        public static Config _config;

        // 配置信息
        public string HostName { get; set; } = "-";
        public string CpuBrand { get; set; } = "-";
        public string CoreNum { get; set; } = "-";
        public string MemCount { get; set; } = "-";
        public string KernelVersion { get; set; } = "-";
        public string OSName { get; set; } = "-";
        public string UpDays { get; set; } = "-";
        public string CountProcess { get; set; } = "-";
        public string LoggedSesion { get; set; } = "-";
        public string Ips { get; set; } = "";

        public CancellationToken Ct;
        public CancellationTokenSource Cts;

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
                    ssh.Disconnect();
                    break;
                }

                Thread.Sleep(1000);
                var now = DateTime.Now;
                key = Config.Ip + Config.User + Config.Port.ToString() + Config.Password;
                
                // 如果config有变动，更新config配置 或者首次连接
                if (Config.Ip != null && Config.User != null && Config.Port != 0 && Config.Password != null && oldKey != key)
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
                        var shellCommand = "osqueryi --json 'SELECT * FROM system_info;'";
                        var command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        var data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        HostName = data[0]["computer_name"].ToString();
                        CpuBrand = data[0]["cpu_brand"].ToString();
                        CoreNum = data[0]["cpu_physical_cores"].ToString();
                        var memCount= data[0]["physical_memory"].ToString();
                        MemCount = (int.Parse(memCount) / (1024 * 1024)).ToString();

                        RaisePropertyChanged("HostName");
                        RaisePropertyChanged("CpuBrand");
                        RaisePropertyChanged("CoreNum");
                        RaisePropertyChanged("MemCount");

                        shellCommand = "osqueryi --json 'select version from kernel_info;'";
                        command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        KernelVersion = data[0]["version"].ToString();
                        RaisePropertyChanged("KernelVersion");

                        shellCommand = "osqueryi --json 'select * from os_version;'";
                        command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        OSName = data[0]["name"].ToString() + " " + data[0]["version"].ToString();
                        RaisePropertyChanged("OSName");

                        shellCommand = "osqueryi --json 'select * from uptime'";
                        command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        UpDays = "运行" + data[0]["hours"].ToString() + "天,";
                        RaisePropertyChanged("UpDays");

                        shellCommand = "osqueryi --json ' select count(*) as total from processes'";
                        command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        CountProcess = "进程" + data[0]["total"].ToString() + "个,";
                        RaisePropertyChanged("CountProcess");

                        shellCommand = "osqueryi --json \"select count(*) as logged_session from logged_in_users where tty like 'pts%' and type = 'user';\"";
                        command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        LoggedSesion = "SSH登录" + data[0]["logged_session"].ToString() + "个";
                        RaisePropertyChanged("LoggedSesion");

                        shellCommand = "osqueryi --json \" select * from interface_addresses where interface like 'eth%' or interface like 'ens%';\"";
                        command = ssh.CreateCommand(shellCommand, System.Text.Encoding.UTF8);
                        result = command.Execute();
                        data = JsonConvert.DeserializeObject<List<JObject>>(result);
                        Ips = "";
                        foreach (JObject o in data)
                        {
                            Ips += o["interface"].ToString() + " | " + o["address"].ToString() + "\n";
                        }
                        RaisePropertyChanged("Ips");
                    }
                    catch (Exception e)
                    {
                        // TODO 处理异常
                    }
                }
                // end 
            }

            // 断开ssh 连接
            if (ssh != null)
            {
                ssh.Disconnect();
            }
        }
    }
}
