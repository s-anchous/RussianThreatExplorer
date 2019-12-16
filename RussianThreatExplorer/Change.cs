using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianThreatExplorer
{
    class Change
    {
        public enum ChangeType { Add, Remove, Edit }

        public static string Combine(string prev, string curr)
        {
            if (prev == curr)
                return curr;

            return $"Было: {prev.ToString() }\nСтало: {curr.ToString()}";
        }

        public Threat Previous;
        public Threat Current;
        public ChangeType Type;

        public Change(Threat previous, Threat current)
        {
            Previous = previous;
            Current = current;

            if (Previous == null) Type = ChangeType.Add;
            else if (Current == null) Type = ChangeType.Remove;
            else Type = ChangeType.Edit;
        }

        public string NumberChange()
        {
            return Combine(Previous.FullNumber, Current.FullNumber);
        }
        public string NameChange()
        {
            return Combine(Previous.Name, Current.Name);

        }
        public string DiscriptionChange()
        {
            return Combine(Previous.Discription, Current.Discription);
        }
        public string SourceChange()
        {
            return Combine(Previous.Source, Current.Source);
        }
        public string ObjectChange()
        {
            return Combine(Previous.Object, Current.Object);
        }
        public string IsPrivacyViolationChange()
        {
            return Combine(Previous.IsPrivacyViolation.MyToString(), Current.IsPrivacyViolation.MyToString());
        }
        public string IsIntegrityViolationCHange()
        {
            return Combine(Previous.IsIntegrityViolation.MyToString(), Current.IsIntegrityViolation.MyToString());
        }
        public string IsAccessibilityViolationChange()
        {
            return Combine(Previous.IsAccessibilityViolation.MyToString(), Current.IsAccessibilityViolation.MyToString());
        }

    }
}
