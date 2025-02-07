using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace UserService.Api.Helper;

public class FirebaseConfig
{
    public static void InitializeFirebase()
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile("tracio-firebase-adminsdk.json")
        });
    }
}