global.eggshell = global.eggshell || {};
global.eggshell.AppEggShellGallery = {}
global.eggshell.AppEggShellGallery.configSourceOverrides = {}

global.eggshell.AppEggShellGallery.configSourceOverrides.AppUrlBase                         = "http://eggshell.app";
global.eggshell.AppEggShellGallery.configSourceOverrides.BackendUrl                         = "https://api.eggshell.app";
global.eggshell.AppEggShellGallery.configSourceOverrides.OtpResendTimeoutSeconds            = "30";
global.eggshell.AppEggShellGallery.configSourceOverrides.InitializeReactXPInDevMode         = "false";
global.eggshell.AppEggShellGallery.configSourceOverrides.InitializeReactXPInDebugMode       = "false";

// Google Maps
global.eggshell.AppEggShellGallery.configSourceOverrides.GoogleMapsApiKey                   = "YOUR_KEY_HERE";

// Google ReCaptcha
global.eggshell.AppEggShellGallery.configSourceOverrides.ReCaptchaSiteKey                   = "YOUR_KEY_HERE"

// Application Insight
global.eggshell.AppEggShellGallery.configSourceOverrides.MaybeAppInsightsInstrumentationKey = "YOUR_APP_INSIGHTS_KEY_GUID";
global.eggshell.AppEggShellGallery.configSourceOverrides.MaybeAppInsightsCloudRole          = "EggShell.Native";

global.eggshell.AppEggShellGallery.configSourceOverrides.AuthRedirectAllowRegex             = "^http://localhost.*$"

// Firebase analytics Web configuration
// Native app configuration is located at android/app/google-services.json
// Make sure that setting file is also updated
global.eggshell.AppEggShellGallery.configSourceOverrides.MaybeFirebaseApiKey              = "YOUR_KEY_HERE"
global.eggshell.AppEggShellGallery.configSourceOverrides.MaybeFirebaseAppId               = "REMOVED"
global.eggshell.AppEggShellGallery.configSourceOverrides.MaybeFirebaseMeasurementId       = "REMOVED"
global.eggshell.AppEggShellGallery.configSourceOverrides.MaybeFirebaseProjectId           = "REMOVED"