global.eggshell = global.eggshell || {};
global.eggshell.AppPerformancePlayground = {}
global.eggshell.AppPerformancePlayground.configSourceOverrides = {}

eggshell.AppPerformancePlayground.configSourceOverrides.AppUrlBase                                = "http://localhost:8100";
eggshell.AppPerformancePlayground.configSourceOverrides.BackendUrl                                = "http://localhost:5000";
global.eggshell.AppPerformancePlayground.configSourceOverrides.OtpResendTimeoutSeconds            = "30";
global.eggshell.AppPerformancePlayground.configSourceOverrides.InitializeReactXPInDevMode         = "true";
global.eggshell.AppPerformancePlayground.configSourceOverrides.InitializeReactXPInDebugMode       = "true";
global.eggshell.AppPerformancePlayground.configSourceOverrides.MaybeAppInsightsInstrumentationKey = "YOUR_APP_INSIGHTS_KEY_GUID";
global.eggshell.AppPerformancePlayground.configSourceOverrides.MaybeAppInsightsCloudRole          = "Example.App.Native";