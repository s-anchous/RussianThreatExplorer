using System;
using System.Windows;
using System.IO;

namespace RussianThreatExplorer
{

    public partial class App : Application
    {
        public static readonly string DataDirectory = @"C:\ProgramData\RussianThreatExplorer";

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
