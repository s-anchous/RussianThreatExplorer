using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThreatViewer
{
    public class Threat : INotifyPropertyChanged
    {
        [Key]
        public int ID { get; set; }

        private int _number;
        public int Number
        {
            get
            {
                return _number;
            }

            set
            {
                _number = value;
                OnNotifyPropertyChanged("Number");
                OnNotifyPropertyChanged("FullNumber");
            }

        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                OnNotifyPropertyChanged("Name");
            }

        }

        public string Discription { get; set; }
        public string Source { get; set; }
        public string Object { get; set; }
        public bool IsPrivacyViolation { get; set; }
        public bool IsIntegrityViolation { get; set; }
        public bool IsAccessibilityViolation { get; set; }
        public long LastUpdateTime { get; set; }

        [NotMapped]
        public string FullNumber => "УБИ." + _number.ToString().PadLeft(3, '0');

        public Threat Clone()
        {
            return new Threat()
            {
                ID = ID,
                Number = Number,
                Name = Name,
                Discription = Discription,
                Source = Source,
                Object = Object,
                IsPrivacyViolation = IsPrivacyViolation,
                IsIntegrityViolation = IsIntegrityViolation,
                IsAccessibilityViolation = IsAccessibilityViolation,
                LastUpdateTime = LastUpdateTime
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnNotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
