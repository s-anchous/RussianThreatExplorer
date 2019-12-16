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

namespace RussianThreatExplorer
{
    public delegate void ButtonAction(); 
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Threat> _list = new ObservableCollection<Threat>();
        private readonly ThreatContext _db = new ThreatContext();
        private int _pageCapacity = 20;
        private int _pageNumber = 0;

        ButtonAction messageButtonAction;
        ButtonAction threatButtonAction;

        private bool _showedAllChanges;
        private Queue<Change> _changes;

        public MainWindow()
        {
            InitializeComponent();
            ListOfThreats.ItemsSource = _list;

            if (!_db.Database.Exists()) FirstLaunch();
            else SetPage(0);
        }

        private void SetPage(int page)
        {
            if (page < 0 || page > _db.Threats.Count() / _pageCapacity)
                return;

            _pageNumber = page;
            PageLabel.Text = (_pageNumber + 1).ToString();

            _list.Clear();

            int startIndex = _pageNumber * _pageCapacity;

            var nextPageThreats = _db.Threats.OrderBy(x => x.Number).Skip(startIndex).Take(_pageCapacity);
            foreach (var item in nextPageThreats)
            {
                _list.Add(item);
            }
        }

        private void FirstLaunch()
        {
            _showedAllChanges = true;
            ShowMessage("Здравствуйте", "На данный момент у программы отсутствует база данных угроз ФСТЭК России. Скачать её с официального сайта?", UpdateDatabaseAsync, "Загрузить");
        }

        private async void UpdateDatabaseAsync()
        {

            LoadingLayer.Visibility = Visibility.Visible;
            try
            {
                _changes = await Task.Run(_db.TryUpdate);
            }
            catch (Exception e)
            {
                // Ошибка загрузки БД
                LoadingLayer.Visibility = Visibility.Collapsed;
                ShowMessage("Ошибка", e.Message, UpdateDatabaseAsync, "Повторить");
                return;
            }
            LoadingLayer.Visibility = Visibility.Collapsed;

            SetPage(0);
            
            // Если база была пустой
            if (_showedAllChanges)
            {
                _changes = null;
                HideMessage();
            }
            // Если изменений не было
            else if (_changes.Count == 0)
            {
                _showedAllChanges = true;
                ShowMessage("Обновление","Изменений не найдено", HideMessage);
            }
            // Если изменения есть
            else
            {
                HideMessage();
                ShowNextChange();
                threatButtonAction = ShowNextChange;
            }
        }
        private void ThreatLayerButton_Click(object sender, RoutedEventArgs e)
        {
            threatButtonAction?.Invoke();
        }

        private void MessageLayerButton_Click(object sender, RoutedEventArgs e)
        {
            messageButtonAction?.Invoke();
        }

        private void ToLeftPageButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(_pageNumber - 1);
        }

        private void ToRightPageButton_Click(object sender, RoutedEventArgs e)
        {
            SetPage(_pageNumber + 1);
        }

        private void UpdateDataBase_Click(object sender, RoutedEventArgs e)
        {
            _showedAllChanges = false;
            UpdateDatabaseAsync();
        }

        private void ListOfThreats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(ListOfThreats.SelectedItem is Threat item)) return;
            SetThreatToLayout(item);
            ShowThreatLayer();
            threatButtonAction = HideThreatLayer;
        }

        private void SetThreatToLayout(Threat t)
        {
            NumberLabel.Text = t.FullNumber.ToString();
            NameLabel.Text = t.Name;
            DiscriptionLabel.Text = t.Discription;
            SourceLabel.Text = t.Source;
            ObjectLabel.Text = t.Object;
            IsPrivacyViolationLabel.Text = t.IsPrivacyViolation.MyToString();
            IsIntegrityViolationLabel.Text = t.IsIntegrityViolation.MyToString();
            IsAccessibilityViolationLabel.Text = t.IsAccessibilityViolation.MyToString();
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

            ShowThreatLayer();
        }
        private void ShowNextChange()
        {
            if (_changes.Count > 0)
            {
                var change = _changes.Dequeue();
                ShowChangeThreat(change);
            }
            else
            {
                HideThreatLayer();
            }
        }

        private void ShowMessage(string title, string text, ButtonAction action, string buttonText = "Ок")
        {
            MessageLayerTitle.Content = title;
            MessageLayerText.Text = text;
            MessageLayerButton.Content = buttonText;

            messageButtonAction = action;

            MessageLayer.Visibility = Visibility.Visible;
        }

        private void HideMessage()
        {
            MessageLayer.Visibility = Visibility.Collapsed;
        }

        public void ShowThreatLayer()
        {
            ThreatLayer.Visibility = Visibility.Visible;
        }

        public void HideThreatLayer()
        {
            ThreatLayer.Visibility = Visibility.Collapsed;
        }
    }
}
