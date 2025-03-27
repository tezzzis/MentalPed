using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.SceneManagement;
using static PreguntaManager;

public class PreguntaManager1 : MonoBehaviour
{
    public TMP_Text preguntaText;
    public TMP_Text resultadoText;
    public Button[] botonesRespuestas;
    public Pregunta[] bancoPreguntas;
    public GameObject ventanaPrincipal;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string userEmail;
    private List<string> shuffledQuestionIds;
    private string currentWeekKey;
    private Pregunta preguntaActual;
    private DocumentReference weekRef;

    private void Start()
    {
        FirebaseInitializer initializer = FindObjectOfType<FirebaseInitializer>();
        initializer.OnFirestoreInitialized += InitializeFirebase;

        foreach (Button boton in botonesRespuestas)
        {
            boton.onClick.AddListener(() => SeleccionarRespuesta(boton));
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        userEmail = auth.CurrentUser.Email;

        CheckCurrentWeek();
    }

    private void CheckCurrentWeek()
    {
        DateTime now = DateTime.Now;
        int year = now.Year;
        int week = GetIso8601WeekOfYear(now);
        currentWeekKey = $"{year}-W{week}";

        DocumentReference weekRef = db.Collection("users").Document(userEmail).Collection("semanas").Document(currentWeekKey);

        weekRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.Result.Exists)
            {
                // Nueva semana: barajar preguntas y guardar en Firestore
                shuffledQuestionIds = bancoPreguntas.Select(q => q.id).ToList();
                ShuffleQuestions(shuffledQuestionIds);

                Dictionary<string, object> weekData = new Dictionary<string, object>
                {
                    { "preguntasBarajadas", shuffledQuestionIds },
                    { "respuestas", new Dictionary<string, string>() }
                };

                weekRef.SetAsync(weekData).ContinueWithOnMainThread(t =>
                {
                    CheckCurrentDayQuestion();
                });
            }
            else
            {
                // Cargar preguntas barajadas de la semana actual
                shuffledQuestionIds = task.Result.GetValue<List<string>>("preguntasBarajadas");
                CheckCurrentDayQuestion();
            }
        });
    }

    private void CheckCurrentDayQuestion()
    {
        DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
        int dayIndex = (dayOfWeek == DayOfWeek.Sunday) ? 6 : (int)dayOfWeek - 1;

        if (dayIndex >= shuffledQuestionIds.Count)
        {
            Debug.LogError("No hay preguntas disponibles para hoy.");
            return;
        }

        string questionId = shuffledQuestionIds[dayIndex];
        preguntaActual = bancoPreguntas.FirstOrDefault(q => q.id == questionId);

        weekRef = db.Collection("users").Document(userEmail).Collection("semanas").Document(currentWeekKey);
        weekRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al verificar respuesta.");
                return;
            }

            var respuestas = task.Result.GetValue<Dictionary<string, string>>("respuestas");
            if (respuestas != null && respuestas.ContainsKey(questionId))
            {
                Debug.Log("Ya respondió la pregunta de hoy.");
                ventanaPrincipal.SetActive(true);
            }
            else
            {
                MostrarPregunta(preguntaActual);
            }
        });
    }

    private void MostrarPregunta(Pregunta pregunta)
    {
        ventanaPrincipal.SetActive(false);
        preguntaText.text = pregunta.textoPregunta;

        for (int i = 0; i < botonesRespuestas.Length; i++)
        {
            if (i < pregunta.respuestas.Length)
            {
                botonesRespuestas[i].GetComponentInChildren<TMP_Text>().text = pregunta.respuestas[i];
                botonesRespuestas[i].gameObject.SetActive(true);
            }
            else
            {
                botonesRespuestas[i].gameObject.SetActive(false);
            }
        }
    }

    private void SeleccionarRespuesta(Button boton)
    {
        string respuesta = boton.GetComponentInChildren<TMP_Text>().text;

        // Guardar respuesta en Firestore
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"respuestas.{preguntaActual.id}", respuesta }
        };

        weekRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            resultadoText.text = "Respuesta registrada: " + respuesta;
            ventanaPrincipal.SetActive(true);
        });
    }

    // Utilidad: Barajar preguntas
    private void ShuffleQuestions(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // Utilidad: Obtener semana ISO 8601
    private int GetIso8601WeekOfYear(DateTime time)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    public void CerrarSesion()
    {
        auth.SignOut();
        SceneManager.LoadScene("Login");
    }
}