using WebKey.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace WebKey.ViewModels
{
    public interface IHomeListViewModel
    {
        Task Refresh();
        Task Add(KeyEntry data);
        Task Add(List<KeyEntry> importList);
        Task Delete(int id);
        ObservableCollection<KeyEntry> Items { get; set; }
        Task WriteAll();
        Task Update(KeyEntry data);
    }

    public class HomeListViewModel : ViewModelBase, IHomeListViewModel
    {
        IDataProvider _dataProvider = null;

        public HomeListViewModel(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            Items = new ObservableCollection<KeyEntry>();
        }

        public ObservableCollection<KeyEntry> Items { get; set; }

        public async Task Refresh()
        {
            Items.Clear();
            foreach (var item in await _dataProvider.GetAll())
            {
                Items.Add(item);
            }
        }

        public async Task WriteAll()
        {
            await _dataProvider.SaveAll(Items);
        }

        public async Task Add(List<KeyEntry> importList)
        {
            foreach (var entry in importList)
            {
                entry.Id = Items.Any() ? Items.Max(i => i.Id) + 1 : 1;
                Items.Insert(0, entry);
            }
            await _dataProvider.SaveAll(Items);
        }

        public async Task Add(KeyEntry data)
        {
            data.Id = Items.Any() ? Items.Max(i => i.Id) + 1 : 1;
            Items.Insert(0, data);
            await _dataProvider.SaveAll(Items);
        }

        public async Task Delete(int id)
        {
            var item = Items.First(i => i.Id == id);
            Items.Remove(item);
            await _dataProvider.SaveAll(Items);
        }

        public async Task Update(KeyEntry data)
        {
            var item = Items.First(i => i.Id == data.Id);
            item.Header = data.Header;
            item.Detail = data.Detail;

            OnPropertyChanged("Items");

            await _dataProvider.SaveAll(Items);
        }
    }
}