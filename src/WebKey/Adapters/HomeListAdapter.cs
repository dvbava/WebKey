using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using WebKey.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebKey.Adapters
{
    public class HomeListAdapter : BaseAdapter, IListAdapter
    {
        private Activity _activity;
        private string _filter = null;
        ObservableCollection<KeyEntry> _itemSource = null;
        List<KeyEntry> _projection = null;

        public HomeListAdapter(Activity activity, ObservableCollection<KeyEntry> items)
        {
            this._activity = activity;
            this._itemSource = items;
            this._projection = new List<KeyEntry>(items);
            this._itemSource.CollectionChanged += (s, e) => Refresh();
        }

        public void Filter(string text = "")
        {
            this._filter = text.ToLower();
            Refresh();
        }

        public void Refresh()
        {
            this._projection = this._itemSource.Where(p => 
                                   this._filter == null
                                || p.Header.ToLower().Contains(this._filter)
                                || p.Detail.ToLower().Contains(this._filter))
                                .ToList();

            this.NotifyDataSetChanged();
        }

        //Wrapper class for adapter for cell re-use
        private class AdapterHelper : Java.Lang.Object
        {
            public TextView Title { get; set; }
        }

        #region implemented abstract members of BaseAdapter
        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return _projection[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            AdapterHelper helper = null;
            if (convertView == null)
            {
                convertView = _activity.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
                helper = new AdapterHelper();
                helper.Title = convertView.FindViewById<TextView>(Android.Resource.Id.Text1);
                convertView.Tag = helper;
            }
            else
            {
                helper = convertView.Tag as AdapterHelper;
            }

            helper.Title.Text = _projection[position].Header;

            return convertView;
        }

        public override int Count
        {
            get
            {
                return this._projection.Count;
            }
        }
        #endregion
    }
}