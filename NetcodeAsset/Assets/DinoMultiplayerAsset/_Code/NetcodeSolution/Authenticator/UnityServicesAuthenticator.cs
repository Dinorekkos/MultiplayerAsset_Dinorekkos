using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Dino.MultiplayerAsset
{
    public static class UnityServicesAuthenticator 
    {
        #region private properties
        private const int initTimeout = 10000;
        private static bool IsSignedIn = false;
        #endregion

        #region public methods

        

        public static async Task<bool> TryInitServicesAsync(string profileName = null)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized) return true;
            
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                var task = WaitForInitialized();
                if (await Task.WhenAny(task, Task.Delay(initTimeout)) != task)
                    return false; // Timeout reached

                return UnityServices.State == ServicesInitializationState.Initialized;
            }
            
            if (profileName != null)
            {
                //ProfileNames can't contain non-alphanumeric characters
                Regex rgx = new Regex("[^a-zA-Z0-9 - _]");
                profileName = rgx.Replace(profileName, "");
                var authProfile = new InitializationOptions().SetProfile(profileName);

                //If you are using multiple unity services, make sure to initialize it only once before using your services.
                await UnityServices.InitializeAsync(authProfile);
            }
            else
                await UnityServices.InitializeAsync();

            return UnityServices.State == ServicesInitializationState.Initialized;

            async Task WaitForInitialized()
            {
                while (UnityServices.State != ServicesInitializationState.Initialized)
                    await Task.Delay(100);
            }

        }
        
        public static async Task<bool> TrySignInAsync(string profileName = null)
        {
            if (!await TryInitServicesAsync(profileName))
                return false;
            if (IsSignedIn)
            {
                var task = WaitForSignedIn();
                if (await Task.WhenAny(task, Task.Delay(initTimeout)) != task)
                    return false; //timed out
                return AuthenticationService.Instance.IsSignedIn;
            }

            IsSignedIn = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            IsSignedIn = false;

            return AuthenticationService.Instance.IsSignedIn;

            async Task WaitForSignedIn()
            {
                while (!AuthenticationService.Instance.IsSignedIn)
                    await Task.Delay(100);
            }
        }

        #endregion
    
    }
}