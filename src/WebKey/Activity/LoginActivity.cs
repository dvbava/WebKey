using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using WebKey.Support;
using Autofac;

namespace WebKey.Activities
{
    [Activity(Label = "WebKey", MainLauncher = true, Theme = "@style/android:Theme.Holo.Light", Icon = "@drawable/icon_1")]
    public class LoginActivity : Activity
    {
        public LoginActivity()
        {
            Startup.Initialize();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Login);

            EnsureRegistration();

            // Create your application here
            EditText txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            Button btnProceed = FindViewById<Button>(Resource.Id.btnProceed);
            Button btnCancel = FindViewById<Button>(Resource.Id.btnCancel);

            this.Title = "Login";

            var encryptor = Startup.Container.Resolve<IEncryptDecrypt>();
            var dataProvider = Startup.Container.Resolve<IDataProvider>();

            btnProceed.Enabled = false;

            txtPassword.AfterTextChanged += (s, e) =>
            {
                btnProceed.Enabled = !string.IsNullOrEmpty(txtPassword.Text) && txtPassword.Text.Length > 3;
            };

            btnProceed.Click += (s, e) =>
            {
                var userPass = txtPassword.Text;

                if (encryptor.ComputeHash(userPass) == dataProvider.GetComputedHash())
                {
                    var homeListActivity = new Intent(Application.Context, typeof(HomeListActivity));
                    encryptor.SetKey(userPass);
                    StartActivity(homeListActivity);
                    Finish();
                }
                else
                {
                    var alertMessage = new AlertDialog.Builder(this);
                    alertMessage.SetMessage("Incorrect password...");
                    alertMessage.SetNeutralButton("Try again", delegate
                    {
                        txtPassword.RequestFocus();
                    });
                    alertMessage.SetNegativeButton("Cancel", delegate
                    {
                        System.Environment.Exit(0);
                    });

                    // Show the alert dialog to the user and wait for response.
                    alertMessage.Show();
                }
            };

            btnCancel.Click += (s, e) =>
            {
                System.Environment.Exit(0);
            };
        }

        private void EnsureRegistration()
        {
            var dataProvider = new DataProvider(null);

            if (!dataProvider.IsolatedStorageFileExists(Constants.LoginHashFile))
            {
                var registerActivity = new Intent(Application.Context, typeof(RegisterActivity));
                StartActivity(registerActivity);
                Finish();
            }
        }
    }
}