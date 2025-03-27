using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class PreguntaManager : MonoBehaviour
{
    [Serializable]
    public class Hora
    {
        [Range(0, 23)] public int horas;
        [Range(0, 59)] public int minutos;

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(horas, minutos, 0);
        }
    }

    [Serializable]
    public class Pregunta
    {
        public string textoPregunta;
        public string[] respuestas;
        public Hora horaMostrar;
    }

    public TMP_Text preguntaText;
    public TMP_Text resultadoText;
    public Button[] botonesRespuestas;
    public Pregunta[] bancoPreguntas;
    public GameObject ventanaPrincipal;

    private int preguntaActualIndex = -1;
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string userEmail;
    private bool isFirestoreInitialized = false;

    private void Start()
    {
        FirebaseInitializer firebaseInitializer = FindObjectOfType<FirebaseInitializer>();

        if (firebaseInitializer != null)
        {
            // Esperar a que Firebase esté inicializado
            firebaseInitializer.OnFirestoreInitialized += InitializeFirebase;
        }
        else
        {
            Debug.LogError("No se encontró FirebaseInitializer.");
        }

        for (int i = 0; i < botonesRespuestas.Length; i++)
        {
            int index = i;
            botonesRespuestas[i].onClick.AddListener(() => SeleccionarRespuesta(index));
        }
    }
    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FindObjectOfType<FirebaseInitializer>().GetFirestore();
        userEmail = auth.CurrentUser?.Email;
        isFirestoreInitialized = true;

        Debug.Log("Firestore está disponible.");

        InvokeRepeating("ActualizarPregunta", 0, 10);
    }



    private void FirestoreInitialized()
    {
        db = FindObjectOfType<FirebaseInitializer>().GetFirestore();
        userEmail = FirebaseAuth.DefaultInstance.CurrentUser?.Email;
        isFirestoreInitialized = true;

        Debug.Log("Firestore está disponible.");

        InvokeRepeating("ActualizarPregunta", 0, 10);
    }

    void ActualizarPregunta()
    {
        if (!isFirestoreInitialized) return;

        DateTime horaActual = DateTime.Now;
        Debug.Log("Hora Actual: " + horaActual);

        for (int i = 0; i < bancoPreguntas.Length; i++)
        {
            TimeSpan horaPregunta = bancoPreguntas[i].horaMostrar.ToTimeSpan();
            DateTime horaPreguntaInicio = DateTime.Today.Add(horaPregunta);  // Hora exacta de la pregunta
            DateTime horaPreguntaFin = horaPreguntaInicio.AddMinutes(5);     // Intervalo de 5 minutos después

            Debug.Log($"Revisando pregunta {i}: {bancoPreguntas[i].textoPregunta} - Hora Configurada: {horaPreguntaInicio:HH:mm} - Hora Fin: {horaPreguntaFin:HH:mm} - Hora Actual: {horaActual:HH:mm}");

            // Verifica si la hora actual está dentro del intervalo de 5 minutos
            if (horaActual >= horaPreguntaInicio && horaActual <= horaPreguntaFin)
            {
                if (preguntaActualIndex != i)
                {
                    // Verificar si la pregunta ya fue respondida
                    VerificarPreguntaRespondida(i);
                    break;  // Solo muestra una pregunta a la vez
                }
            }
        }
    }
    void VerificarPreguntaRespondida(int index)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("Usuario no autenticado.");
            return;
        }

        // Obtenemos el ID de la pregunta
        int preguntaId = index + 1;

        // Referencia al documento donde se guarda el estado de las respuestas
        var docRef = db.Collection("respuestas")
                       .Document(userEmail)
                       .Collection("estadoPreguntas")
                       .Document(preguntaId.ToString());

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener el estado de la pregunta: " + task.Exception);
            }
            else
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    bool respondida = snapshot.GetValue<bool>("respondida");

                    if (respondida)
                    {
                        Debug.Log("La pregunta ya fue respondida, no se mostrará.");
                    }
                    else
                    {
                        Debug.Log("¡Mostrando pregunta: " + bancoPreguntas[index].textoPregunta + "!");
                        MostrarPregunta(index);
                    }
                }
                else
                {
                    // Si no existe el documento, es que nunca se ha respondido, entonces la mostramos
                    MostrarPregunta(index);
                }
            }
        });
    }
    private void VerificarTodasPreguntasRespondidas()
    {
        var estadoCollection = db.Collection("respuestas")
                                 .Document(userEmail)
                                 .Collection("estadoPreguntas");

        estadoCollection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al verificar preguntas: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            int totalRespondidas = 0;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.GetValue<bool>("respondida")) totalRespondidas++;
            }

            // Si todas están respondidas, reiniciar
            if (totalRespondidas >= bancoPreguntas.Length)
            {
                Debug.Log("¡Todas las preguntas completadas! Reiniciando...");
                ResetearEstadoPreguntas(); // <- Nuevo método
            }
        });
    }
    private void ResetearEstadoPreguntas()
    {
        var estadoCollection = db.Collection("respuestas")
                                 .Document(userEmail)
                                 .Collection("estadoPreguntas");

        estadoCollection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener documentos: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                estadoCollection.Document(doc.Id).DeleteAsync();
            }

            Debug.Log("Estado de preguntas reiniciado. Ciclo comenzará de nuevo.");
        });
    }




    void MostrarPregunta(int index)
    {
        if (index < 0 || index >= bancoPreguntas.Length)
        {
            Debug.LogError("Índice de pregunta fuera de rango: " + index);
            return;
        }

        preguntaActualIndex = index;

        if (preguntaText != null)
        {
            preguntaText.text = bancoPreguntas[index].textoPregunta;
        }
        else
        {
            Debug.LogError("preguntaText no está asignado.");
        }

        // Desactivar ventanaPrincipal al mostrar la pregunta
        if (ventanaPrincipal != null)
        {
            ventanaPrincipal.SetActive(false);
        }

        for (int i = 0; i < botonesRespuestas.Length; i++)
        {
            if (i < bancoPreguntas[index].respuestas.Length)
            {
                var respuesta = bancoPreguntas[index].respuestas[i];
                if (botonesRespuestas[i] != null)
                {
                    var botonText = botonesRespuestas[i].GetComponentInChildren<TextMeshProUGUI>();

                    if (botonText != null)
                    {
                        botonText.text = respuesta;
                        botonesRespuestas[i].interactable = true;
                    }
                    else
                    {
                        Debug.LogError($"El botón en la posición {i} no tiene un componente Text o TextMeshPro.");
                    }
                }
                else
                {
                    Debug.LogError($"El botón en la posición {i} es null.");
                }
            }
            else
            {
                botonesRespuestas[i].interactable = false;
            }
        }
    }

    void SeleccionarRespuesta(int index)
    {
        if (preguntaActualIndex != -1)
        {
            var respuestaSeleccionada = bancoPreguntas[preguntaActualIndex].respuestas[index];
            Debug.Log("Opción seleccionada: " + respuestaSeleccionada);

            if (resultadoText != null)
            {
                resultadoText.text = "Has seleccionado: " + respuestaSeleccionada;
            }
            else
            {
                Debug.LogError("resultadoText no está asignado.");
            }

            GuardarRespuestaEnFirestore(preguntaActualIndex + 1, respuestaSeleccionada);

            DesactivarRespuestas();
            VerificarTodasPreguntasRespondidas();

            // Activar ventanaPrincipal después de enviar la respuesta
            if (ventanaPrincipal != null)
            {
                ventanaPrincipal.SetActive(true);
            }
        }
    }

    private void GuardarRespuestaEnFirestore(int preguntaId, string respuesta)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("Usuario no autenticado. No se puede guardar la respuesta.");
            return;
        }

        if (db == null)
        {
            Debug.LogError("Firestore no ha sido inicializado correctamente.");
            return;
        }

        var respuestaData = new
        {
            preguntaId = preguntaId,
            respuesta = respuesta,
            timestamp = DateTime.UtcNow
        };

        var docRef = db.Collection("respuestas").Document(userEmail).Collection("respuestasUsuario").Document(Guid.NewGuid().ToString());
        docRef.SetAsync(respuestaData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error guardando respuesta en Firestore: " + task.Exception);
            }
            else
            {
                Debug.Log("Respuesta guardada exitosamente para " + userEmail);

                // Ahora actualizamos el estado de la pregunta a 'respondida'
                ActualizarEstadoPreguntaRespondida(preguntaId);
            }
        });
    }

    private void ActualizarEstadoPreguntaRespondida(int preguntaId)
    {
        var docRef = db.Collection("respuestas")
                       .Document(userEmail)
                       .Collection("estadoPreguntas")
                       .Document(preguntaId.ToString());

        var estadoData = new { respondida = true, timestamp = DateTime.UtcNow };

        docRef.SetAsync(estadoData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error actualizando el estado de la pregunta: " + task.Exception);
            }
            else
            {
                Debug.Log("Estado de la pregunta actualizado a 'respondida' para la pregunta ID: " + preguntaId);
            }
        });
    }


    void DesactivarRespuestas()
    {
        foreach (var boton in botonesRespuestas)
        {
            boton.interactable = false;
        }
    }

    public void Delog()
    {
        if (auth != null)
        {
            auth.SignOut();
        }

        // Limpia las instancias de Firebase
       

        SceneManager.LoadScene("login");
    }
}
