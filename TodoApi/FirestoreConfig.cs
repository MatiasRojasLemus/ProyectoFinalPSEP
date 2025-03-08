using System;
using System.IO;
using Google.Cloud.Firestore;

public class FirestoreConfig
{
    private static FirestoreDb firestoreDb;

    public static FirestoreDb InitializeFirestore(){
        if(firestoreDb == null){
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","credenciales.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",path);
            firestoreDb = FirestoreDb.Create("proyectopsep-acda");
            Console.WriteLine("🔥 Firestore inicializado correctamente.");
        }
        return firestoreDb;
    }
}