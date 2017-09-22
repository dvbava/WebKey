using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Autofac;
using WebKey.Models;
using WebKey.ViewModels;
using System;
using System.Linq;

namespace WebKey.Activities
{
    [Activity(Label = "Add", Theme = "@style/android:Theme.Holo.Light", Icon = "@drawable/icon_0")]
    public class AddEditActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.AddEdit);

            IHomeListViewModel viewModel = Startup.Container.Resolve<IHomeListViewModel>();
            var mode = Intent.GetStringExtra("Mode");
            var Id = Intent.GetIntExtra("Id", -1);

            EditText txtHeader = FindViewById<EditText>(Resource.Id.add_header);
            EditText txtDetails = FindViewById<EditText>(Resource.Id.add_detail);
            Button btnAdd = FindViewById<Button>(Resource.Id.add_btn_save);
            Button btnBack = FindViewById<Button>(Resource.Id.add_btn_back);

            if (mode == "Edit")
            {
                this.Title = "Edit";
                var item = viewModel.Items.First(i => i.Id == Id);
                txtHeader.Text = item.Header;
                txtDetails.Text = item.Detail;
            }

            Func<bool> canEnableProcessButton = () =>
            {
                var header = txtHeader.Text.Trim();
                var detail = txtDetails.Text.Trim();
                return detail.Length > 2 && header.Length > 2 && !viewModel.Items.Any(i => i.Header == header);
            };

            btnAdd.Enabled = canEnableProcessButton();

            txtHeader.AfterTextChanged += (s, e) =>
            {
                btnAdd.Enabled = canEnableProcessButton();
            };

            txtDetails.AfterTextChanged += (s, e) =>
            {
                btnAdd.Enabled = canEnableProcessButton();
            };

            btnAdd.Click += async (s, e) =>
            {
                if (mode == "Add")
                    await viewModel.Add(new KeyEntry { Header = txtHeader.Text.Trim(), Detail = txtDetails.Text });

                if (mode == "Edit")
                    await viewModel.Update(new KeyEntry { Id = Id, Header = txtHeader.Text.Trim(), Detail = txtDetails.Text });

                Finish();
            };

            btnBack.Click += (s, e) =>
            {
                Finish();
            };
        }
    }
}
