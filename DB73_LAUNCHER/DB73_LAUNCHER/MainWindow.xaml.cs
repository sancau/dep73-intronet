namespace DB73_LAUNCHER
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Text;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string localPath;
        private string buildPath;
        private string buildInfoPath;
        private string localInfoPath;
        private string localVersion;
        private string serverVersion;
        private bool readyForSetup;

        public MainWindow()
        {
            InitializeComponent();

            this.Show();
            this.Background = Brushes.AliceBlue;

            string version = GetData(); // gets all required data from config files  
            CheckBuild(version);
        }

        private void UpdateBuild()
        {
            try
            {
                foreach(string dirPath in Directory.GetDirectories(buildPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(buildPath, localPath));
                }

                foreach(string newPath in Directory.GetFiles(buildPath, "*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(buildPath, localPath), true);
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Ошибка при копировании файлов, обратитесь к администратору!");
                this.Close();
            }
        }

        private void RunDB73()
        {
            try
            {
                this.Close();

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = localPath;
                startInfo.FileName = localPath + "db73.exe";
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Ошибка при запуске системы, обратитесь к администратору!");
                this.Close();
            }
        }

        private void CheckBuild(string version)
        {
            serverVersionLabel.Content = serverVersion;
            if (version == "not_installed") // if no local version detected
            {
                var config = File.ReadAllLines("launcher.conf", Encoding.GetEncoding("utf-8"));
                var newConfig = config.Where(line => !line.Contains(GetPCID()));
                File.WriteAllLines("launcher.conf", newConfig);

               
                CurrentStepInfo.Foreground = Brushes.Red;
                CurrentStepInfo.Text = "Установленные компоненты не обнаружены, укажите путь для их установки";
                pathLabel.Content = "Выберите путь установки программы!";
                pathLabel.Foreground = Brushes.Red;
                windowTitle.Content = "ПОДГОТОВКА К УСТАНОВКЕ";

				
                setPathButton.Visibility = Visibility.Visible;
                step2.Background = Brushes.LightGoldenrodYellow;

                localVersionLabel.Content = "Не обнаружена";
            }

            else //if program installed localy
            {
                CurrentStepInfo.Foreground = Brushes.Green;

                pathLabel.Content = localPath;
                pathLabel.Foreground = Brushes.Green;

                step2.Background = Brushes.LightGreen;
                step3.Background = Brushes.LightGoldenrodYellow;

                CurrentStepInfo.Text = "Обнаружены установленные компоненты";
                windowTitle.Content = "ПРОВЕРКА НАЛИЧИЯ ОБНОВЛЕНИЙ";

                setPathButton.Visibility = Visibility.Collapsed;
                setupButton.Visibility = Visibility.Collapsed;

                localVersionLabel.Content = localVersion;

                if (localVersion == serverVersion)
                {
                    step1.Background = Brushes.LightGreen;
                    step2.Background = Brushes.LightGreen;
                    step3.Background = Brushes.LightGreen;

                    windowTitle.Content = "СИСТЕМА ГОТОВА К ЗАПУСКУ";

                    pathLabel.Visibility = Visibility.Collapsed;

                    CurrentStepInfo.Text = "Все требуемые операции завершены успешно";

                    Start.Visibility = Visibility.Visible;
                }
                else
                {
                    step2.Background = Brushes.LightGreen;
                    step3.Background = Brushes.LightGoldenrodYellow;

                    System.Windows.MessageBox.Show("Обнаружено обновление! Сейчас необходимые фалы будут загружены на Ваш компьютер.");

                    windowTitle.Content = "Обновление системы";
                    CurrentStepInfo.Text = "Идет копирование файлов обновления. Это может занять несколько секунд...";

                    setPathButton.Visibility = Visibility.Collapsed;
                    setupButton.Visibility = Visibility.Collapsed;
                    pathLabel.Visibility = Visibility.Collapsed;

                    UpdateBuild();

                    string versionlocal = GetData();
                    CheckBuild(versionlocal);
                }
            }
        }

        //Gets config data
        private string GetData()
        {
            try
            {
                windowTitle.Content = "СБОР ДАННЫХ";
                CurrentStepInfo.Text = "Программа собирает необходимые данные... Это может занять несколько секунд";

                var encoding = Encoding.GetEncoding("utf-8");
                var config = File.ReadAllLines("launcher.conf", encoding);

                buildPath = config[0];
                buildInfoPath = config[0] + "config/buildInfo.conf";
                serverVersion = File.ReadAllText(buildInfoPath, encoding);

                if (config.Length == 1)
                {
                    step1.Background = Brushes.LightGreen;
                    return "not_installed";
                }

                string pcString = config.Where(str => str.Contains(GetPCID())).FirstOrDefault();

                if (pcString == null) 
                {
		            step1.Background = Brushes.LightGreen;
		            return "not_installed";
		        }

                localPath = pcString.Replace(GetPCID(), "");

                localInfoPath = localPath + "config/buildInfo.conf";

                try
                {
                    localVersion = File.ReadAllText(localInfoPath);
                }
                catch
                {
                    step1.Background = Brushes.LightGreen;
                    return "not_installed";
                }
                
                step1.Background = Brushes.LightGreen;
                return localVersion;
            }
            catch
            {
                System.Windows.MessageBox.Show("Ошибка при сборе данных! Обратитесь к администратору!");
                this.Close();
                return null;
            }
        }

        //setup button event handler
        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (readyForSetup)
            {
                try
                {
                    UpdateBuild();

                    var encoding = Encoding.GetEncoding("utf-8");
                    var config = File.ReadAllLines("launcher.conf", encoding);

                    File.AppendAllText("launcher.conf", "\n" + localPath + GetPCID());
			
                    string version = GetData(); 
                    CheckBuild(version);
                }
                catch(Exception)
                {
                    System.Windows.MessageBox.Show("Ошибка при копировании файлов, обратитесь к администратору!");
                    this.Close();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Недостаточно данных для начала установки!");
            }
        }

        //install path button event handler
        private void SetPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.ShowDialog();

            if (dialog.SelectedPath != "")
            {
                localPath = dialog.SelectedPath + @"\";

                pathLabel.Content = localPath;
                pathLabel.Foreground = Brushes.Green;

                if ((buildPath != null) && (localPath != null))
                {
	                setupButton.Visibility = Visibility.Visible;
                    readyForSetup = true;
                    windowTitle.Content = "УСТАНОВКА НЕОБХОДИМЫХ КОМПОНЕНТОВ";
                    CurrentStepInfo.Foreground = Brushes.Black;
                    CurrentStepInfo.Text = "Начните установку...";
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            RunDB73();
        }

        private string GetPCID()
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                break;
            }

            string drive = "C";
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();
            string volumeSerial = dsk["VolumeSerialNumber"].ToString();

            return cpuInfo + volumeSerial;
        }
    }
}
