using System;
using System.Windows;
using System.IO;

namespace ThreatViewer
{

    public partial class App : Application
    {
        public static readonly string DataDirectory = @"C:\ProgramData\ThreatViewer";

        [STAThread]
        public static void Main()
        {
            if (!Directory.Exists(DataDirectory))
            {
                try
                {
                    Directory.CreateDirectory(DataDirectory);
                }
                catch
                {
                    MessageBox.Show("Произошла ошибка при инициализации приложения. Обратитесь к администратору", "Ошибка", MessageBoxButton.OK);
                    return;
                }
            }
                


            var application = new App();
            application.InitializeComponent();
            application.Run();
        }


    }
}
