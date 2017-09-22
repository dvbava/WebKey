using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using WebKey.Support;

namespace WebKey.Activities
{
    [Activity(Label = "Register", Theme = "@style/android:Theme.Holo.Light", Icon = "@drawable/icon_0")]
    public class RegisterActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Register);

            // Create your application here
            EditText txtPassword = FindViewById<EditText>(Resource.Id.reg_txtPassword);
            EditText txtConfirmPassword = FindViewById<EditText>(Resource.Id.reg_textPasswordConfirm);
            Button btnProceed = FindViewById<Button>(Resource.Id.reg_btnProceed);
            Button btnCancel = FindViewById<Button>(Resource.Id.reg_btnCancel);
            var encryptor = new EncryptDecrypt();
            var dataProvider = new DataProvider(null);

            btnProceed.Enabled = false;

            txtPassword.AfterTextChanged += (s, e) =>
            {
                btnProceed.Enabled = !string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text.Length > 3 && txtPassword.Text == txtConfirmPassword.Text;
            };

            txtConfirmPassword.AfterTextChanged += (s, e) =>
            {
                btnProceed.Enabled = !string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text.Length > 3 && txtPassword.Text == txtConfirmPassword.Text;
            };

            btnProceed.Click += (s, e) =>
            {
                var userPass = txtPassword.Text;
                dataProvider.SaveComputedHash(encryptor.ComputeHash(userPass));

                var loginActivity = new Intent(Application.Context, typeof(LoginActivity));
                StartActivity(loginActivity);
                Finish();
            };

            btnCancel.Click += (s, e) =>
            {
                System.Environment.Exit(0);
            };
        }
    }
}