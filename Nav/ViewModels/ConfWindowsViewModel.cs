using Common;
using Common.Events;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Nav.ViewModels
{
    public class ConfWindowsViewModel : BindableBase
    {
        public ConfWindowsViewModel()
        {
            ConnectCommand = new DelegateCommand(Connect);
            // 自动写入配置文件
            if (File.Exists("config.json"))
            {
                String s = File.ReadAllText("config.json");
                JObject o = JsonConvert.DeserializeObject<JObject>(s);

                Ip = o["Ip"].ToString();
                User = o["User"].ToString();
                Port = (int)o["Port"];
                Password = o["Password"].ToString();
            }
        }

        public String Ip { get; set; } = "";
        public int Port { get; set; } = 22;
        public String User { get; set; } = "root";
        public String Password { get; set; } = "";

        public int ConnectionProgress { get; set; } = 0;
        public SolidColorBrush ProgressColor { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00B32A"));
        public String Progressvisibility { get; set; } = "hidden";

        public List<Config> ListBoxSource { get; set; } = new List<Config> { };
        public Config SelectedItem { get; set; }

        public IEventAggregator eventAggregator;

        public DelegateCommand ConnectCommand { get; private set; }
        public async void Connect()
        {
            Connected();
            SolidColorBrush errorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF6633"));
            SolidColorBrush successBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00B32A"));

            Func<Task> taskFunc = () =>
            {
                ProgressColor = successBrush;
                RaisePropertyChanged("ProgressColor");

                return Task.Run(() =>
                {
                    Progressvisibility = "visible";
                    RaisePropertyChanged("ProccessVisibility");

                    AuthenticationMethod authenticationMethod =
                    new PasswordAuthenticationMethod(User, Password);

                    ConnectionProgress = 20;
                    RaisePropertyChanged("ConnectionProgress");

                    using (var ssh =
                        new SshClient(new ConnectionInfo(
                            Ip,
                            Port,
                            User,
                            authenticationMethod)))
                    {
                        try
                        {
                            ssh.Connect();
                            var command = ssh.CreateCommand("uptime");
                            var result = command.Execute();
                            Console.Out.WriteLine(result);
                            ssh.Disconnect();

                            // 存储文件到本地
                            JObject o = new JObject();
                            o["Ip"] = Ip;
                            o["User"] = User;
                            o["Port"] = Port;
                            o["Password"] = User;
                            File.WriteAllText(@"config.json", o.ToString(), Encoding.UTF8);

                            // 发布事件
                            Connected();
                        }
                        catch (Exception e)
                        {
                            ProgressColor = errorBrush;
                            RaisePropertyChanged("ProgressColor");
                        }
                    }

                    ConnectionProgress = 100;
                    RaisePropertyChanged("ConnectionProgress");
                });
            };

            await taskFunc();
        }

        // 暂时没有用，用作事件
        public void Connected()
        {
            Config config = new Config();
            Config.Ip = Ip;
            Config.User = User;
            Config.Port = Port;
            Config.Password = Password;

            eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<ConnectedEvent>().Publish(config);
        }
    }
}
