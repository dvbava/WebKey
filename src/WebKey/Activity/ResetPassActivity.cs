using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using WebKey.Support;
using Autofac;
using WebKey.ViewModels;

namespace WebKey.Activities
{
    [Activity(Label = "Reset password", Theme = "@style/android:Theme.Holo.Light", Icon = "@drawable/icon_0")]
    public class ResetPassActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ResetPassActivity);

            // Create your application here
            EditText txtOldPassword = FindViewById<EditText>(Resource.Id.reset_old_password);
            EditText txtPassword = FindViewById<EditText>(Resource.Id.reset_new_password);
            EditText txtConfirmPassword = FindViewById<EditText>(Resource.Id.reset_new_confirm_password);

            Button btnProceed = FindViewById<Button>(Resource.Id.reset_btnProceed);
            Button btnCancel = FindViewById<Button>(Resource.Id.reset_btnCancel);

            var encryptor = Startup.Container.Resolve<IEncryptDecrypt>();
            var dataProvider = Startup.Container.Resolve<IDataProvider>();
            var viewModel = Startup.Container.Resolve<IHomeListViewModel>();

            btnProceed.Enabled = false;

            Func<bool> canEnableProcessButton = () =>
            {
                return
                encryptor.GetKey() == txtOldPassword.Text
                && !string.IsNullOrEmpty(txtPassword.Text)
                && txtPassword.Text.Length > 3
                && txtPassword.Text == txtConfirmPassword.Text;
            };

            txtOldPassword.AfterTextChanged += (s, e) =>
            {
                btnProceed.Enabled = canEnableProcessButton();
            };

            txtPassword.AfterTextChanged += (s, e) =>
            {
                btnProceed.Enabled = canEnableProcessButton();
            };

            txtConfirmPassword.AfterTextChanged += (s, e) =>
            {
                btnProceed.Enabled = canEnableProcessButton();
            };

            btnProceed.Click += async (s, e) =>
            {
                var userPass = txtPassword.Text;
                encryptor.SetKey(userPass);
                dataProvider.SaveComputedHash(encryptor.ComputeHash(userPass), true);
                await viewModel.WriteAll();
                Finish();
            };

            btnCancel.Click += (s, e) =>
            {
                Finish();
            };
        }
    }
}