using System;
using System.ComponentModel;

namespace WebKey.Models
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class KeyEntry : ViewModelBase
    {
        public KeyEntry() : this(string.Empty, string.Empty)
        {

        }

        public KeyEntry(string header, string detail, bool saved = true)
        {
            this.Header = header;
            this.Detail = detail;
            this.Saved = saved;
        }


        public int Id { get; set; }

        [NonSerialized]
        public bool Saved;

        private string header;
        public string Header
        {
            get { return header; }
            set
            {
                header = value;
                OnPropertyChanged("Header");
            }
        }

        private string detail;
        public string Detail
        {
            get { return detail; }
            set
            {
                detail = value;
                this.Saved = false;
                OnPropertyChanged("Detail");
            }
        }
    }
}
