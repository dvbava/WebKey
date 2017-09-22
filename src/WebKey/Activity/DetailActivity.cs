using Android.App;
using Android.Content;
using Android.OS;
using Android.Text.Method;
using Android.Widget;
using Autofac;
using WebKey.ViewModels;
using System;
using System.Linq;

namespace WebKey.Activities
{
    [Activity(Label = "View", Theme = "@style/android:Theme.Holo.Light", Icon = "@drawable/icon_0")]
    public class DetailActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Detail);

            IHomeListViewModel viewModel = Startup.Container.Resolve<IHomeListViewModel>();
            var Id = Intent.GetIntExtra("Id", -1);

            TextView txtDetails = FindViewById<TextView>(Resource.Id.detail_detail);
            Button btnEdit = FindViewById<Button>(Resource.Id.detail_btn_edit);
            Button btnBack = FindViewById<Button>(Resource.Id.detail_btn_back);
            Button btnDelete = FindViewById<Button>(Resource.Id.detail_btn_delete);

            txtDetails.MovementMethod = LinkMovementMethod.Instance;
            var model = viewModel.Items.First(i => i.Id == Id);
            this.Title = model.Header;
            txtDetails.Text = model.Detail;

            btnEdit.Click += (s, e) =>
            {
                var addEditActivity = new Intent(this, typeof(AddEditActivity));
                addEditActivity.PutExtra("Mode", "Edit");
                addEditActivity.PutExtra("Id", Id);
                StartActivity(addEditActivity);
            };

            btnDelete.Click += (s, e) =>
            {
                var alertMessage = new AlertDialog.Builder(this);
                alertMessage.SetMessage("Confirm delete...");
                alertMessage.SetNeutralButton("Confirn", async delegate
                {
                    await viewModel.Delete(Id);
                    Finish();
                });
                alertMessage.SetNegativeButton("Cancel", delegate
                {
                    
                });

                // Show the alert dialog to the user and wait for response.
                alertMessage.Show();
            };

            btnBack.Click += (s, e) =>
            {
                Finish();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            IHomeListViewModel viewModel = Startup.Container.Resolve<IHomeListViewModel>();
            var Id = Intent.GetIntExtra("Id", -1);
            var model = viewModel.Items.First(i => i.Id == Id);

            TextView txtDetails = FindViewById<TextView>(Resource.Id.detail_detail);
            this.Title = model.Header;
            txtDetails.Text = model.Detail;
        }
    }
}