using System;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Messaging;
using Firebase.Auth;

public class FirebaseInitializer : MonoBehaviour
{
    private FirebaseFirestore db;

    public event Action OnFirestoreInitialized;

    void Start()
    {
        InitializeFirebase();
    }
    /*void Awake()
    {
        DontDestroyOnLoad(gameObject); 
    }*/
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("No se pudo inicializar Firebase: " + task.Exception);
                return; 
            }

          
            FirebaseApp app = FirebaseApp.DefaultInstance;
            Debug.Log("Firebase inicializado correctamente.");

            InitializeFirestore();
            InitializeFCM();
        });
    }

    private void InitializeFirestore()
    {
        try
        {
            db = FirebaseFirestore.GetInstance(FirebaseApp.DefaultInstance);

            if (db != null)
            {
                Debug.Log("Firestore inicializado correctamente.");
                OnFirestoreInitialized?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error inicializando Firestore: {ex.Message}");
        }
    }
    private void InitializeFCM()
    {
        // Recibir el token FCM para enviar notificaciones push
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        // Intentar obtener el token de FCM
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                string token = task.Result;
                Debug.Log("FCM Token recibido: " + token);
               
            }
            else
            {
                Debug.LogError("No se pudo obtener el token de FCM: " + task.Exception);
            }
        });
    }
    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Token FCM recibido: " + token.Token);

        
        string userEmail = FirebaseAuth.DefaultInstance.CurrentUser?.Email;
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("No se encontr� un usuario autenticado.");
            return;
        }

       
        GuardarTokenEnFirestore(userEmail, token.Token);
    }
    private void GuardarTokenEnFirestore(string userEmail, string token)
{
    // Crea un objeto con el token y la fecha de creacion
    var tokenData = new
    {
        token = token,
        timestamp = DateTime.UtcNow
    };

    // Referencia al documento de Firestore
    var docRef = FirebaseFirestore.DefaultInstance
        .Collection("users")  
        .Document(userEmail);     

    // Guardamos el token
    docRef.SetAsync(tokenData).ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Error al guardar el token en Firestore: " + task.Exception);
        }
        else
        {
            Debug.Log("Token FCM guardado correctamente para el usuario: " + userEmail);
        }
    });
}


    
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Mensaje recibido: " + e.Message);
    }

    public FirebaseFirestore GetFirestore()
    {
        if (db == null)
        {
            Debug.LogError("Firestore no ha sido inicializado. Aseg�rate de que Firebase est� configurado correctamente.");
        }
        return db; 
    }
    void OnDestroy()
    {
    }
}
