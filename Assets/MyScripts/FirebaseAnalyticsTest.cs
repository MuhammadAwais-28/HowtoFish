using Firebase.Analytics;
using UnityEngine;

public class FirebaseAnalyticsTest : MonoBehaviour
{
    void Start()
    {
        FirebaseAnalytics.LogEvent("test_import_check");
        Debug.Log("[FirebaseAnalyticsTest] FirebaseAnalytics API is available.");
    }
}