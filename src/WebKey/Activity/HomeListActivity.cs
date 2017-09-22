using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;
using Autofac;
using Java.IO;
using WebKey.Adapters;
using WebKey.ViewModels;
using Android.Runtime;
using System.Collections.Generic;
using System.Linq;
using WebKey.Models;
using WebKey.Support;
using System.Threading.Tasks;
using System.Threading;

namespace WebKey.Activities
{
    [Activity(Label = "List", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, Theme = "@style/android:Theme.Holo.Light", Icon = "@drawable/icon_0")]
    public class HomeListActivity : ListActivity
    {
        private IHomeListViewModel _viewModel = null;
        private IDataProvider _dataProvider = null;
        private IEncryptDecrypt _encryptDecrypt = null;

        public HomeListActivity()
        {
            _viewModel = Startup.Container.Resolve<IHomeListViewModel>();
            _dataProvider = Startup.Container.Resolve<IDataProvider>();
            _encryptDecrypt = Startup.Container.Resolve<IEncryptDecrypt>();
            _viewModel.Refresh();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.HomeList);

            EditText textSearch = FindViewById<EditText>(Resource.Id.textSearch);
            var homeListAdapter = new HomeListAdapter(this, _viewModel.Items);
            ListAdapter = homeListAdapter;

            ListView.ItemClick += (sender, e) =>
            {
                var detailActivity = new Intent(this, typeof(DetailActivity));
                detailActivity.PutExtra("Id", (int)e.Id);
                StartActivity(detailActivity);
            };

            textSearch.AfterTextChanged += (s, e) =>
            {
                homeListAdapter.Filter(textSearch.Text.Trim());
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.master_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnResume()
        {
            base.OnResume();
            ((BaseAdapter)ListAdapter).NotifyDataSetChanged();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_add:
                    var addEditActivity = new Intent(this, typeof(AddEditActivity));
                    addEditActivity.PutExtra("Mode", "Add");
                    StartActivity(addEditActivity);
                    break;
                case Resource.Id.action_reset_pass:
                    var resetPassActivity = new Intent(this, typeof(ResetPassActivity));
                    StartActivity(resetPassActivity);
                    break;
                case Resource.Id.action_email:
                    SendEmail();
                    break;
                case Resource.Id.action_import:
                    StartImport();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent resultData)
        {
            base.OnActivityResult(requestCode, resultCode, resultData);

            if (requestCode == 1000 && resultCode == Result.Ok)
            {
                if (resultData != null)
                {
                    try
                    {
                        Task.Run(() => {
                            List<KeyEntry> importList = new List<KeyEntry>();
                            using (var fs = new System.IO.StreamReader(ContentResolver.OpenInputStream(resultData.Data)))
                            {
                                while (!fs.EndOfStream)
                                {
                                    var entry = new KeyEntry { Header = this._encryptDecrypt.Decrypt(fs.ReadLine()), Detail = this._encryptDecrypt.Decrypt(fs.ReadLine()), Saved = true };
                                    importList.Add(entry);
                                }
                            }
                            return importList;
                        }).ContinueWith(async (result) => {
                            await this._viewModel.Add(result.Result);
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    catch (System.Security.Cryptography.CryptographicException)
                    {
                        AlertImportError("Current password did not match for selected import file, please reset your correct import password and try again.");
                    }
                    catch (System.Exception ex)
                    {
                        AlertImportError(ex.Message);
                    }
                }
            }
        }

        private void AlertImportError(string message)
        {
            var alertMessage = new AlertDialog.Builder(this);
            alertMessage.SetMessage(message);
            alertMessage.SetTitle("Import Error...");
            alertMessage.SetNeutralButton("OK", delegate { });

            // Show the alert dialog to the user and wait for response.
            alertMessage.Show();
        }

        private void StartImport()
        {
            var intent = new Intent();
            intent.SetType("image/*");
            intent.SetType("text/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), 1000);
        }

        private void SendEmail()
        {
            var filename = _dataProvider.GetDataFilePath();

            File filelocation = new File(filename);
            Uri path = Uri.FromFile(filelocation);
            Intent emailIntent = new Intent(Intent.ActionSend);
            var uri = Android.Net.Uri.FromFile(filelocation);

            filelocation.SetReadable(true, false);
            emailIntent.PutExtra(Android.Content.Intent.ExtraEmail, new string[] { "" });
            emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Web key file " + System.DateTime.Now.ToString("dd-MM-yyyy"));
            emailIntent.PutExtra(Intent.ExtraStream, uri);
            emailIntent.SetType("message/rfc822");
            StartActivity(Intent.CreateChooser(emailIntent, "Send mail..."));
        }
    }
}
