using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThreatViewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Threat> _list = new ObservableCollection<Threat>();
        private readonly ThreatContext db = new ThreatContext();
        private int _pageCapacity = 15;
        private int _pageNumber = 0;

        private bool _needUpdateDatebase;
        private bool _showedAllChanges = true;
        private Queue<Change> _changes;

        private int StartThreatIndex => _pageNumber * _pageCapacity;

        public MainWindow()
        {
            InitializeComponent();
            ListOfThreats.ItemsSource = _list;

            if (!db.Database.Exists())
            {
                _showedAllChanges = true;
                _needUpdateDatebase = true;
                ShowMessage("Здравствуйте", "На данный момент у программы отсутствует база данных угроз ФСТЭК России. Скачать её с официального сайта?", "Загрузить");
                return;
            }

            SetPage(0);
        }

        private void SetPage(int page)
        {
            if (page < 0 || page > db.Threats.Count() / _pageCapacity)
                return;

            _pageNumber = page;
            PageLabel.Text = (_pageNumber + 1).ToString();

            _list.Clear();

            var nextPage = db.Threats.OrderBy(x => x.Number).Skip(StartThreatIndex).Take(_pageCapacity);
            foreach (var item in nextPage)
            {
                _list.Add(item);
            }
        }

        private void MessageLayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_needUpdateDatebase) UpdateDatabaseAsync();
            else HideMessage();
        }

        private async void UpdateDatabaseAsync()
        {
            _needUpdateDatebase = true;

            LoadingLayer.Visibility = Visibility.Visible;
            try
            {
                _changes = await Task.Run(db.Update);
            }
            catch (Exception e)
            {
                // Ошибка загрузки БД
                LoadingLayer.Visibility = Visibility.Collapsed;
                ShowMessage("Ошибка", e.Message, "Повторить");
                return;
            }
            LoadingLayer.Visibility = Visibility.Collapsed;
            _needUpdateDatebase = false;

            SetPage(0);
            
            // Если база была пустой
            if (_showedAllChanges)
            {
                _changes = null;
                HideMessage();
                return;
            }

            // Если изменений не было
            if (_changes.Count == 0)
            {
                _showedAllChanges = true;
                ShowMessage("Обновление","Изменений не найдено");
            }
            // Если изменения есть
            else
            {
                HideMessage();
                _showedAllChanges = false;
                ShowChangeThreat(_changes.Dequeue());
            }
        }

        private void ToLeftPageButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(_pageNumber - 1);
        }

        private void ToRightPageButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(_pageNumber + 1);
        }

        private void ListOfThreats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(ListOfThreats.SelectedItem is Threat item)) return;

            ShowThreat(item);

        }

        private void ShowThreat(Threat t)
        {
            SetThreatToLayout(t);
            ThreatLayer.Visibility = Visibility.Visible;
        }

        private void SetThreatToLayout(Threat t)
        {
            NumberLabel.Text = t.FullNumber.ToString();
            NameLabel.Text = t.Name;
            DiscriptionLabel.Text = t.Discription;
            SourceLabel.Text = t.Source;
            ObjectLabel.Text = t.Object;
            IsPrivacyViolationLabel.Text = t.IsPrivacyViolation ? "Да" : "Нет";
            IsIntegrityViolationLabel.Text = t.IsIntegrityViolation ? "Да" : "Нет";
            IsAccessibilityViolationLabel.Text = t.IsAccessibilityViolation ? "Да" : "Нет";
        }

        private void ShowChangeThreat(Change change)
        {
            switch (change.Type)
            {
                case Change.ChangeType.Add:
                    ThreatLayerStatusLabel.Content = "[Добавлено]";
                    SetThreatToLayout(change.Current);
                    break;
                case Change.ChangeType.Remove:
                    ThreatLayerStatusLabel.Content = "[Удалено]";
                    SetThreatToLayout(change.Previous);
                    break;
                case Change.ChangeType.Edit:
                    ThreatLayerStatusLabel.Content = "[Изменено]";

                    NumberLabel.Text = change.NumberChange();
                    NameLabel.Text = change.NameChange();
                    DiscriptionLabel.Text = change.DiscriptionChange();
                    SourceLabel.Text = change.SourceChange();
                    ObjectLabel.Text = change.ObjectChange();
                    IsPrivacyViolationLabel.Text = change.IsPrivacyViolationChange();
                    IsIntegrityViolationLabel.Text = change.IsIntegrityViolationCHange();
                    IsAccessibilityViolationLabel.Text = change.IsAccessibilityViolationChange();

                    break;
            }

            ThreatLayer.Visibility = Visibility.Visible;
        }

        private void ThreatLayerOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_showedAllChanges && _changes.Count > 0)
            {
                var change = _changes.Dequeue();
                ShowChangeThreat(change);
                return;
            }

            ThreatLayer.Visibility = Visibility.Collapsed;
            _showedAllChanges = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            list?.UnselectAll();
        }

        private void ShowMessage(string title, string text, string buttonText = "Ок")
        {
            MessageLayerTitle.Content = title;
            MessageLayerText.Text = text;
            MessageLayerButton.Content = buttonText;

            MessageLayer.Visibility = Visibility.Visible;
        }

        private void HideMessage()
        {
            MessageLayer.Visibility = Visibility.Collapsed;
        }

        private void UpdateDataBase_Click(object sender, RoutedEventArgs e)
        {
            _showedAllChanges = false;
            UpdateDatabaseAsync();
        }
    }
}
